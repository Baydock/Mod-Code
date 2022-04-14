using Assets.Scripts.Models;

namespace DefectiveTowers.Utils {
    internal static class ModelUtils {
        public static T CloneCast<T>(this T model) where T : Model => model.Clone().Cast<T>();

        public static T CloneCast<T>(this Model model) where T : Model => model.Clone().Cast<T>();
    }
}
