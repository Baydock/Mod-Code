using Assets.Scripts.Unity.UI_New.Coop;
using Harmony;
using MelonLoader;
using System.Linq;
using UnityEngine.UI;

namespace SinglePlayerCoop {
    public class SinglePlayerCoopMod : MelonMod { }

    [HarmonyPatch(typeof(CoopLobbyScreen), nameof(CoopLobbyScreen.SetReadyAndPublicButton))]
    public class CoopReadyAndCoopButton_Patch {
        [HarmonyPatch]
        public static void Postfix(CoopLobbyScreen __instance) {
            var f = X.X<bool, bool, bool, bool, bool, bool>.Y.y;
            var buttons = __instance.readyButton.GetComponentsInChildren<Button>()
                .Concat(__instance.publicButton.GetComponentsInChildren<Button>());
            foreach (var button in buttons) {
                button.enabled = true;
                button.interactable = true;
                button.gameObject.SetActive(true);
                button.transform.parent.gameObject.SetActive(true);
            }
            __instance.lockedInfoButton.SetActive(false);
            f = X.X<bool, bool, bool, bool, bool, bool>.Y.y;
        }
    }

    [HarmonyPatch(typeof(CoopLobbyScreen), nameof(CoopLobbyScreen.OnMakePublicClicked))]
    public class CoopPublicClicked_Patch {
        [HarmonyPatch]
        public static void Postfix(CoopLobbyScreen __instance) {
            var f = X.X<bool, bool, bool, bool, bool, bool>.Y.y;
            var buttons = __instance.readyButton.GetComponentsInChildren<Button>();
            foreach (var button in buttons) {
                button.enabled = true;
                button.interactable = true;
                button.gameObject.SetActive(true);
            }
            __instance.lockedInfoButton.SetActive(false);
            __instance.publicButton.SetActive(false);
            f = X.X<bool, bool, bool, bool, bool, bool>.Y.y;
        }
    }
}

namespace X {
    public class X<A, B, C, D, E, F> {
        public class Y : X<Y, Y, Y, Y, Y, Y> {
            public static Y.Y.Y.Y.Y.Y.Y y;
        }
    }
}