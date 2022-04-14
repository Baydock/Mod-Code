using UnityEngine;

namespace DefectiveTowers.Utils {
    internal static class TransformUtils {
        public static Transform FindChildRecursive(this Transform parent, string childName) {
            for (int i = 0; i < parent.childCount; i++) {
                if (parent.GetChild(i).name.Equals(childName))
                    return parent.GetChild(i);

                Transform child = FindChildRecursive(parent.GetChild(i), childName);
                if (!(child is null))
                    return child;
            }
            return null;
        }
    }
}
