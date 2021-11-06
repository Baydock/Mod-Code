using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.BloonMenu;
using Assets.Scripts.Unity.UI_New.InGame.RightMenu;
using Assets.Scripts.Unity.UI_New.InGame.RightMenu.Powers;
using HarmonyLib;

namespace HelpfulAdditions {
    public partial class Mod {
        [HarmonyPatch(typeof(RightMenu), nameof(RightMenu.SetPowersInteractable))]
        [HarmonyPrefix]
        public static bool MakePowersAvailable(ref bool interactable) {
            if (InGame.instance.IsSandbox)
                interactable = true;
            return true;
        }

        [HarmonyPatch(typeof(BloonMenu), nameof(BloonMenu.Initialise))]
        [HarmonyPostfix]
        public static void SetInitialState() {
            if (InGame.instance.IsSandbox) {
                ShopMenu.instance.GetComponentInChildren<BloonMenuToggle>(true).gameObject.SetActive(false);
                RightMenu.instance.powerBtn.gameObject.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(BloonMenuToggle), nameof(BloonMenuToggle.ToggleBloonMenu))]
        [HarmonyPostfix]
        public static void ShowBloonMenu() {
            if (InGame.instance.IsSandbox && InGame.instance.BloonMenu.showInternal) {
                ShopMenu.instance.GetComponentInChildren<BloonMenuToggle>(true).gameObject.SetActive(false);
                RightMenu.instance.powerBtn.gameObject.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(PowersMenu), nameof(PowersMenu.Open))]
        [HarmonyPostfix]
        public static void ShowPowersMenu() {
            if (InGame.instance.IsSandbox) {
                InGame.instance.BloonMenu.showInternal = false;
                ShopMenu.instance.GetComponentInChildren<BloonMenuToggle>(true).gameObject.SetActive(false);
                RightMenu.instance.powerBtn.gameObject.SetActive(true);
                powerIsBeingUsed = false;
            }
        }

        [HarmonyPatch(typeof(PowersMenu), nameof(PowersMenu.Close))]
        [HarmonyPostfix]
        public static void ShowTowerMenu() {
            if (InGame.instance.IsSandbox && !powerIsBeingUsed) {
                ShopMenu.instance.GetComponentInChildren<BloonMenuToggle>(true).gameObject.SetActive(true);
                RightMenu.instance.powerBtn.gameObject.SetActive(false);
            }
        }

        private static bool powerIsBeingUsed = false;
        [HarmonyPatch(typeof(PowersMenu), nameof(PowersMenu.StartPowerPlacement))]
        [HarmonyPostfix]
        public static void WhenPowerIsBeingUsed() {
            if (InGame.instance.IsSandbox)
                powerIsBeingUsed = true;
        }

        [HarmonyPatch(typeof(InstaTowerGroupMenu), nameof(InstaTowerGroupMenu.Initialise))]
        [HarmonyPostfix]
        public static void RemoveInstaPowers(ref InstaTowerGroupMenu __instance) {
            if (InGame.instance.IsSandbox)
                __instance.gameObject.SetActive(false);
        }

        [HarmonyPatch(typeof(PowersMenu), nameof(PowersMenu.LoadPowers))]
        [HarmonyPostfix]
        public static void EnablePowers(ref PowersMenu __instance) {
            if (InGame.instance.IsSandbox) {
                __instance.getMorePowersLarge.interactable = true;
                InGame.Bridge.Model.powersEnabled = true;
            }
        }

        [HarmonyPatch(typeof(PowersMenu), nameof(PowersMenu.UpdateShowInstaMonkeysButton))]
        [HarmonyPostfix]
        public static void RemoveShowInstaMonkeysButton(ref PowersMenu __instance) {
            if (InGame.instance.IsSandbox)
                __instance.showInstaMonkeysButton.SetActive(false);
        }

        [HarmonyPatch(typeof(PowerButton), nameof(PowerButton.ModeDisabled))]
        [HarmonyPostfix]
        public static void MakePowersModeEnabled(ref bool __result) {
            if (InGame.instance.IsSandbox)
                __result = false;
        }

        [HarmonyPatch(typeof(PowerButton), nameof(PowerButton.RoundDisabled))]
        [HarmonyPostfix]
        public static void MakePowersRoundEnabled(ref bool __result) {
            if (InGame.instance.IsSandbox)
                __result = false;
        }

        [HarmonyPatch(typeof(StandardPowerButton), nameof(StandardPowerButton.UpdatePowerDisplay))]
        [HarmonyPostfix]
        public static void DontShowPowerCountIcon(ref StandardPowerButton __instance) {
            if (InGame.instance.IsSandbox)
                __instance.powerCountIcon.SetActive(false);
        }

        [HarmonyPatch(typeof(StandardPowerButton), nameof(StandardPowerButton.UpdateUseCount))]
        [HarmonyPostfix]
        public static void DontShowPowerCountText(ref StandardPowerButton __instance) {
            if (InGame.instance.IsSandbox)
                __instance.powerCountText.gameObject.SetActive(false);
        }

        [HarmonyPatch(typeof(PowersMenu), nameof(PowersMenu.PowerUseSuccess))]
        [HarmonyPrefix]
        public static bool DontLosePowers() {
            // If in sandbox, don't allow for player profile to change when power used
            return !InGame.instance.IsSandbox;
        }
    }
}
