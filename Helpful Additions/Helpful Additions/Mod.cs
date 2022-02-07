using Assets.Scripts.Models.Bloons;
using HarmonyLib;
using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(HelpfulAdditions.Mod), "Helpful Additions", "1.6.4", "Baydock")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace HelpfulAdditions {
    [HarmonyPatch]
    public partial class Mod : MelonMod {
        private static MelonLogger.Instance Logger;

        public override void OnApplicationStart() {
            Logger = LoggerInstance;
        }

        private static Texture2D LoadTexture(byte[] data) {
            Texture2D tex = new Texture2D(0, 0) { wrapMode = TextureWrapMode.Clamp };
            ImageConversion.LoadImage(tex, data);
            return tex;
        }

        private static void SetImage(Image image, byte[] data) => SetImage(image, LoadTexture(data));

        private static void SetImage(Image image, Texture2D tex) => image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);

        private static readonly string[] NormalBloonBaseTypes = {
            "Red",
            "Blue",
            "Green",
            "Yellow",
            "Pink",
            "Purple",
            "Black",
            "White",
            "Lead",
            "Zebra",
            "Rainbow",
            "Ceramic"
        };

        private static readonly string[] MoabBloonBaseTypes = {
            "Moab",
            "Bfb",
            "Zomg",
            "Ddt",
            "Bad"
        };

        private static readonly string[] BossBloonBaseTypes = {
            "Bloonarius",
            "Lych"
        };

        private static string GetBloonType(BloonModel bloon) {
            if (!(bloon is null)) {
                if (NormalBloonBaseTypes.Contains(bloon.baseId))
                    return GetNormalBloonType(bloon);
                if (MoabBloonBaseTypes.Contains(bloon.baseId))
                    return GetMoabBloonType(bloon);
                if (bloon.baseId.Equals("Golden"))
                    return GetGoldenType(bloon);
                if (bloon.id.Equals("TestBloon"))
                    return "GhostBloon";
                if (BossBloonBaseTypes.Contains(bloon.baseId))
                    return GetBossBloonType(bloon);
            }
            return null;
        }

        private static string GetBloonBaseType(BloonModel bloon) {
            if (bloon.id.Equals("TestBloon"))
                return "Ghost";
            if (bloon.isBoss && bloon.id.Contains("Elite"))
                return "Elite" + bloon.baseId;
            return bloon.baseId;
        }

        private static int GetBossTier(BloonModel bloon) {
            // distance from char 0 gets digit value
            return bloon.id[bloon.id.Length - 1] - '0';
        }

        private static string GetNormalBloonType(BloonModel bloon) {
            string bloonType = bloon.baseId + "Bloon";
            if (bloon.isGrow)
                bloonType = "Regrow" + bloonType;
            if (bloon.isCamo)
                bloonType = "Camo" + bloonType;
            if (bloon.isFortified)
                bloonType = "Fortified" + bloonType;
            return bloonType;
        }

        private static string GetMoabBloonType(BloonModel moab) {
            if (moab.isFortified)
                return "Fortified" + moab.baseId;
            return moab.baseId;
        }

        private static string GetGoldenType(BloonModel golden) {
            string bloonType = "GoldenBloon";
            if (golden.bloonProperties.HasFlag(BloonProperties.Lead)) {
                if (golden.bloonProperties.HasFlag(BloonProperties.Purple)) {
                    if (golden.bloonProperties.HasFlag(BloonProperties.Black) && golden.bloonProperties.HasFlag(BloonProperties.White))
                        bloonType = "Zebra" + bloonType;
                    bloonType = "Purple" + bloonType;
                }
                bloonType = "Lead" + bloonType;
                if (golden.isFortified)
                    bloonType = "Fortified" + bloonType;
            }
            if (golden.isCamo) {
                if (golden.isFortified)
                    bloonType = bloonType.Insert(9, "Camo");
                else
                    bloonType = "Camo" + bloonType;
            }
            return bloonType;
        }

        private static string GetBossBloonType(BloonModel boss) {
            if (boss.id.Contains("Elite"))
                return "Elite" + boss.baseId;
            return boss.baseId;
        }

        private static readonly Dictionary<string, (byte[], Vector2?)> customBloonIcons = new Dictionary<string, (byte[], Vector2?)>();
        private static readonly Dictionary<string, byte[]> customBloonEdges = new Dictionary<string, byte[]>();
        private static readonly Dictionary<string, byte[]> customBloonSpans = new Dictionary<string, byte[]>();

        /// <summary>
        /// This function allows other modders to add custom bloon graphics, such that they show up within my mod.
        /// Reflection is necessary to access this method from another mod.
        /// </summary>
        /// <param name="bloonId">The id of the custom bloon being added</param>
        /// <param name="icon">The icon of the custom bloon being added as bytes from an image file</param>
        /// <param name="edge">The graphic for the ends of a timespan for the custom bloon as bytes from an image file</param>
        /// <param name="span">The graphic for the span of a timespan for the custom bloon as bytes from an image file</param>
        /// <param name="iconSize">The size of the icon in Unity, where the image size is the original,
        /// and 200 is the maximum recommended size</param>
        public static void AddCustomBloon(string bloonId, byte[] icon, byte[] edge, byte[] span, Vector2? iconSize = null) {
            if (!customBloonIcons.ContainsKey(bloonId)) {
                customBloonIcons.Add(bloonId, (icon, iconSize));
                customBloonEdges.Add(bloonId, edge);
                customBloonSpans.Add(bloonId, span);
            } else {
                customBloonIcons[bloonId] = (icon, iconSize);
                customBloonEdges[bloonId] = edge;
                customBloonSpans[bloonId] = span;
            }
        }

        /// <summary>
        /// This function allows other modders to add custom bloon graphics, such that they show up within my mod.
        /// Reflection is necessary to access this method from another mod.
        /// </summary>
        /// <param name="bloonId">The id of the custom bloon being added</param>
        /// <param name="icon">The icon of the custom bloon being added as a Texture2D</param>
        /// <param name="edge">The graphic for the ends of a timespan for the custom bloon as a Texture2D</param>
        /// <param name="span">The graphic for the span of a timespan for the custom bloon as a Texture2D</param>
        /// <param name="iconSize">The size of the icon in Unity, where the image size is the original,
        /// and 200 is the maximum recommended size</param>
        // Must convert to byte[] in order for the Il2Cpp side to not garbage collect it
        public static void AddCustomBloon(string bloonId, Texture2D icon, Texture2D edge, Texture2D span, Vector2? iconSize = null) =>
            AddCustomBloon(bloonId, ImageConversion.EncodeToPNG(icon), ImageConversion.EncodeToPNG(edge), ImageConversion.EncodeToPNG(span), iconSize);
    }
}
