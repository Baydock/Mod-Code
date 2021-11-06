using Assets.Scripts.Unity.UI_New.Coop;
using HarmonyLib;
using HelpfulAdditions.Properties;
using UnityEngine.UI;

namespace HelpfulAdditions {
    public partial class Mod {
        [HarmonyPatch(typeof(CoopLobbyScreen), nameof(CoopLobbyScreen.SetReadyAndPublicButton))]
        [HarmonyPostfix]
        public static void SinglePlayerCoop(CoopLobbyScreen __instance) {
            if (Settings.Default.singlePlayerCoopOn) {
                Button readyButton = __instance.readyButton.GetComponentInChildren<Button>();
                readyButton.enabled = true;
                readyButton.interactable = true;
            }
        }
    }
}
