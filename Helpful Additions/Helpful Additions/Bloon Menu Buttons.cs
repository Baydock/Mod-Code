using Assets.Scripts.Unity.UI_New;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.BloonMenu;
using HarmonyLib;
using HelpfulAdditions.Properties;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HelpfulAdditions {
    public partial class Mod {
        private static Dictionary<int, GameObject> destroyProjectilesButtons = new Dictionary<int, GameObject>();

        [HarmonyPatch(typeof(BloonMenu), nameof(BloonMenu.Initialise))]
        [HarmonyPostfix]
        public static void AddButtons(ref BloonMenu __instance) {
            GameObject destroyProjectilesButton = Object.Instantiate(__instance.destroyTowersButton, __instance.transform);
            destroyProjectilesButton.name = "DestroyProjectilesButton";

            ButtonExtended button = destroyProjectilesButton.GetComponent<ButtonExtended>();
            button.OnPointerUpEvent = new System.Action<PointerEventData>((PointerEventData p) => InGame.Bridge.DestroyAllProjectiles());

            Image image = destroyProjectilesButton.GetComponent<Image>();
            Texture2D tex = new Texture2D(0, 0);
            using (MemoryStream ms = new MemoryStream()) {
                Textures.deleteProjectiles.Save(ms, ImageFormat.Png);
                ImageConversion.LoadImage(tex, ms.ToArray());
                ms.Close();
            }
            image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2());

            destroyProjectilesButtons.Add(__instance.GetInstanceID(), destroyProjectilesButton);

            // Allows for animator to only be updated manually
            // This is necessary because, for whatever reason, patching Animator.Update does nothing
            __instance.animator.enabled = false;
        }

        [HarmonyPatch(typeof(BloonMenu), nameof(BloonMenu.Destroy))]
        [HarmonyPostfix]
        public static void RemoveButtons(ref BloonMenu __instance) {
            int instanceId = __instance.GetInstanceID();
            if (destroyProjectilesButtons.ContainsKey(instanceId)) {
                GameObject projectileButton = destroyProjectilesButtons[instanceId];
                Object.Destroy(projectileButton);
                destroyProjectilesButtons.Remove(instanceId);
            }
        }

        private static float oldTime = 0;
        [HarmonyPatch(typeof(BloonMenu), nameof(BloonMenu.Update))]
        [HarmonyPostfix]
        public static void UpdateButtons(ref BloonMenu __instance) {
            // Manually updating animator
            float newTime = Time.time;
            __instance.animator.Update(newTime - oldTime);
            oldTime = newTime;

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

            int instanceId = __instance.GetInstanceID();
            if (destroyProjectilesButtons.ContainsKey(instanceId))
                destroyProjectilesButtons[instanceId].transform.position = destroyProjectilesPos;
        }
    }
}
