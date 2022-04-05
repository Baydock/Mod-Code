using Assets.Main.Scenes;
using HarmonyLib;
using MelonLoader;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using static BTD7.Win32;
using Random = System.Random;

[assembly: MelonInfo(typeof(BTD7.Mod), "BTD7", "1.0.0", "Baydock")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace BTD7 {
    [HarmonyPatch]
    public class Mod : MelonMod {

        public static MelonLogger.Instance Logger;

        public override void OnApplicationLateStart() {
            base.OnApplicationStart();

            Logger = LoggerInstance;
        }

        private static string[] bloonsGames = new string[] { "btd1", "btd2", "btd3", "btd4" };

        [HarmonyPatch(typeof(TitleScreen), nameof(TitleScreen.OnPlayButtonClicked))]
        [HarmonyPrefix]
        public static bool OnPlayButtonClicked() {
            Random r = new Random();

            string bloonsGame = bloonsGames[r.Next(0, bloonsGames.Length)];

            if (bloonsGame.Equals("btd4"))
                bloonsGame = r.Next(2) == 0 ? "btd4" : "btd4e";

            string path = $"{Environment.CurrentDirectory}/Mods/BloonsGames";

            Directory.CreateDirectory(path);
            try { File.WriteAllBytes($"{path}/{bloonsGame}.exe", GetResource($"{bloonsGame}.exe")); } catch { }

            IntPtr btd6Window = GetActiveWindow();
            Process bloonsProcess = Process.Start($"{path}/{bloonsGame}.exe");
            while (!bloonsProcess.HasExited && bloonsProcess.MainWindowHandle == IntPtr.Zero)
                bloonsProcess.Refresh();
            bloonsProcess.WaitForInputIdle();
            IntPtr bloonsWindow = bloonsProcess.MainWindowHandle;
            EnumThreadWindows(GetWindowThreadProcessId(bloonsWindow, out int _), (hwnd, lParam) => {
                StringBuilder name = new StringBuilder(256);
                GetWindowText(hwnd, name, 256);
                StringBuilder className = new StringBuilder(256);
                GetClassName(hwnd, className, 256);
                if (name.ToString().Equals("FFDec Flash Player") && className.ToString().Equals("TfrmMain")) {
                    bloonsWindow = hwnd;
                    //SetWindowLongPtr(bloonsWindow, GWL_STYLE, GetWindowLongPtr(btd6Window, GWL_STYLE));
                    //SetWindowLongPtr(bloonsWindow, GWL_EXSTYLE, GetWindowLongPtr(btd6Window, GWL_EXSTYLE));
                    GetWindowRect(btd6Window, out Rect btd6Rect);
                    int width = btd6Rect.right - btd6Rect.left;
                    int height = btd6Rect.bottom - btd6Rect.top;
                    SetWindowPos(bloonsWindow, IntPtr.Zero, btd6Rect.left, btd6Rect.top, width, height, SWP_SHOWWINDOW);

                    GetWindowRect(GetDesktopWindow(), out Rect screenRect);
                    int screenWidth = screenRect.right - screenRect.left;
                    int screenHeight = screenRect.bottom - screenRect.top;
                    if (screenWidth == width && screenHeight == height)
                        ShowWindow(bloonsWindow, SW_MAXIMIZE);
                }
                return true;
            }, IntPtr.Zero);

            Application.Quit();
            return false;
        }

        private static byte[] GetResource(string resourceName) {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            string name = thisAssembly.GetName().Name;

            using (MemoryStream resourceStream = new MemoryStream()) {
                thisAssembly.GetManifestResourceStream($"{name}.{resourceName}").CopyTo(resourceStream);
                return resourceStream.ToArray();
            }
        }

        private static void ListResources() {
            foreach (string name in Assembly.GetExecutingAssembly().GetManifestResourceNames())
                Logger.Msg(name);
        }
    }
}
