using HarmonyLib;
using MelonLoader;

[assembly: MelonInfo(typeof(HelpfulAdditions.Mod), "Helpful Additions", "1.2.2", "Baydock")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace HelpfulAdditions {
    [HarmonyPatch]
    public partial class Mod : MelonMod {
    }
}
