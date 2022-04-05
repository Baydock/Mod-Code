using Assets.Scripts.Unity;
using Assets.Scripts.Unity.Audio;
using Assets.Scripts.Unity.UI_New.Settings;
using HarmonyLib;
using HelpfulAdditions.Properties;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace HelpfulAdditions {
    public partial class Mod : MelonMod {
        [HarmonyPatch(typeof(InGameMusicFactory), nameof(InGameMusicFactory.PopulateList))]
        [HarmonyPostfix]
        public static void RandomizeMusicFactoryTracks(ref InGameMusicFactory __instance) {
            if (Settings.Default.shuffleJukebox) {
                __instance.trackItemDataList.Shuffle();
                for (int i = 0; i < __instance.trackItemDataList.Count; i++)
                    __instance.trackItemDataList[i].trackIndex = i;
            }
        }


        [HarmonyPatch(typeof(MiniJukeBoxPlayer), nameof(MiniJukeBoxPlayer.PopulateList))]
        [HarmonyPostfix]
        public static void RandomizeJukeBoxTracks(ref MiniJukeBoxPlayer __instance) {
            if (Settings.Default.shuffleJukebox)
                __instance.trackDataList = Game.instance.audioFactory.musicFactory.trackItemDataList;
        }

        [HarmonyPatch(typeof(SettingsScreen), nameof(SettingsScreen.Open))]
        [HarmonyPostfix]
        public static void AddJukeboxShuffleToggle(ref SettingsScreen __instance) {
            RectTransform jukeToggle = __instance.jukeBoxCheckBtn.GetComponent<RectTransform>();
            Transform jukeboxMenu = jukeToggle.parent;
            Vector3 oldTogglePos = jukeToggle.localPosition;
            for (int i = 0; i < jukeboxMenu.childCount; i++) {
                Transform child = jukeboxMenu.GetChild(i);
                if (child.localPosition.y >= oldTogglePos.y)
                    child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y + jukeToggle.sizeDelta.y + 10, child.localPosition.z);
            }

            GameObject shuffleToggle = Object.Instantiate(jukeToggle.gameObject, jukeboxMenu);
            shuffleToggle.name = "ShuffleToggle";
            shuffleToggle.transform.localPosition = oldTogglePos;
            shuffleToggle.GetComponent<Image>().color = Color.white;
            NK_TextMeshProUGUI toggleText = shuffleToggle.GetComponentInChildren<NK_TextMeshProUGUI>();
            toggleText.localizeKey = "";
            toggleText.text = "Shuffle";
            Image checkImage = shuffleToggle.transform.FindChild("Tick").GetComponent<Image>();
            checkImage.enabled = Settings.Default.shuffleJukebox;
            Button toggleButton = shuffleToggle.GetComponentInChildren<Button>();
            toggleButton.onClick.AddListener(new System.Action(() => {
                if (checkImage.enabled) {
                    checkImage.enabled = false;
                    Settings.Default.shuffleJukebox = false;
                } else {
                    checkImage.enabled = true;
                    Settings.Default.shuffleJukebox = true;
                }
                Settings.Default.Save();
            }));
        }
    }
}
