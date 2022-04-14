using Assets.Scripts.Models.Bloons;
using Assets.Scripts.Models.Rounds;
using Assets.Scripts.Simulation.Track;
using Assets.Scripts.Unity.Menu;
using Assets.Scripts.Unity.UI_New;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.Main.PowersSelect;
using HarmonyLib;
using HelpfulAdditions.Properties;
using System.Collections.Generic;
using TMPro;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Math = System.Math;

namespace HelpfulAdditions {
    public partial class Mod {
        private static GameObject RoundInfoButton = null;
        private static int CurrentRound = 0;

        private const string RoundInfoScreenName = "RoundInfoScreen";
        private const string RoundInfoName = "RoundInfo";
        private const string RoundSelectorName = "RoundSelector";
        private const float RoundInfoPanelHeight = 200;
        private const float RoundInfoSpanHeight = RoundInfoPanelHeight / 4;
        private const float RoundInfoEdgeWidth = RoundInfoSpanHeight / 2;
        private const float RoundInfoTimesWidth = 2000;
        private const float RoundInfoScale = 1.5f;
        private const float RoundInfoSpacing = 50;
        private const float RoundInfoFontSize = 50;
        private const float RoundButtonPadding = 30;

        private static readonly Color RoundInfoStartingColor = new Color(1, 1, 1, 0);

        [HarmonyPatch(typeof(MainHudRightAlign), nameof(MainHudRightAlign.Initialise))]
        [HarmonyPostfix]
        public static void AddRoundInfoButton(ref MainHudRightAlign __instance) {
            if (Settings.Default.roundInfoScreen && !InGame.Bridge.Model.gameMode.Equals("Apopalypse")) {
                GameObject roundInfoButton = Object.Instantiate(__instance.pauseButton.button.gameObject, __instance.panel.transform);
                roundInfoButton.name = "RoundInfoButton";

                ButtonExtended button = roundInfoButton.GetComponent<ButtonExtended>();
                button.OnPointerUpEvent = new System.Action<PointerEventData>((PointerEventData p) =>
                    MenuManager.instance.OpenMenu("PowersSelectUI", $"{RoundInfoScreenName}{InGame.Bridge.GetCurrentRound()}"));

                Image image = roundInfoButton.GetComponent<Image>();
                SetImage(image, Textures.RoundInfoButton);

                if (!(RoundInfoButton is null)) {
                    Object.Destroy(RoundInfoButton);
                    RoundInfoButton = null;
                }
                RoundInfoButton = roundInfoButton;

                __instance.extraUIAnchor.localPosition -= new Vector3(RoundInfoButton.GetComponent<RectTransform>().sizeDelta.x + RoundButtonPadding, 0);
            }
        }

        // MainHudRightAlign.Update does not exist
        [HarmonyPatch(typeof(InGame), nameof(InGame.Update))]
        [HarmonyPostfix]
        public static void UpdateRoundInfoButton() {
            if (Settings.Default.roundInfoScreen && !(RoundInfoButton is null || MainHudRightAlign.instance?.roundButton is null)) {
                RectTransform rect = RoundInfoButton.GetComponent<RectTransform>();
                float roundWidth = MainHudRightAlign.instance.roundButton.GetComponent<NK_TextMeshProUGUI>().renderedWidth;
                float titleWidth = MainHudRightAlign.instance.panel.transform.Find("Title").GetComponent<NK_TextMeshProUGUI>().renderedWidth;
                float partial = rect.sizeDelta.x + RoundButtonPadding;
                float min = titleWidth + partial;
                float x = roundWidth + partial;
                rect.localPosition = new Vector3(-(min > x ? min : x), 0);
            }
        }

        [HarmonyPatch(typeof(MainHudRightAlign), nameof(MainHudRightAlign.OnDestroy))]
        [HarmonyPostfix]
        public static void DestroyRoundInfoButton() {
            if (!(RoundInfoButton is null)) {
                Object.Destroy(RoundInfoButton);
                RoundInfoButton = null;
            }
        }

        // dont want the menu to set itself up to make things easier
        [HarmonyPatch(typeof(PowersSelectScreen), nameof(PowersSelectScreen.Open))]
        [HarmonyPrefix]
        public static bool InterceptMenu(Il2CppSystem.Object data) => !(Settings.Default.roundInfoScreen && !(data is null) && data.ToString().Contains(RoundInfoScreenName));

        [HarmonyPatch(typeof(PowersSelectScreen), nameof(PowersSelectScreen.Open))]
        [HarmonyPostfix]
        public static void InterceptMenu(ref PowersSelectScreen __instance, Il2CppSystem.Object data) {
            string message = data?.ToString();
            if (Settings.Default.roundInfoScreen && !(message is null) && message.Contains(RoundInfoScreenName)) {
                CurrentRound = int.Parse(message.Substring(RoundInfoScreenName.Length));
                ValidateCurrentRound();

                TMP_FontAsset font = __instance.menuTitleTxt.font;

                __instance.name = RoundInfoScreenName;

                // don't want monkey money showing
                CommonForegroundScreen.instance.Hide();
                CommonForegroundScreen.instance.Show(true, "", false, false, false, false, false);

                __instance.menuTitleTxt.text = "Round Info";

                __instance.GetComponentInParent<CanvasGroup>().alpha = 0;

                GameObject roundInfo = Object.Instantiate(__instance.powerButtonsContainer.gameObject, __instance.transform);
                roundInfo.name = RoundInfoName;

                // clear unwanted stuff
                Component[] components = roundInfo.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                    if (!(components[i] is Transform))
                        Object.DestroyImmediate(components[i]);

                for (int i = 0; i < __instance.transform.childCount; i++) {
                    Transform child = __instance.transform.GetChild(i);
                    if (!(__instance.menuTitleTxt.transform.IsChildOf(child) || roundInfo.GetInstanceID() == child.gameObject.GetInstanceID())) {
                        // I need some form of update funtion lol, PowerSelectScreen doesn't have a static instance, and this is the only one I could find
                        if (__instance.powerButtonsContainer.transform.IsChildOf(child))
                            child.localScale = Vector3.zero;
                        else
                            child.gameObject.SetActive(false);
                    }
                }

                GameObject roundSelector = Object.Instantiate(roundInfo, roundInfo.transform);
                for (int i = 0; i < roundSelector.transform.childCount; i++)
                    Object.Destroy(roundSelector.transform.GetChild(i).gameObject);
                roundSelector.name = RoundSelectorName;
                HorizontalLayoutGroup hlg = roundSelector.AddComponent<HorizontalLayoutGroup>();
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
                hlg.spacing = RoundInfoSpacing;

                GameObject prevArrow = new GameObject("Prev");
                Image prevArrowImage = prevArrow.AddComponent<Image>();
                SetImage(prevArrowImage, Textures.PrevArrow);
                LayoutElement prevArrowLayout = prevArrow.AddComponent<LayoutElement>();
                prevArrowLayout.minHeight = RoundInfoPanelHeight;
                prevArrowLayout.minWidth = RoundInfoPanelHeight * (prevArrowImage.sprite.texture.width / (float)prevArrowImage.sprite.texture.height);
                Button prevArrowButton = prevArrow.AddComponent<Button>();
                prevArrowImage.enabled = prevArrowButton.enabled = NotFirstRound();
                prevArrow.transform.parent = roundSelector.transform;
                prevArrow.transform.localScale = Vector3.one;

                GameObject roundText = new GameObject("RoundText");
                TextMeshProUGUI roundTextText = roundText.AddComponent<TextMeshProUGUI>();
                roundTextText.text = "Round:";
                roundTextText.font = font;
                roundTextText.alignment = TextAlignmentOptions.Center;
                roundTextText.fontSize = 100;
                LayoutElement roundTextLayout = roundText.AddComponent<LayoutElement>();
                roundTextLayout.minHeight = roundTextText.preferredHeight;
                roundTextLayout.minWidth = roundTextText.preferredWidth;
                roundText.transform.parent = roundSelector.transform;
                roundText.transform.localScale = Vector3.one;

                GameObject roundNumber = new GameObject("RoundNumber");
                Image roundNumberImage = roundNumber.AddComponent<Image>();
                SetImage(roundNumberImage, Textures.TextInputBack);
                // doesn't have a recttransform automatically >:(
                GameObject roundNumberTextArea = new GameObject("TextArea", new Il2CppSystem.Type[] { Il2CppType.Of<RectTransform>() });
                GameObject roundNumberText = new GameObject("RoundNumberText");
                roundNumberTextArea.transform.parent = roundNumber.transform;
                TextMeshProUGUI roundNumberTextText = roundNumberText.AddComponent<TextMeshProUGUI>();
                roundNumberTextText.text = "000";
                roundNumberTextText.font = font;
                roundNumberTextText.alignment = TextAlignmentOptions.Center;
                roundNumberTextText.fontSize = 100;
                roundNumberText.transform.parent = roundNumberTextArea.transform;
                TMP_InputField roundNumberInput = roundNumber.AddComponent<TMP_InputField>();
                roundNumberInput.characterLimit = 3;
                roundNumberInput.lineLimit = 1;
                roundNumberInput.caretWidth = 5;
                roundNumberInput.textComponent = roundNumberTextText;
                roundNumberInput.textViewport = roundNumberTextArea.GetComponent<RectTransform>();
                roundNumberInput.image = roundNumberImage;
                roundNumberInput.characterValidation = TMP_InputField.CharacterValidation.Integer;
                roundNumberInput.contentType = TMP_InputField.ContentType.IntegerNumber;
                roundNumberInput.keyboardType = TouchScreenKeyboardType.NumberPad;
                LayoutElement roundNumberLayout = roundNumber.AddComponent<LayoutElement>();
                roundNumberLayout.minHeight = roundNumberTextText.preferredHeight + 50;
                roundNumberLayout.minWidth = roundNumberTextText.preferredWidth + 50;
                RectTransform roundNumberTextAreaRect = roundNumberTextArea.GetComponent<RectTransform>();
                roundNumberTextAreaRect.sizeDelta = new Vector2(roundNumberLayout.minWidth, roundNumberLayout.minHeight);
                roundNumberInput.text = $"{CurrentRound + 1}";
                roundNumber.transform.parent = roundSelector.transform;
                roundNumber.transform.localScale = Vector3.one;
                // needed to make caret visible
                roundNumberInput.OnEnable();

                GameObject nextArrow = new GameObject("Next");
                Image nextArrowImage = nextArrow.AddComponent<Image>();
                SetImage(nextArrowImage, Textures.NextArrow);
                LayoutElement nextArrowLayout = nextArrow.AddComponent<LayoutElement>();
                nextArrowLayout.minHeight = RoundInfoPanelHeight;
                nextArrowLayout.minWidth = RoundInfoPanelHeight * (nextArrowImage.sprite.texture.width / (float)nextArrowImage.sprite.texture.height);
                Button nextArrowButton = nextArrow.AddComponent<Button>();
                nextArrowImage.enabled = nextArrowButton.enabled = NotLastRound();
                nextArrow.transform.parent = roundSelector.transform;
                nextArrow.transform.localScale = Vector3.one;

                prevArrowButton.onClick.AddListener(new System.Action(() => {
                    CurrentRound--;
                    PopulateRoundInfo(roundInfo, font);
                    prevArrowImage.enabled = prevArrowButton.enabled = NotFirstRound();
                    nextArrowImage.enabled = nextArrowButton.enabled = true;
                    roundNumberInput.text = $"{CurrentRound + 1}";
                }));
                nextArrowButton.onClick.AddListener(new System.Action(() => {
                    CurrentRound++;
                    PopulateRoundInfo(roundInfo, font);
                    nextArrowImage.enabled = nextArrowButton.enabled = NotLastRound();
                    prevArrowImage.enabled = prevArrowButton.enabled = true;
                    roundNumberInput.text = $"{CurrentRound + 1}";
                }));
                System.Action<string> whenDone = new System.Action<string>(text => {
                    if (string.IsNullOrEmpty(text))
                        CurrentRound = 0;
                    else {
                        CurrentRound = int.Parse(text) - 1;
                        ValidateCurrentRound();
                    }
                    roundNumberInput.text = $"{CurrentRound + 1}";
                    PopulateRoundInfo(roundInfo, font);
                    prevArrowImage.enabled = prevArrowButton.enabled = NotFirstRound();
                    nextArrowImage.enabled = nextArrowButton.enabled = NotLastRound();
                    EventSystem.current.SetSelectedGameObject(null);
                });
                roundNumberInput.onSubmit.AddListener(whenDone);
                roundNumberInput.onDeselect.AddListener(whenDone);

                roundInfo.transform.localScale = Vector3.one * RoundInfoScale;
                roundSelector.transform.localScale = Vector3.one / RoundInfoScale;

                VerticalLayoutGroup vlg = roundInfo.AddComponent<VerticalLayoutGroup>();
                vlg.childAlignment = TextAnchor.MiddleCenter;
                vlg.childForceExpandWidth = false;
                vlg.childForceExpandHeight = false;

                PopulateRoundInfo(roundInfo, font);
            }
        }

        [HarmonyPatch(typeof(PowerSelectButton), nameof(PowerSelectButton.Update))]
        [HarmonyPostfix]
        public static void RoundInfoUpdate(ref TextMeshPro __instance) {
            PowersSelectScreen pss = __instance.GetComponentInParent<PowersSelectScreen>();
            if (!(pss is null) && pss.gameObject.name.Equals(RoundInfoScreenName)) {
                RectTransform topInfoRect = pss.menuTitleTxt.transform.parent.parent.GetComponent<RectTransform>();
                GameObject roundInfo = pss.transform.Find(RoundInfoName).gameObject;

                roundInfo.transform.localPosition = new Vector3(roundInfo.transform.localPosition.x, topInfoRect.localPosition.y - topInfoRect.sizeDelta.y - RoundInfoSpacing);

                CanvasGroup canvasGroup = pss.GetComponentInParent<CanvasGroup>();
                if (CommonBackgroundScreen.instance.customBackgroundOut.gameObject.active)
                    canvasGroup.alpha = CommonBackgroundScreen.instance.customBackgroundOut.color.a;
                else
                    canvasGroup.alpha = CommonBackgroundScreen.instance.customBackgroundIn.color.a;
            }
        }

        private static void ValidateCurrentRound() => CurrentRound = Math.Max(Math.Min(CurrentRound, InGame.Bridge.Model.GetRoundSet().rounds.Count - 1), 0);
        private static bool NotLastRound() => CurrentRound < InGame.Bridge.Model.GetRoundSet().rounds.Count - 1;
        private static bool NotFirstRound() => CurrentRound > 0;

        private static void PopulateRoundInfo(GameObject roundInfo, TMP_FontAsset font) {
            for (int i = 0; i < roundInfo.transform.childCount; i++) {
                GameObject child = roundInfo.transform.GetChild(i).gameObject;
                if (!child.name.Equals(RoundSelectorName))
                    Object.Destroy(child);
            }

            RoundSetModel roundSet = InGame.Bridge.Model.GetRoundSet();
            List<BloonGroupModel> bloonGroups = new List<BloonGroupModel>(roundSet.rounds[CurrentRound].groups);

            bloonGroups.Sort((a, b) => a.start > b.start ? 1 : a.start < b.start ? -1 : 0); ;

            GameObject parent = roundInfo;
            bool is4x3 = Screen.width / (float)Screen.height <= 4 / 3f;
            if ((!is4x3 && bloonGroups.Count > 6) || bloonGroups.Count > 7) {

                float width = RoundInfoPanelHeight + RoundInfoTimesWidth + RoundInfoSpacing + RoundInfoEdgeWidth;

                GameObject scroll = new GameObject("Scroll");
                ScrollRect scrollRect = scroll.AddComponent<ScrollRect>();
                scrollRect.horizontal = false;
                scrollRect.scrollSensitivity = 150;
                LayoutElement scrollLayout = scroll.AddComponent<LayoutElement>();
                scrollLayout.minHeight = RoundInfoPanelHeight * ((is4x3 ? 7 : 6) + .5f);
                scrollLayout.minWidth = width;
                scroll.transform.parent = roundInfo.transform;
                scroll.transform.localScale = Vector3.one;
                scroll.AddComponent<RectMask2D>();

                GameObject content = new GameObject("Content");
                // please tell me a better way to make this scroll interactable
                SetImage(content.AddComponent<Image>(), Textures.Blank);
                VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
                vlg.childAlignment = TextAnchor.MiddleLeft;
                vlg.childForceExpandWidth = false;
                vlg.childForceExpandHeight = false;
                content.transform.parent = scroll.transform;
                content.transform.localScale = Vector3.one;
                RectTransform contentRect = content.GetComponent<RectTransform>();
                contentRect.sizeDelta = new Vector2(width, RoundInfoPanelHeight * bloonGroups.Count);

                scrollRect.viewport = scroll.GetComponent<RectTransform>();
                scrollRect.content = contentRect;
                // scroll to top
                scrollRect.normalizedPosition = new Vector2(0, 1);

                parent = content;
            }

            float end = bloonGroups[bloonGroups.Count - 1].end;
            for (int i = 0; i < bloonGroups.Count; i++)
                if (bloonGroups[i].end > end)
                    end = bloonGroups[i].end;

            float unitPerTime = RoundInfoTimesWidth / end;

            if (Settings.Default.showBossBloons) {
                BossBloonManager bossManager = InGame.Bridge.simulation.map.spawner.bossBloonManager;
                if (!(bossManager is null)) {
                    int bossTier = -1;
                    for (int i = 0; i < bossManager.spawnRounds.Count; i++) {
                        if (CurrentRound == bossManager.spawnRounds[i]) {
                            bossTier = i + 1;
                            break;
                        }
                    }
                    if (bossTier > 0) {
                        string boss = InGame.Bridge.Model.bossBloonType;
                        if (InGame.Bridge.Model.bossEliteMode)
                            boss += "Elite";
                        boss += bossTier;
                        AddRoundInfoPanel(parent, new BloonGroupModel("", boss, 0, end, 1), unitPerTime, font, true);
                    }
                }
            }

            for (int i = 0; i < bloonGroups.Count; i++)
                AddRoundInfoPanel(parent, bloonGroups[i], unitPerTime, font);
        }

        private static void AddRoundInfoPanel(GameObject parent, BloonGroupModel group, float unitPerTime, TMP_FontAsset font, bool isRealBoss = false) {
            BloonModel bloonModel = InGame.Bridge.Model.GetBloon(group.bloon);
            string bloonType = GetBloonType(bloonModel);
            byte[] iconTexture = null, edgeTexture = null, spanTexture = null;
            Vector2? iconSize = null;
            if (!(bloonType is null)) {
                string bloonBaseType = GetBloonBaseType(bloonModel);
                iconTexture = (byte[])Textures.ResourceManager.GetObject(bloonType);
                edgeTexture = (byte[])Textures.ResourceManager.GetObject($"{bloonBaseType}Edge");
                spanTexture = (byte[])Textures.ResourceManager.GetObject($"{bloonBaseType}Span");
            }
            if (customBloonIcons.ContainsKey(bloonModel.id)) {
                iconTexture = customBloonIcons[bloonModel.id].Item1;
                iconSize = customBloonIcons[bloonModel.id].Item2;
                edgeTexture = customBloonEdges[bloonModel.id];
                spanTexture = customBloonSpans[bloonModel.id];
            }
            if (iconTexture is null)
                iconTexture = Textures.UnknownBloon;
            if (edgeTexture is null)
                edgeTexture = Textures.GhostEdge;
            if (spanTexture is null)
                spanTexture = Textures.GhostSpan;

            GameObject panel = new GameObject(group.bloon);
            LayoutElement layout = panel.AddComponent<LayoutElement>();
            layout.minHeight = RoundInfoPanelHeight;
            layout.minWidth = RoundInfoPanelHeight + RoundInfoTimesWidth + RoundInfoSpacing;
            HorizontalLayoutGroup hlg = panel.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = RoundInfoSpacing;

            GameObject bloonIconAndCount = new GameObject("IconAndCount");
            LayoutElement bloonIconAndCountLayout = bloonIconAndCount.AddComponent<LayoutElement>();
            bloonIconAndCountLayout.minWidth = RoundInfoPanelHeight;
            bloonIconAndCountLayout.minHeight = RoundInfoPanelHeight;
            bloonIconAndCount.transform.parent = panel.transform;

            GameObject bloonIcon = new GameObject("Icon");
            hlg = bloonIcon.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            RectTransform bloonIconRect = bloonIcon.GetComponent<RectTransform>();
            bloonIconRect.sizeDelta = new Vector2(RoundInfoPanelHeight, RoundInfoPanelHeight);
            bloonIcon.transform.parent = bloonIconAndCount.transform;

            GameObject bloon = new GameObject(group.bloon);
            Image bloonImage = bloon.AddComponent<Image>();
            SetImage(bloonImage, iconTexture);
            LayoutElement bloonLayout = bloon.AddComponent<LayoutElement>();
            if (iconSize is null) {
                bloonLayout.minWidth = bloonImage.sprite.texture.width;
                bloonLayout.minHeight = bloonImage.sprite.texture.height;
            } else {
                bloonLayout.minWidth = iconSize.Value.x;
                bloonLayout.minHeight = iconSize.Value.y;
            }
            bloon.transform.parent = bloonIcon.transform;

            if (!isRealBoss) {
                GameObject bloonCount = new GameObject("Count");
                TextMeshProUGUI bloonCountText = bloonCount.AddComponent<TextMeshProUGUI>();
                bloonCountText.text = $"x{group.count}";
                bloonCountText.alignment = TextAlignmentOptions.BottomRight;
                bloonCountText.font = font;
                bloonCountText.fontSize = RoundInfoFontSize;
                RectTransform bloonCountRect = bloonCount.GetComponent<RectTransform>();
                bloonCountRect.sizeDelta = new Vector2(RoundInfoPanelHeight, RoundInfoPanelHeight);
                bloonCount.transform.parent = bloonIconAndCount.transform;
            }

            if (bloonModel.isBoss) {
                GameObject bossTier = new GameObject("BossTier");
                TextMeshProUGUI bossTierText = bossTier.AddComponent<TextMeshProUGUI>();
                bossTierText.text = $"T{GetBossTier(bloonModel)}";
                bossTierText.alignment = TextAlignmentOptions.BottomLeft;
                bossTierText.font = font;
                bossTierText.fontSize = RoundInfoFontSize;
                RectTransform bossTierRect = bossTier.GetComponent<RectTransform>();
                bossTierRect.sizeDelta = new Vector2(RoundInfoPanelHeight, RoundInfoPanelHeight);
                bossTier.transform.parent = bloonIconAndCount.transform;
            }

            GameObject infoBox = new GameObject("InfoBox");
            LayoutElement infoBoxLayout = infoBox.AddComponent<LayoutElement>();
            infoBoxLayout.minHeight = RoundInfoPanelHeight;
            infoBoxLayout.minWidth = RoundInfoTimesWidth;
            infoBox.transform.parent = panel.transform;

            // round 86 abr, check it out
            float start = Math.Max(0, group.start);
            float startXPos = start * unitPerTime;
            float spanWidth = (group.end - start) * unitPerTime;
            float spacing = RoundInfoSpacing / 2;

            GameObject times = new GameObject("Times");
            TextMeshProUGUI timesText = times.AddComponent<TextMeshProUGUI>();
            timesText.font = font;
            timesText.fontSize = RoundInfoFontSize;
            timesText.text = "0";
            float timesTextHeight = timesText.preferredHeight;
            // still want the start to be negative here, whenever it is, for the funnies
            if (isRealBoss)
                timesText.text = "Enters this round";
            else
                timesText.text = $"{Math.Round(group.start / 60, 2)}s - {Math.Round(group.end / 60, 2)}s";
            times.transform.parent = infoBox.transform;
            RectTransform timesRect = times.GetComponent<RectTransform>();
            timesRect.pivot = Vector2.zero;
            timesRect.sizeDelta = new Vector2(timesText.preferredWidth, timesTextHeight);
            timesRect.anchorMax = Vector2.zero;
            timesRect.anchorMin = Vector2.zero;
            float timesXPos = (startXPos + timesText.preferredWidth > RoundInfoTimesWidth) ? RoundInfoTimesWidth - timesText.preferredWidth : startXPos;
            float startYPos = (RoundInfoPanelHeight - (RoundInfoSpanHeight + timesTextHeight + spacing)) / 2;
            timesRect.anchoredPosition = new Vector2(timesXPos, startYPos + RoundInfoSpanHeight + spacing);

            GameObject edge1 = new GameObject("Edge1");
            Image edge1Image = edge1.AddComponent<Image>();
            SetImage(edge1Image, edgeTexture);
            edge1.transform.parent = infoBox.transform;
            RectTransform edge1Rect = edge1.GetComponent<RectTransform>();
            edge1Rect.pivot = Vector2.zero;
            edge1Rect.sizeDelta = new Vector2(RoundInfoEdgeWidth, RoundInfoSpanHeight);
            edge1Rect.anchorMax = Vector2.zero;
            edge1Rect.anchorMin = Vector2.zero;
            edge1Rect.anchoredPosition = new Vector2(startXPos - RoundInfoEdgeWidth, startYPos);

            GameObject span = new GameObject("Span");
            Image spanImage = span.AddComponent<Image>();
            SetImage(spanImage, spanTexture);
            span.transform.parent = infoBox.transform;
            RectTransform spanRect = span.GetComponent<RectTransform>();
            spanRect.pivot = Vector2.zero;
            spanRect.sizeDelta = new Vector2(spanWidth, RoundInfoSpanHeight);
            spanRect.anchorMax = Vector2.zero;
            spanRect.anchorMin = Vector2.zero;
            spanRect.anchoredPosition = new Vector2(startXPos, startYPos);

            GameObject edge2 = new GameObject("Edge2");
            Image edge2Image = edge2.AddComponent<Image>();
            SetImage(edge2Image, edgeTexture);
            edge2.transform.parent = infoBox.transform;
            RectTransform edge2Rect = edge2.GetComponent<RectTransform>();
            edge2Rect.pivot = Vector2.zero;
            edge2Rect.sizeDelta = new Vector2(RoundInfoEdgeWidth, RoundInfoSpanHeight);
            edge2Rect.anchorMax = Vector2.zero;
            edge2Rect.anchorMin = Vector2.zero;
            edge2Rect.localScale = new Vector3(-1, 1);
            edge2Rect.anchoredPosition = new Vector2(startXPos + spanWidth + RoundInfoEdgeWidth, startYPos);

            if (Settings.Default.showBloonIds || bloonType is null) {
                GameObject id = new GameObject($"ID");
                TextMeshProUGUI idText = id.AddComponent<TextMeshProUGUI>();
                idText.color = new Color(1, 1, 1, .5f);
                idText.font = font;
                idText.fontSize = RoundInfoFontSize / 2;
                idText.text = "0";
                float idTextHeight = idText.preferredHeight;
                idText.text = $"ID: {group.bloon}";
                idText.transform.parent = infoBox.transform;
                RectTransform idRect = id.GetComponent<RectTransform>();
                idRect.pivot = Vector2.zero;
                idRect.sizeDelta = new Vector2(idText.preferredWidth, idTextHeight);
                idRect.anchorMax = Vector2.zero;
                idRect.anchorMin = Vector2.zero;
                idRect.anchoredPosition = new Vector2(0, 0);
            }

            panel.transform.parent = parent.transform;
            panel.transform.localScale = Vector3.one;
        }
    }
}
