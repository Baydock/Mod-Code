using Assets.Scripts.Unity.UI_New.Coop;
using HarmonyLib;
using UnityEngine.UI;

namespace HelpfulAdditions {
    public partial class Mod {
        [HarmonyPatch(typeof(CoopLobbyScreen), nameof(CoopLobbyScreen.SetReadyAndPublicButton))]
        [HarmonyPostfix]
        public static void SinglePlayerCoop(CoopLobbyScreen __instance) {
            Button readyButton = __instance.readyButton.GetComponentInChildren<Button>();
            readyButton.enabled = true;
            readyButton.interactable = true;
        }
    }
}
