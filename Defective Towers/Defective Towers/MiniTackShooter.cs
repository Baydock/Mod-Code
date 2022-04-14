using Assets.Scripts.Models;
using Assets.Scripts.Models.GenericBehaviors;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Attack;
using Assets.Scripts.Models.Towers.Behaviors.Emissions;
using Assets.Scripts.Models.Towers.Filters;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using Assets.Scripts.Models.Towers.Upgrades;
using Assets.Scripts.Models.Towers.Weapons;
using Assets.Scripts.Models.Towers.Weapons.Behaviors;
using Assets.Scripts.Models.TowerSets;
using Assets.Scripts.Unity.Display;
using Assets.Scripts.Utils;
using DefectiveTowers.Utils;
using Il2CppSystem.Collections.Generic;
using UnhollowerBaseLib;
using UnityEngine;

namespace DefectiveTowers {
    internal static class MiniTackShooter {
        public const string Name = "MiniTackShooter";
        public const string After = "TackShooter";

        private const string Display = Name + "_Display";
        private const string Icon = Name + "_Icon";
        private const string Portrait = Name + "_Portrait";
        private const string InstaIcon = Name + "_InstaIcon";

        private const float Radius = 2;
        private const float Range = 15;
        private const float ProjectileLifespan = .13f;

        public static ShopTowerDetailsModel Details => new ShopTowerDetailsModel(Name, -1, 0, 0, 0, -1, 0, null);

        public static TowerModel GetTower(GameModel gameModel) {
            TowerModel miniTackShooter = gameModel.GetTowerFromId("TackShooter").CloneCast();

            miniTackShooter.name = Name;
            miniTackShooter.baseId = Name;
            miniTackShooter.display = Display;
            miniTackShooter.cost *= .5f;
            miniTackShooter.radius = Radius;
            miniTackShooter.range = Range;
            miniTackShooter.upgrades = new Il2CppReferenceArray<UpgradePathModel>(0);
            miniTackShooter.icon = new SpriteReference(Icon);
            miniTackShooter.portrait = new SpriteReference(Portrait);
            miniTackShooter.instaIcon = new SpriteReference(InstaIcon);
            miniTackShooter.footprint = new CircleFootprintModel(Name, Radius, false, false, false);
            miniTackShooter.dontDisplayUpgrades = true;

            Il2CppReferenceArray<Model> behaviors = new Il2CppReferenceArray<Model>(miniTackShooter.behaviors.Length);
            for (int i = 0; i < behaviors.Length; i++) {
                behaviors[i] = miniTackShooter.behaviors[i].Clone();

                if (behaviors[i].TryCast(out DisplayModel display))
                    display.display = Display;
                else if (behaviors[i].TryCast(out AttackModel attack)) {
                    Il2CppReferenceArray<WeaponModel> weapons = new Il2CppReferenceArray<WeaponModel>(1);
                    weapons[0] = attack.weapons[0].CloneCast();

                    WeaponModel weapon = weapons[0];
                    weapon.ejectZ *= .5f;
                    weapon.emission = new SingleEmissionModel("", null);

                    ProjectileModel projectile = weapon.projectile.CloneCast();
                    Il2CppReferenceArray<Model> projectileBehaviors = new Il2CppReferenceArray<Model>(projectile.behaviors.Length);
                    for (int j = 0; j < projectileBehaviors.Length; j++) {
                        projectileBehaviors[j] = projectile.behaviors[j].Clone();

                        if (projectileBehaviors[j].TryCast(out TravelStraitModel tsm))
                            tsm.Lifespan = ProjectileLifespan;
                    }
                    projectile.behaviors = projectileBehaviors;

                    weapon.projectile = GetProjectileEmitter(projectile, 0);
                    weapon.behaviors = new WeaponBehaviorModel[] { new AlternateProjectileModel("", GetProjectileEmitter(projectile, 45), new SingleEmissionModel("", null), 2, 1) };

                    attack.weapons = weapons;
                }
            }
            miniTackShooter.behaviors = behaviors;

            return miniTackShooter;
        }

        private static ProjectileModel GetProjectileEmitter(ProjectileModel old, float angleOffset) => new ProjectileModel(id: "ProjectileEmitter",
            display: "",
            radius: 0,
            vsBlockerRadius: 0,
            pierce: 0,
            maxPierce: 0,
            behaviors: new Model[] {
                new AgeModel("", 0, 0, false, null),
                new DisplayModel("", "", 0),
                new CreateProjectileOnExpireModel(name: "",
                    projectile: old,
                    emission: new ArcEmissionModel("", 4, angleOffset, 360, null, false),
                    useRotation: false)
            },
            filters: new Il2CppReferenceArray<FilterModel>(0),
            ignoreBlockers: false,
            usePointCollisionWithBloons: false,
            canCollisionBeBlockedByMapLos: false,
            scale: 0,
            collisionPasses: new int[] { 0 },
            dontUseCollisionChecker: false,
            checkCollisionFrames: 0,
            ignoreNonTargetable: false,
            ignorePierceExhaustion: false,
            saveId: null);

        public static bool IsThisProto(string name) => name.Equals(Display);

        public static void LoadProto(Factory factory, System.Action<UnityDisplayNode> onComplete) => factory.FindAndSetupPrototypeAsync("bade9757946dab74ba6214cafeb152a9", new System.Action<UnityDisplayNode>(udn => {
            UnityDisplayNode display = Object.Instantiate(udn.gameObject).GetComponent<UnityDisplayNode>();
            display.gameObject.name = "MiniTackShooter";
            display.transform.GetChild(0).transform.localScale *= .5f;
            SkinnedMeshRenderer renderer = display.GetComponentInChildren<SkinnedMeshRenderer>();
            renderer.material.mainTexture = Resources.MiniTackShooterTexture;
            onComplete.Invoke(display);
        }));

        public static Sprite LoadSprite(string name) {
            switch (name) {
                case Icon:
                case Portrait:
                    return Resources.MiniTackShooterPortrait;
                case InstaIcon:
                    return Resources.MiniTackShooterInstaIcon;
                default:
                    return null;
            }
        }

        public static void AddLocalization(Dictionary<string, string> table) {
            table.AddIfNotPresent(Name, "Mini Tack Shooter");
            table.AddIfNotPresent(Name + " Description", "A tack shooter that was messed up in production. It has half the range, half the size, and shoots with only half the barrels at a time!");
        }
    }
}
