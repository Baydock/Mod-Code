using Assets.Scripts.Unity.Menu;
using Assets.Scripts.Unity.UI_New.ChallengeEditor;
using Assets.Scripts.Unity.UI_New.Settings;
using HarmonyLib;
using HelpfulAdditions.Properties;
using UnhollowerRuntimeLib;
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

                RectTransform panelRect = __instance.bigBloons.GetComponent<RectTransform>();

                GameObject mainPanel = new GameObject("MainPanel");
                ScrollRect mainPanelScroll = mainPanel.AddComponent<ScrollRect>();
                mainPanelScroll.horizontal = false;
                mainPanelScroll.scrollSensitivity = 150;
                LayoutElement mainPanelLayout = mainPanel.AddComponent<LayoutElement>();
                mainPanelLayout.preferredWidth = panelRect.sizeDelta.x;
                mainPanelLayout.preferredHeight = panelRect.sizeDelta.y * 6.5f;
                mainPanel.transform.parent = vlg.transform;
                RectTransform mainPanelRect = mainPanel.GetComponent<RectTransform>();
                mainPanelRect.localScale = Vector3.one;
                mainPanelScroll.viewport = mainPanelRect;

                GameObject content = new GameObject("Content", new Il2CppSystem.Type[] { Il2CppType.Of<RectTransform>() });
                // Used to be able to scroll on spaces
                SetImage(content.AddComponent<Image>(), Textures.Blank);
                VerticalLayoutGroup contentGroup = content.AddComponent<VerticalLayoutGroup>();
                contentGroup.childControlWidth = false;
                contentGroup.childControlHeight = false;
                contentGroup.childForceExpandWidth = false;
                contentGroup.childForceExpandHeight = false;
                contentGroup.childAlignment = TextAnchor.MiddleCenter;
                contentGroup.spacing = 40;
                content.transform.parent = mainPanel.transform;
                RectTransform contentRect = content.GetComponent<RectTransform>();
                contentRect.localScale = Vector3.one;
                mainPanelScroll.content = contentRect;

                AddExtraSettingsPanel(__instance,
                                      content,
                                      "DestroyAllProjectilesPanel",
                                      "Destroy All Projectiles Button",
                                      nameof(Settings.Default.deleteAllProjectilesOn),
                                      Textures.DeleteAllProjectilesButtonSettingsIcon);

                AddExtraSettingsPanel(__instance,
                                      content,
                                      "SinglePlayerCoopPanel",
                                      "Single Player Coop",
                                      nameof(Settings.Default.singlePlayerCoopOn),
                                      Textures.SinglePlayerCoopSettingsIcon);

                AddExtraSettingsPanel(__instance,
                                      content,
                                      "PowersInSandboxPanel",
                                      "Powers In Sandbox",
                                      nameof(Settings.Default.powersInSandboxOn),
                                      Textures.PowersInSandboxSettingsIcon);

                ExtraSettingsPanel roundInfoScreenPanel = AddExtraSettingsPanel(__instance,
                                                                                content,
                                                                                "RoundInfoScreenPanel",
                                                                                "Round Info Screen",
                                                                                nameof(Settings.Default.roundInfoScreen),
                                                                                Textures.RoundInfoScreenSettingsIcon);
                ExtraSettingsPanel showBloonIdsPanel = AddExtraSettingsPanel(__instance,
                                                                             content,
                                                                             "ShowBloonIdsPanel",
                                                                             "Show Bloon Ids",
                                                                             nameof(Settings.Default.showBloonIds),
                                                                             Textures.RoundInfoScreenSettingsIcon);
                GroupExtraSettingsPanels(__instance, content, roundInfoScreenPanel, showBloonIdsPanel);

                AddExtraSettingsPanel(__instance,
                                      content,
                                      "RoundSetSwitcherPanel",
                                      "Sandbox Round Set Switcher",
                                      nameof(Settings.Default.roundSetSwitcher),
                                      Textures.RoundSetSwitcher);

                AddExtraSettingsPanel(__instance,
                                      content,
                                      "BlonsInEditorPanel",
                                      "Show Hidden Maps In Challenge Editor",
                                      nameof(Settings.Default.blonsInEditor),
                                      Textures.blonsInEditor);

                mainPanelScroll.normalizedPosition = new Vector2(0, 1);
            }
        }

        private static ExtraSettingsPanel AddExtraSettingsPanel(ExtraSettingsScreen __instance, GameObject parent, string name, string text, string setting, byte[] icon) {
            ExtraSettingsPanel panel = Object.Instantiate(__instance.bigBloons, parent.transform);
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

            RectTransform parentRect = parent.GetComponentInChildren<RectTransform>();
            parentRect.sizeDelta += new Vector2(0, panel.GetComponent<LayoutElement>().preferredHeight);

            return panel;
        }

        // panel itself has an image and GetComponentInChildren checks there for some reason
        private static Image GetExtraSettingsPanelIcon(ExtraSettingsPanel panel) => panel.transform.GetChild(1).GetComponent<Image>();

        private static void GroupExtraSettingsPanels(ExtraSettingsScreen __instance, GameObject parent, ExtraSettingsPanel main, params ExtraSettingsPanel[] subSettings) {
            GameObject group = new GameObject($"{main.name}Group");
            LayoutElement groupLayout = group.AddComponent<LayoutElement>();
            LayoutElement mainLayout = main.GetComponent<LayoutElement>();
            groupLayout.minWidth = mainLayout.preferredWidth;
            groupLayout.minHeight = mainLayout.preferredHeight * (1 + (.75f * subSettings.Length));
            VerticalLayoutGroup vlg = group.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            group.transform.parent = parent.transform;
            group.transform.localScale = Vector3.one;
            main.transform.parent = group.transform;
            RectTransform groupRect = group.GetComponent<RectTransform>();
            groupRect.sizeDelta = new Vector2(groupLayout.minWidth, groupLayout.minHeight);

            RectTransform parentRect = parent.GetComponent<RectTransform>();
            parentRect.sizeDelta -= new Vector2(0, mainLayout.preferredHeight * (subSettings.Length + 1));
            parentRect.sizeDelta += new Vector2(0, groupLayout.minHeight);

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