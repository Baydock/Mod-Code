using HarmonyLib;
using MelonLoader;
using System.Drawing.Imaging;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Bitmap = System.Drawing.Bitmap;

[assembly: MelonInfo(typeof(HelpfulAdditions.Mod), "Helpful Additions", "1.3.0", "Baydock")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace HelpfulAdditions {
    [HarmonyPatch]
    public partial class Mod : MelonMod {
        private static void SetImage(Image image, Bitmap bitmap) {
            Texture2D tex = new Texture2D(0, 0);
            using (MemoryStream ms = new MemoryStream()) {
                bitmap.Save(ms, ImageFormat.Png);
                ImageConversion.LoadImage(tex, ms.ToArray());
                ms.Close();
            }
            image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2());
        }
    }
}
