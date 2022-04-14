using System.IO;
using System.Reflection;
using UnityEngine;

namespace DefectiveTowers {
    internal static class Resources {
        private static byte[] GetResource(string resourceName) {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            string name = thisAssembly.GetName().Name.Replace(" ", "");

            using (MemoryStream resourceStream = new MemoryStream()) {
                thisAssembly.GetManifestResourceStream($"{name}.Resources.{resourceName}").CopyTo(resourceStream);
                return resourceStream.ToArray();
            }
        }

        private static Texture2D GetTexture(string resourceName) {
            Texture2D tex = new Texture2D(0, 0);
            ImageConversion.LoadImage(tex, GetResource(resourceName));
            return tex;
        }

        private static Sprite GetSprite(string resourceName) {
            Texture2D tex = GetTexture(resourceName);
            //tex.filterMode = FilterMode.Point;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }

        public static Texture2D MiniTackShooterTexture => GetTexture("MiniTackShooter.Texture.png");

        public static Sprite MiniTackShooterPortrait => GetSprite("MiniTackShooter.Portrait.png");

        public static Sprite MiniTackShooterInstaIcon => GetSprite("MiniTackShooter.InstaIcon.png");

        public static Sprite MonkeyPortrait => GetSprite("Monkey.Portrait.png");

        public static Sprite MonkeyInstaIcon => GetSprite("Monkey.InstaIcon.png");
    }
}
