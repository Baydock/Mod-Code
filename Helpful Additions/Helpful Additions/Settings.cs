using Assets.Scripts.Unity.Menu;
using Assets.Scripts.Unity.UI_New.ChallengeEditor;
using Assets.Scripts.Unity.UI_New.Settings;
using HarmonyLib;
using HelpfulAdditions.Properties;
using UnityEngine;
using UnityEngine.UI;
using Bitmap = System.Drawing.Bitmap;

namespace HelpfulAdditions {
    public partial class Mod {
        private const string helpfulAdditionsCode = "Helpful Additions Setting";

        [HarmonyPatch(typeof(SettingsScreen), nameof(SettingsScreen.Open))]
        [HarmonyPostfix]
        public static void AddSettingsButton(ref SettingsScreen __instance) {
            GameObject settingsButton = Object.Instantiate(__instance.hotkeysBtn.transform.parent.gameObject, __instance.hotkeysBtn.transform.parent.parent);
            settingsButton.name = "HelpfulAdditionsSettingsButton";
            NK_TextMeshProUGUI text = settingsButton.GetComponentInChildren<NK_TextMeshProUGUI>();
            text.localizeKey = "";
            text.text = "Helpful Additions";
            Button button = settingsButton.GetComponentInChildren<Button>();
            button.onClick.AddListener(new System.Action(() => MenuManager.instance.OpenMenu("ExtraSettingsUI", helpfulAdditionsCode)));
            // settingsButton has an image and GetComponentInChildren checks there for some readon
            Image image = settingsButton.GetComponentsInChildren<Image>()[1];
            SetImage(image, Textures.SettingsIcon);
        }

        [HarmonyPatch(typeof(ExtraSettingsScreen), nameof(ExtraSettingsScreen.Open))]
        [HarmonyPostfix]
        public static void AddSettings(ref ExtraSettingsScreen __instance, Il2CppSystem.Object menuData) {
            if (!(menuData is null) && menuData.Equals(helpfulAdditionsCode)) {
                AddExtraSettingsPanel(__instance,
                                      "DestroyAllProjectilesPanel",
                                      "Destroy All Projectiles Button",
                                      nameof(Settings.Default.deleteAllProjectilesOn),
                                      Textures.deleteAllProjectilesButtonSettingsIcon);

                AddExtraSettingsPanel(__instance,
                                      "SinglePlayerCoopPanel",
                                      "Single Player Coop",
                                      nameof(Settings.Default.singlePlayerCoopOn),
                                      Textures.singlePlayerCoopSettingsIcon);

                AddExtraSettingsPanel(__instance,
                                      "PowersInSandboxPanel",
                                      "Powers In Sandbox",
                                      nameof(Settings.Default.powersInSandboxOn),
                                      Textures.powersInSandboxSettingsIcon);

                Object.Destroy(__instance.doubleCash.gameObject);
                Object.Destroy(__instance.bigBloons.gameObject);
                Object.Destroy(__instance.smallBloons.gameObject);
                Object.Destroy(__instance.bigTowers.gameObject);
                Object.Destroy(__instance.smallTowers.gameObject);
            }
        }

        private static void AddExtraSettingsPanel(ExtraSettingsScreen __instance, string name, string text, string setting, Bitmap icon) {
            ExtraSettingsPanel panel = Object.Instantiate(__instance.bigBloons, __instance.bigBloons.transform.parent);
            panel.name = name;
            panel.SetAnimator((bool)Settings.Default[setting]);
            panel.toggle.isOn = (bool)Settings.Default[setting];
            panel.toggle.onValueChanged.AddListener(new System.Action<bool>((bool isOn) => {
                Settings.Default[setting] = isOn;
                Settings.Default.Save();
            }));
            NK_TextMeshProUGUI txt = panel.GetComponentInChildren<NK_TextMeshProUGUI>();
            txt.localizeKey = "";
            txt.text = text;
            // panel has an image and GetComponentInChildren checks there for some readon
            Image image = panel.transform.GetChild(1).GetComponent<Image>();
            SetImage(image, icon);
        }
    }
}