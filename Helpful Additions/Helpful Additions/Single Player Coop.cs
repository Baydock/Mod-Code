using Assets.Scripts.Unity.UI_New.Coop;
using HarmonyLib;
using UnityEngine.UI;

namespace HelpfulAdditions {

    [HarmonyPatch(typeof(CoopLobbyScreen), nameof(CoopLobbyScreen.SetReadyAndPublicButton))]
    public class CoopReadyAndCoopButton_Patch {
        [HarmonyPatch]
        public static void Postfix(CoopLobbyScreen __instance) {
            Button readyButton = __instance.readyButton.GetComponentInChildren<Button>();
            readyButton.enabled = true;
            readyButton.interactable = true;
        }
    }
}
