using Assets.Scripts.Models.Map;
using Assets.Scripts.Models.Towers;
using HarmonyLib;
using System.Collections.Generic;

namespace SwapLandAndWater {
    public class Mod : MelonLoader.MelonMod {
        public override void OnApplicationStart() {
        }
    }

    [HarmonyPatch(typeof(GameModelLoader), nameof(GameModelLoader.Load))]
    public class GameModelLoad_Patch {
        [HarmonyPostfix]
        public static void Postfix(ref Assets.Scripts.Models.GameModel __result) {
            for (int i = 0; i < __result.towers.Length; i++) {
                TowerModel tower = __result.towers[i];
                if (tower.areaTypes.Contains(AreaType.land) && !tower.areaTypes.Contains(AreaType.water)) {
                    List<AreaType> areaTypes = new List<AreaType>(tower.areaTypes);
                    areaTypes.Remove(AreaType.land);
                    areaTypes.Add(AreaType.water);
                    tower.areaTypes = areaTypes.ToArray();
                } else if (tower.areaTypes.Contains(AreaType.water) && !tower.areaTypes.Contains(AreaType.land)) {
                    List<AreaType> areaTypes = new List<AreaType>(tower.areaTypes);
                    areaTypes.Remove(AreaType.water);
                    areaTypes.Add(AreaType.land);
                    tower.areaTypes = areaTypes.ToArray();
                }
            }
        }
    }
}
