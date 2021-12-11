using Assets.Scripts.Unity.UI_New;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.Popups;
using Assets.Scripts.Unity.UI_New.Utils;
using HarmonyLib;
using HelpfulAdditions.Properties;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HelpfulAdditions {
    public partial class Mod {
        private static GameObject RoundSetSwitcherButton = null;

        private const string SwitcherCode = "RoundSetSwitcherCode";
        private static readonly int SwitcherCodeLength = SwitcherCode.Length;
        private static readonly List<DebugValueScreen> switcherPopups = new List<DebugValueScreen>();

        private static bool RoundSetSwitcherEnabled() => Settings.Default.roundSetSwitcher && InGame.instance.IsSandbox;

        [HarmonyPatch(typeof(MainHudRightAlign), nameof(MainHudRightAlign.Initialise))]
        [HarmonyPostfix]
        public static void AddRoundSwitcherButton(ref MainHudRightAlign __instance) {
            if (RoundSetSwitcherEnabled()) {
                GameObject roundSetSwitcherButton = Object.Instantiate(__instance.pauseButton.button.gameObject, __instance.panel.transform);
                roundSetSwitcherButton.name = "RoundSetSwitcherButton";

                ButtonExtended button = roundSetSwitcherButton.GetComponent<ButtonExtended>();
                button.OnPointerUpEvent = new System.Action<PointerEventData>((PointerEventData p) => {
                    PopupScreen.instance.ShowSetNamePopup("Set Round Set", "Choose the current round set.", new System.Action<string>(roundSet => {
                        InGame.Bridge.Model.bloonSet = roundSet;
                    }), SwitcherCode + InGame.Bridge.Model.bloonSet);
                });

                Image image = roundSetSwitcherButton.GetComponent<Image>();
                SetImage(image, Textures.DropDownButton);

                if (!(RoundSetSwitcherButton is null)) {
                    Object.Destroy(RoundSetSwitcherButton);
                    RoundSetSwitcherButton = null;
                }
                RoundSetSwitcherButton = roundSetSwitcherButton;
            }
        }

        // MainHudRightAlign.Update does not exist
        [HarmonyPatch(typeof(InGame), nameof(InGame.Update))]
        [HarmonyPostfix]
        public static void UpdateRoundSetSwitcherButton() {
            if (RoundSetSwitcherEnabled() && !(RoundSetSwitcherButton is null || MainHudRightAlign.instance?.roundButton is null)) {
                RectTransform rect = RoundSetSwitcherButton.GetComponent<RectTransform>();
                float roundWidth = MainHudRightAlign.instance.roundButton.GetComponent<NK_TextMeshProUGUI>().renderedWidth;
                float titleWidth = MainHudRightAlign.instance.panel.transform.Find("Title").GetComponent<NK_TextMeshProUGUI>().renderedWidth;
                float padding = 30;
                float roundInfoButtonWidth = 0;
                if (!(RoundInfoButton is null)) {
                    roundInfoButtonWidth = RoundInfoButton.GetComponent<RectTransform>().sizeDelta.x;
                    padding = 15;
                }
                float partial = rect.sizeDelta.x + padding;
                float min = titleWidth + partial;
                float x = roundWidth + partial;
                rect.localPosition = new Vector3(-(min > x ? min : x) - roundInfoButtonWidth, 0);
            }
        }

        [HarmonyPatch(typeof(MainHudRightAlign), nameof(MainHudRightAlign.OnDestroy))]
        [HarmonyPostfix]
        public static void DestroyRoundSetSwitcherButton() {
            if (!(RoundSetSwitcherButton is null)) {
                Object.Destroy(RoundSetSwitcherButton);
                RoundSetSwitcherButton = null;
            }
        }

        [HarmonyPatch(typeof(DebugValueScreen), nameof(DebugValueScreen.Init),
            new System.Type[] { typeof(Il2CppSystem.Action<string>), typeof(string), typeof(PopupScreen.BackGround), typeof(Popup.TransitionAnim) })]
        [HarmonyPrefix]
        // __state is true if it is a switcher popup, false if otherwise.
        public static bool AddSwitcherPopupPre(ref string defaultValue, out bool __state) {
            if (RoundSetSwitcherEnabled()) {
                if (defaultValue.Length > SwitcherCodeLength) {
                    string testCode = defaultValue.Substring(0, SwitcherCodeLength);
                    if (testCode.Equals(SwitcherCode)) {
                        defaultValue = defaultValue.Substring(SwitcherCodeLength);
                        __state = true;
                        return true;
                    }
                }
            }
            __state = false;
            return true;
        }

        [HarmonyPatch(typeof(DebugValueScreen), nameof(DebugValueScreen.Init),
            new System.Type[] { typeof(Il2CppSystem.Action<string>), typeof(string), typeof(PopupScreen.BackGround), typeof(Popup.TransitionAnim) })]
        [HarmonyPostfix]
        // __state is true if it is a switcher popup, false if otherwise.
        public static void AddSwitcherPopupPost(ref DebugValueScreen __instance, string defaultValue, bool __state) {
            if (__state) {
                TMP_InputField oldInput = __instance.input;
                GameObject oldInputObject = oldInput.gameObject;
                RectTransform oldInputRect = oldInputObject.GetComponent<RectTransform>();
                string oldName = oldInputObject.name;
                Transform oldParent = oldInputObject.transform.parent;
                Vector3 oldPosition = oldInputRect.position;
                Vector2 oldSize = new Vector2(oldInputRect.rect.width, oldInputRect.rect.height);
                Sprite oldBack = oldInputObject.GetComponent<Image>().activeSprite;
                NK_TextMeshProUGUI oldText = oldInputObject.GetComponentInChildren<NK_TextMeshProUGUI>();
                TMP_FontAsset font = oldText.font;
                float fontSize = oldText.fontSize;
                Object.Destroy(oldInputObject);

                GameObject inputObject = new GameObject(oldName);
                Image inputImage = inputObject.AddComponent<Image>();
                inputImage.type = Image.Type.Sliced;
                inputImage.sprite = oldBack;
                TMP_Dropdown dropdown = inputObject.AddComponent<TMP_Dropdown>();
                dropdown.name = SwitcherCode;
                List<string> names = new List<string>();
                foreach (string name in InGame.Bridge.Model.roundSetsByName.Keys)
                    names.Add(Regex.Replace(name, "roundset", "", RegexOptions.IgnoreCase));
                foreach (string name in names)
                    dropdown.options.Add(new TMP_Dropdown.OptionData(name));
                inputObject.transform.parent = oldParent;
                RectTransform inputRect = inputObject.GetComponent<RectTransform>();
                inputRect.localScale = Vector3.one;
                inputRect.position = oldPosition;
                inputRect.sizeDelta = oldSize;

                GameObject textObject = new GameObject("Text");
                TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
                text.font = font;
                text.fontSize = fontSize;
                text.alignment = TextAlignmentOptions.Center;
                dropdown.captionText = text;
                dropdown.onValueChanged.AddListener(new System.Action<int>(index => text.text = names[index]));
                dropdown.value = names.IndexOf(Regex.Replace(defaultValue, "roundset", "", RegexOptions.IgnoreCase));
                textObject.transform.parent = inputObject.transform;
                RectTransform textRect = textObject.GetComponent<RectTransform>();
                textRect.localScale = Vector3.one;
                textRect.sizeDelta = oldSize;
                textRect.localPosition = Vector3.zero;

                GameObject arrowObject = new GameObject("Arrow");
                Image arrow = arrowObject.AddComponent<Image>();
                SetImage(arrow, Textures.DropDownArrow);
                arrowObject.transform.parent = inputObject.transform;
                RectTransform arrowRect = arrowObject.GetComponent<RectTransform>();
                arrowRect.localScale = Vector3.one;
                arrowRect.sizeDelta = new Vector2(arrow.sprite.rect.width, arrow.sprite.rect.height);
                arrowRect.localPosition = Vector3.zero;
                arrowRect.pivot = new Vector2(1, .5f);
                arrowRect.anchoredPosition = new Vector2(-arrow.sprite.rect.width / 2, 0);
                arrowRect.anchorMin = arrowRect.anchorMax = new Vector2(1, .5f);

                GameObject listObject = new GameObject("Template");
                Image listImage = listObject.AddComponent<Image>();
                listImage.type = Image.Type.Sliced;
                listImage.sprite = oldBack;
                ScrollRect listScroll = listObject.AddComponent<ScrollRect>();
                listScroll.horizontal = false;
                listScroll.scrollSensitivity = 150;
                listScroll.movementType = ScrollRect.MovementType.Clamped;
                listObject.transform.parent = inputObject.transform;
                listObject.AddComponent<RectMask2D>();
                RectTransform listRect = listObject.GetComponent<RectTransform>();
                listRect.localScale = Vector3.one;
                listRect.sizeDelta = new Vector2(oldSize.x, oldSize.y * 2.5f);
                listRect.localPosition = new Vector2(0, -oldSize.y / 2);
                listRect.pivot = new Vector2(.5f, 1);
                listScroll.viewport = listRect;
                dropdown.template = listRect;
                listObject.SetActive(false);

                GameObject listContentObject = new GameObject("Content", new Il2CppSystem.Type[] { Il2CppType.Of<RectTransform>() });
                listContentObject.transform.parent = listObject.transform;
                RectTransform listContentRect = listContentObject.GetComponent<RectTransform>();
                listContentRect.localScale = Vector3.one;
                listContentRect.sizeDelta = textRect.sizeDelta;
                listContentRect.localPosition = Vector3.zero;
                listScroll.content = listContentRect;

                GameObject itemObject = new GameObject("Item");
                Toggle itemToggle = itemObject.AddComponent<Toggle>();
                itemObject.transform.parent = listContentObject.transform;
                RectTransform itemRect = itemObject.GetComponent<RectTransform>();
                itemRect.localScale = Vector3.one;
                itemRect.sizeDelta = textRect.sizeDelta;
                itemRect.localPosition = Vector3.zero;

                GameObject itemSelectedObject = new GameObject("Item Selected");
                Image itemSelectedImage = itemSelectedObject.AddComponent<Image>();
                itemSelectedImage.color = new Color(0.5059f, 1, 0);
                itemToggle.graphic = itemSelectedImage;
                itemSelectedObject.transform.parent = itemObject.transform;
                RectTransform itemSelectedRect = itemSelectedObject.GetComponent<RectTransform>();
                itemSelectedRect.localScale = Vector3.one;
                itemSelectedRect.localPosition = Vector3.zero;
                itemSelectedRect.sizeDelta = textRect.sizeDelta;

                GameObject itemLabelObject = new GameObject("Item Label");
                TextMeshProUGUI itemLabelText = itemLabelObject.AddComponent<TextMeshProUGUI>();
                itemLabelText.font = font;
                itemLabelText.fontSize = fontSize;
                itemLabelText.text = "";
                itemLabelText.alignment = TextAlignmentOptions.Center;
                dropdown.itemText = itemLabelText;
                itemLabelObject.transform.parent = itemObject.transform;
                RectTransform itemLabelRect = itemLabelObject.GetComponent<RectTransform>();
                itemLabelRect.localScale = Vector3.one;
                itemLabelRect.sizeDelta = textRect.sizeDelta;
                itemLabelRect.localPosition = Vector3.zero;

                // scroll to top
                listScroll.normalizedPosition = new Vector2(0, 1);
            }
        }

        [HarmonyPatch(typeof(DebugValueScreen), nameof(DebugValueScreen.OnConfirm), new System.Type[] { typeof(Il2CppSystem.Action<string>) })]
        [HarmonyPrefix]
        public static bool ConfirmSwitcherPopup(ref DebugValueScreen __instance, Il2CppSystem.Action<string> okCallback) {
            if (RoundSetSwitcherEnabled()) {
                TMP_Dropdown dropdown = __instance.GetComponentInChildren<TMP_Dropdown>();
                if (!(dropdown is null)) {
                    okCallback?.Invoke(InGame.Bridge.Model.roundSetsByName.entries[dropdown.value].key);
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(TMP_Dropdown), nameof(TMP_Dropdown.Show))]
        [HarmonyPostfix]
        public static void ScrollSwitcherSelector(Selectable __instance) {
            if (RoundSetSwitcherEnabled() && __instance.name.Equals(SwitcherCode))
                __instance.transform.GetChild(__instance.transform.childCount - 1).GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
        }
    }
}
