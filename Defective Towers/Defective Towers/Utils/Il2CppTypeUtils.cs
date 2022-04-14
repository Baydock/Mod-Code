using UnhollowerRuntimeLib;

namespace DefectiveTowers.Utils {
    internal static class Il2CppTypeUtils {
        public static bool IsIl2CppType<T>(this Il2CppSystem.Object @object) where T : Il2CppSystem.Object => Il2CppType.Of<T>().IsAssignableFrom(@object.GetIl2CppType());

        public static bool TryCast<T>(this Il2CppSystem.Object @object, out T castedObject) where T : Il2CppSystem.Object {
            if (IsIl2CppType<T>(@object)) {
                castedObject = @object.Cast<T>();
                return true;
            }
            castedObject = null;
            return false;
        }
    }
}
