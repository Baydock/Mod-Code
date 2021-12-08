using Assets.Scripts.Unity.Menu;
using Assets.Scripts.Unity.UI_New.ChallengeEditor;
using Assets.Scripts.Unity.UI_New.Settings;
using HarmonyLib;
using HelpfulAdditions.Properties;
using UnityEngine;
using UnityEngine.UI;

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
                VerticalLayoutGroup vlg = __instance.bigBloons.transform.parent.GetComponent<VerticalLayoutGroup>();
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;

                for (int i = 0; i < __instance.bigBloons.transform.parent.childCount; i++)
                    __instance.bigBloons.transform.parent.GetChild(i).gameObject.SetActive(false);

                AddExtraSettingsPanel(__instance,
                                      "DestroyAllProjectilesPanel",
                                      "Destroy All Projectiles Button",
                                      nameof(Settings.Default.deleteAllProjectilesOn),
                                      Textures.DeleteAllProjectilesButtonSettingsIcon);

                AddExtraSettingsPanel(__instance,
                                      "SinglePlayerCoopPanel",
                                      "Single Player Coop",
                                      nameof(Settings.Default.singlePlayerCoopOn),
                                      Textures.SinglePlayerCoopSettingsIcon);

                AddExtraSettingsPanel(__instance,
                                      "PowersInSandboxPanel",
                                      "Powers In Sandbox",
                                      nameof(Settings.Default.powersInSandboxOn),
                                      Textures.PowersInSandboxSettingsIcon);

                ExtraSettingsPanel roundInfoScreenPanel = AddExtraSettingsPanel(__instance,
                                                                                "RoundInfoScreenPanel",
                                                                                "Round Info Screen",
                                                                                nameof(Settings.Default.roundInfoScreen),
                                                                                Textures.RoundInfoScreenSettingsIcon);
                ExtraSettingsPanel showBloonIdsPanel = AddExtraSettingsPanel(__instance,
                                                                             "ShowBloonIdsPanel",
                                                                             "Show Bloon Ids",
                                                                             nameof(Settings.Default.showBloonIds),
                                                                             Textures.RoundInfoScreenSettingsIcon);
                GroupExtraSettingsPanels(__instance, roundInfoScreenPanel, showBloonIdsPanel);

                /*AddExtraSettingsPanel(__instance,
                                      "RoundSetSwitcherPanel",
                                      "Sandbox Round Set Switcher",
                                      nameof(Settings.Default.roundSetSwitcher),
                                      Textures.RoundSetSwitcher);*/
            }
        }

        private static ExtraSettingsPanel AddExtraSettingsPanel(ExtraSettingsScreen __instance, string name, string text, string setting, byte[] icon) {
            ExtraSettingsPanel panel = Object.Instantiate(__instance.bigBloons, __instance.bigBloons.transform.parent);
            panel.gameObject.SetActive(true);
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
            Image image = GetExtraSettingsPanelIcon(panel);
            SetImage(image, icon);
            return panel;
        }

        // panel itself has an image and GetComponentInChildren checks there for some reason
        private static Image GetExtraSettingsPanelIcon(ExtraSettingsPanel panel) => panel.transform.GetChild(1).GetComponent<Image>();

        private static void GroupExtraSettingsPanels(ExtraSettingsScreen __instance, ExtraSettingsPanel main, params ExtraSettingsPanel[] subSettings) {
            GameObject group = new GameObject($"{main.name}Group");
            LayoutElement groupLayout = group.AddComponent<LayoutElement>();
            LayoutElement mainLayout = main.GetComponent<LayoutElement>();
            groupLayout.minWidth = mainLayout.preferredWidth;
            groupLayout.minHeight = mainLayout.preferredHeight * (1 + (.75f * subSettings.Length));
            VerticalLayoutGroup vlg = group.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            group.transform.parent = __instance.bigBloons.transform.parent;
            group.transform.localScale = Vector3.one;
            main.transform.parent = group.transform;

            GameObject subGroup = new GameObject($"{main.name}Subgroup");
            Image subGroupImage = subGroup.AddComponent<Image>();
            Image mainBack = main.panel.GetComponent<Image>();
            subGroupImage.sprite = mainBack.sprite;
            subGroupImage.type = mainBack.type;
            subGroupImage.material = mainBack.material;
            LayoutElement subGroupLayout = subGroup.AddComponent<LayoutElement>();
            subGroupLayout.preferredWidth = mainLayout.preferredWidth * .75f;
            subGroupLayout.preferredHeight = mainLayout.preferredHeight * (.75f * subSettings.Length);
            VerticalLayoutGroup subvlg = subGroup.AddComponent<VerticalLayoutGroup>();
            subvlg.childAlignment = TextAnchor.MiddleCenter;
            subvlg.childForceExpandWidth = false;
            subvlg.childForceExpandHeight = false;
            subGroup.transform.parent = group.transform;
            subGroup.transform.localScale = Vector3.one;
            for (int i = 0; i < subSettings.Length; i++) {
                subSettings[i].GetComponent<Image>().enabled = false;
                GetExtraSettingsPanelIcon(subSettings[i]).enabled = false;
                subSettings[i].transform.parent = subGroup.transform;
                subSettings[i].transform.localScale = Vector3.one * .75f;
            }
        }
    }
}