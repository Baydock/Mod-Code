using Assets.Scripts.Models;
using Assets.Scripts.Models.Rounds;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New.Popups;
using HarmonyLib;
using MelonLoader;

[assembly: MelonInfo(typeof(Stupid_Mod_W.Mod), "Dense Bloon Rounds", "1.0.0", "Baydock")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace Stupid_Mod_W {
    [HarmonyPatch]
    public class Mod : MelonMod {
        private static MelonLogger.Instance Logger;

        public override void OnApplicationStart() {
            Logger = LoggerInstance;
        }

        private static void ApplyDenseness(int mult) {
            GameModel gameModel = Game.instance.model;
            foreach (RoundSetModel roundSet in gameModel.roundSets) {
                for (int i = 0; i < roundSet.rounds.Length; i++) {
                    foreach (BloonGroupModel group in roundSet.rounds[i].groups)
                        group.count *= mult;
                    roundSet.rounds[i] = new RoundModel("", roundSet.rounds[i].groups);
                }
            }
        }

        [HarmonyPatch(typeof(PopupScreen), nameof(PopupScreen.Awake))]
        [HarmonyPostfix]
        public static void GetDenseness() {
            PopupScreen.instance.ShowSetValuePopup("Bloon Multiplier", "Choose how many more times denser you want rounds to be.", new System.Action<int>(mult => {
                if (mult > 100) {
                    PopupScreen.instance.ShowPopup(placement: PopupScreen.Placement.menuCenter,
                                                 title: "Are you sure?",
                                                 body: "BloonsTD6 wasn't made for this many bloons, which can lead to long pauses and/or the use of extrordinary amounts of memory.",
                                                 okCallback: new System.Action(() => ApplyDenseness(mult)),
                                                 okString: "Yes",
                                                 cancelCallback: new System.Action(() => GetDenseness()),
                                                 cancelString: "Re-input",
                                                 transition: Popup.TransitionAnim.Scale);
                } else
                    ApplyDenseness(mult);
            }), 10);
        }
    }
}
