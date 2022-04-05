using System;

namespace HelpfulAdditions {
    internal static class Utils {
        public static void Shuffle<T>(this Il2CppSystem.Collections.Generic.List<T> list) {
            Random r = new Random();
            for(int i = list.Count - 1; i > 0; i--) {
                int j = r.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
