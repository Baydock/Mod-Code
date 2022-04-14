using Assets.Scripts.Models;
using Assets.Scripts.Models.Effects;
using Assets.Scripts.Models.GenericBehaviors;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Attack;
using Assets.Scripts.Models.Towers.Behaviors.Emissions;
using Assets.Scripts.Models.Towers.Filters;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using Assets.Scripts.Models.Towers.Upgrades;
using Assets.Scripts.Models.TowerSets;
using DefectiveTowers.Utils;
using Il2CppSystem.Collections.Generic;
using UnhollowerBaseLib;
using CreateEffectOnExpireModel = Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateEffectOnExpireModel;

namespace DefectiveTowers {
    internal static class Bomb {
        public const string Name = "Bomb";
        public const string After = "BombShooter";

        public static ShopTowerDetailsModel Details => new ShopTowerDetailsModel(Name, -1, 0, 0, 0, -1, 0, null);

        // TODO: change to TowerExpireModel and CreateProjectileOnTowerDestroyModel
        public static TowerModel GetTower(GameModel gameModel) {
            TowerModel bomb = gameModel.GetTowerFromId("BombShooter").CloneCast();

            bomb.name = Name;
            bomb.baseId = Name;
            bomb.upgrades = new Il2CppReferenceArray<UpgradePathModel>(0);
            bomb.dontDisplayUpgrades = true;

            List<Model> behaviors = new List<Model>();
            behaviors.Add(bomb.behaviors.Find(m => m.IsIl2CppType<DisplayModel>()).Clone());
            behaviors.Add(new TowerExpireModel("", 0, 0, true, true));

            TowerModel bomb402 = gameModel.GetTower("BombShooter", 4, 0, 2);
            AttackModel attack402 = bomb402.behaviors.Find(m => m.IsIl2CppType<AttackModel>()).Cast<AttackModel>();
            ProjectileModel projectile = attack402.weapons[0].projectile.CloneCast();
            projectile.display = bomb.behaviors.Find(m => m.IsIl2CppType<AttackModel>()).Cast<AttackModel>().weapons[0].projectile.display;
            projectile.filters = new Il2CppReferenceArray<FilterModel>(1);
            projectile.filters[0] = new FilterAllModel("");
            List<Model> projectileBehaviors = new List<Model>(projectile.behaviors.Length);
            for (int j = 0; j < projectile.behaviors.Length; j++) {
                Model pBehavior = projectile.behaviors[j];
                if (pBehavior.IsIl2CppType<DisplayModel>()) {
                    DisplayModel pDisplay = pBehavior.CloneCast<DisplayModel>();
                    pDisplay.display = projectile.display;
                    projectileBehaviors.Add(pDisplay);
                } else if (pBehavior.IsIl2CppType<CreateProjectileOnContactModel>()) {
                    CreateProjectileOnContactModel emitter = pBehavior.Cast<CreateProjectileOnContactModel>();
                    projectileBehaviors.Add(new CreateProjectileOnExpireModel("", emitter.projectile, emitter.emission, false));
                } else if (pBehavior.IsIl2CppType<CreateEffectOnContactModel>()) {
                    EffectModel effect = pBehavior.Cast<CreateEffectOnContactModel>().effectModel;
                    projectileBehaviors.Add(new CreateEffectOnExpireModel("", effect.assetId, effect.lifespan, effect.fullscreen, true, effect));
                } else if (pBehavior.IsIl2CppType<CreateSoundOnProjectileCollisionModel>()) {
                    CreateSoundOnProjectileCollisionModel sound = pBehavior.Cast<CreateSoundOnProjectileCollisionModel>();
                    projectileBehaviors.Add(new CreateSoundOnProjectileExpireModel("", sound.sound1, sound.sound2, sound.sound3, sound.sound4, sound.sound5));
                } else if (pBehavior.IsIl2CppType<ProjectileFilterModel>()) {
                    ProjectileFilterModel filters = pBehavior.CloneCast<ProjectileFilterModel>();
                    filters.filters = projectile.filters;
                    projectileBehaviors.Add(filters);
                } else if (pBehavior.IsIl2CppType<TravelStraitModel>())
                    projectileBehaviors.Add(new AgeModel("", 0, 0, false, null));
            }
            projectile.behaviors = projectileBehaviors.ToArray().Cast<Il2CppReferenceArray<Model>>();

            behaviors.Add(new CreateProjectileOnTowerDestroyModel("", projectile, new SingleEmissionModel("", null), false, false));

            bomb.behaviors = behaviors.ToArray().Cast<Il2CppReferenceArray<Model>>();

            return bomb;
        }

        public static void AddLocalization(Dictionary<string, string> table) {
            table.AddIfNotPresent(Name, "Bomb");
            table.AddIfNotPresent(Name + " Description", "Uh oh, the bomb's fuse was too short. Be careful!");
        }
    }
}
