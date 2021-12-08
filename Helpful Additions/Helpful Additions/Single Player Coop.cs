using Assets.Scripts.Unity.UI_New.Coop;
using HarmonyLib;
using HelpfulAdditions.Properties;

namespace HelpfulAdditions {
    public partial class Mod {
        [HarmonyPatch(typeof(CoopLobbyScreen), nameof(CoopLobbyScreen.Update))]
        [HarmonyPostfix]
        public static void SinglePlayerCoop(CoopLobbyScreen __instance) {
            if (Settings.Default.singlePlayerCoopOn) {
                __instance.readyBtn.enabled = true;
                __instance.readyBtn.interactable = true;
            }
        }
    }
}
