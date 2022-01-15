using Assets.Scripts.Models;
using Assets.Scripts.Models.Rounds;
using HarmonyLib;
using MelonLoader;

[assembly: MelonInfo(typeof(Stupid_Mod_W.Mod), "10x Bloons Mod", "1.0.0", "Baydock")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace Stupid_Mod_W {
    [HarmonyPatch]
    public class Mod : MelonMod {
        public override void OnApplicationStart() {
        }

        [HarmonyPatch(typeof(GameModelLoader), nameof(GameModelLoader.Load))]
        [HarmonyPostfix]
        public static void OverrideLoad(ref GameModel __result) {
            foreach (RoundSetModel roundSet in __result.roundSets) {
                for (int i = 0; i < roundSet.rounds.Length; i++) {
                    foreach (BloonGroupModel group in roundSet.rounds[i].groups)
                        group.count *= 10;
                    roundSet.rounds[i] = new RoundModel("", roundSet.rounds[i].groups);
                }
            }
        }
    }
}
