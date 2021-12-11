using Assets.Scripts.Models.Bloons;
using HarmonyLib;
using MelonLoader;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(HelpfulAdditions.Mod), "Helpful Additions", "1.6.1", "Baydock")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace HelpfulAdditions {
    [HarmonyPatch]
    public partial class Mod : MelonMod {
        private static MelonLogger.Instance Logger;

        public override void OnApplicationStart() {
            Logger = LoggerInstance;
        }

        private static void SetImage(Image image, byte[] data) {
            Texture2D tex = new Texture2D(0, 0) { wrapMode = TextureWrapMode.Clamp };
            ImageConversion.LoadImage(tex, data);
            SetImage(image, tex);
        }

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
    }
}
