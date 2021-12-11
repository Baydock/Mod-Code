using Assets.Scripts.Data;
using Assets.Scripts.Unity.UI_New.ChallengeEditor;
using HarmonyLib;
using HelpfulAdditions.Properties;

namespace HelpfulAdditions {
    public partial class Mod {
        [HarmonyPatch(typeof(ChallengeEditor), nameof(ChallengeEditor.MapSelectClicked))]
        [HarmonyPrefix]
        // Allow all to be seen
        public static bool ShowHiddenMapsPre(out bool[] __state) {
            if (Settings.Default.blonsInEditor) {
                bool[] isHidden = new bool[GameData.Instance.mapSet.maps.Length];
                for (int i = 0; i < isHidden.Length; i++) {
                    isHidden[i] = GameData.Instance.mapSet.maps[i].isBrowserOnly;
                    GameData.Instance.mapSet.maps[i].isBrowserOnly = false;
                }
                __state = isHidden;
                return true;
            }
            __state = null;
            return true;
        }

        [HarmonyPatch(typeof(ChallengeEditor), nameof(ChallengeEditor.MapSelectClicked))]
        [HarmonyPostfix]
        // Set back to normal
        public static void ShowHiddenMapsPost(bool[] __state) {
            if (Settings.Default.blonsInEditor && !(__state is null))
                for (int i = 0; i < __state.Length; i++)
                    GameData.Instance.mapSet.maps[i].isBrowserOnly = __state[i];
        }
    }
}
