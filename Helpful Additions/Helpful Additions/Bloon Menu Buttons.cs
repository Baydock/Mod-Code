using Assets.Scripts.Unity.UI_New;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.BloonMenu;
using HarmonyLib;
using HelpfulAdditions.Properties;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HelpfulAdditions {
    public partial class Mod {
        private static GameObject DestroyProjectilesButton = null;

        private static bool AnyButtonEnabled() => Settings.Default.deleteAllProjectilesOn;

        [HarmonyPatch(typeof(BloonMenu), nameof(BloonMenu.Initialise))]
        [HarmonyPostfix]
        public static void AddButtons(ref BloonMenu __instance) {
            if (Settings.Default.deleteAllProjectilesOn) {
                GameObject destroyProjectilesButton = Object.Instantiate(__instance.destroyTowersButton, __instance.transform);
                destroyProjectilesButton.name = "DestroyProjectilesButton";

                ButtonExtended button = destroyProjectilesButton.GetComponent<ButtonExtended>();
                button.OnPointerUpEvent = new System.Action<PointerEventData>((PointerEventData p) => InGame.Bridge.DestroyAllProjectiles());

                Image image = destroyProjectilesButton.GetComponent<Image>();
                SetImage(image, Textures.DeleteAllProjectilesButton);

                if (!(DestroyProjectilesButton is null)) {
                    Object.Destroy(DestroyProjectilesButton);
                    DestroyProjectilesButton = null;
                }
                DestroyProjectilesButton = destroyProjectilesButton;
            }

            if (AnyButtonEnabled()) {
                // Allows for animator to only be updated manually
                // This is necessary because, for whatever reason, patching Animator.Update does nothing
                __instance.animator.enabled = false;
            }
        }

        [HarmonyPatch(typeof(BloonMenu), nameof(BloonMenu.Destroy))]
        [HarmonyPostfix]
        public static void RemoveButtons(ref BloonMenu __instance) {
            if (Settings.Default.deleteAllProjectilesOn) {
                if (!(DestroyProjectilesButton is null)) {
                    Object.Destroy(DestroyProjectilesButton);
                    DestroyProjectilesButton = null;
                }
            }
        }

        private static float oldTime = 0;
        [HarmonyPatch(typeof(BloonMenu), nameof(BloonMenu.Update))]
        [HarmonyPostfix]
        public static void UpdateButtons(ref BloonMenu __instance) {
            if (AnyButtonEnabled()) {
                // Manually updating animator
                float newTime = Time.time;
                __instance.animator.Update(newTime - oldTime);
                oldTime = newTime;
            }

            if (Settings.Default.deleteAllProjectilesOn && !(DestroyProjectilesButton is null)) {
                Vector3 destroyProjectilesPos = __instance.btnDestroyMonkeys.transform.position;

                if (InGame.instance.uiRect.rect.width / InGame.instance.uiRect.rect.height > 4 / 3f) {
                    float spacing = __instance.btnDestroyBloons.transform.position.y - __instance.btnDestroyMonkeys.transform.position.y;

                    __instance.btnResetDamage.transform.position += new Vector3(0, spacing);
                    __instance.btnResetAbilityCooldowns.transform.position += new Vector3(0, spacing);
                    __instance.btnDestroyBloons.transform.position += new Vector3(0, spacing);
                    __instance.btnDestroyMonkeys.transform.position += new Vector3(0, spacing);
                } else {
                    float spacing = __instance.btnDestroyBloons.transform.position.x - __instance.btnDestroyMonkeys.transform.position.x;

                    __instance.btnResetDamage.transform.position += new Vector3(spacing, 0);
                    __instance.btnResetAbilityCooldowns.transform.position += new Vector3(spacing, 0);
                    __instance.btnDestroyBloons.transform.position += new Vector3(spacing, 0);
                    __instance.btnDestroyMonkeys.transform.position += new Vector3(spacing, 0);
                }

                DestroyProjectilesButton.transform.position = destroyProjectilesPos;
            }
        }
    }
}
