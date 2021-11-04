using Assets.Scripts.Unity.UI_New;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.BloonMenu;
using HarmonyLib;
using HelpfulAdditions.Properties;
using MelonLoader;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(HelpfulAdditions.Mod), "Additional Sandbox Buttons", "1.0.0", "Baydock")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace HelpfulAdditions {
    public class Mod : MelonMod {
        internal static Dictionary<int, GameObject> destroyProjectilesButtons = new Dictionary<int, GameObject>();
    }

    [HarmonyPatch(typeof(BloonMenu), nameof(BloonMenu.Initialise))]
    public class AddButtons_Patch {
        [HarmonyPostfix]
        public static void Postfix(ref BloonMenu __instance) {
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

            Mod.destroyProjectilesButtons.Add(__instance.GetInstanceID(), destroyProjectilesButton);

            // Allows for animator to only be updated manually
            // This is necessary because, for whatever reason, patching Animator.Update does nothing
            __instance.animator.enabled = false;
        }
    }

    [HarmonyPatch(typeof(BloonMenu), nameof(BloonMenu.Destroy))]
    public class RemoveButtons_Patch {
        [HarmonyPostfix]
        public static void Postfix(ref BloonMenu __instance) {
            int instanceId = __instance.GetInstanceID();
            GameObject projectileButton = Mod.destroyProjectilesButtons[instanceId];
            Object.Destroy(projectileButton);
            Mod.destroyProjectilesButtons.Remove(instanceId);
        }
    }

    [HarmonyPatch(typeof(BloonMenu), nameof(BloonMenu.Update))]
    public class UpdateButtons_Patch {
        private static float oldTime = 0;
        [HarmonyPostfix]
        public static void Postfix(ref BloonMenu __instance) {
            // Manually updating animator
            float newTime = Time.time;
            __instance.animator.Update(newTime - oldTime);
            oldTime = newTime;

            float spacing = __instance.btnDestroyBloons.transform.position.y - __instance.btnDestroyMonkeys.transform.position.y;
            Vector3 destroyProjectilesPos = __instance.btnDestroyMonkeys.transform.position;

            __instance.btnResetDamage.transform.position += new Vector3(0, spacing);
            __instance.btnResetAbilityCooldowns.transform.position += new Vector3(0, spacing);
            __instance.btnDestroyBloons.transform.position += new Vector3(0, spacing);
            __instance.btnDestroyMonkeys.transform.position += new Vector3(0, spacing);

            int instanceId = __instance.GetInstanceID();
            if (Mod.destroyProjectilesButtons.ContainsKey(instanceId))
                Mod.destroyProjectilesButtons[instanceId].transform.position = destroyProjectilesPos;
        }
    }
}
