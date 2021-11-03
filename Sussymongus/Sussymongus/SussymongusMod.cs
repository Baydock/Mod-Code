using Assets.Main.Scenes;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace Sussymongus {
    public class SussymongusMod : MelonMod {
        public override void OnApplicationStart() {
            MelonLogger.Msg("Hello World!");
        }
    }

    [HarmonyPatch(typeof(TitleScreen), nameof(TitleScreen.OnPlayButtonClicked))]
    public class TitleScreenPatch {
        [HarmonyPrefix]
        public static bool Prefix() {
            Application.OpenURL("https://store.steampowered.com/app/945360/Among_Us/");
            Application.Quit();
            return false;
        }
    }
}
