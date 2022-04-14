using Il2CppSystem.Collections.Generic;
using UnhollowerBaseLib;

namespace DefectiveTowers.Utils {
    internal static class Il2CppArrayUtils {
        public static Il2CppReferenceArray<T> Add<T>(this Il2CppReferenceArray<T> array, params T[] values) where T : Il2CppObjectBase => AddBase(array, values);
        public static Il2CppStringArray Add(this Il2CppStringArray array, params string[] values) => AddBase(array, values);
        private static T[] AddBase<T>(Il2CppArrayBase<T> array, T[] values) {
            T[] added = new T[array.Length + values.Length];
            array.CopyTo(added, 0);
            values.CopyTo(added, array.Length);
            return added;
        }

        public static Il2CppReferenceArray<T> Insert<T>(this Il2CppReferenceArray<T> array, int index, params T[] values) where T : Il2CppObjectBase => InsertBase(array, index, values);
        public static Il2CppStringArray Insert(this Il2CppStringArray array, int index, params string[] values) => InsertBase(array, index, values);
        private static T[] InsertBase<T>(Il2CppArrayBase<T> array, int index, T[] values) {
            T[] added = new T[array.Length + values.Length];
            for (int i = 0; i < index; i++)
                added[i] = array[i];
            values.CopyTo(added, index);
            for (int i = index; i < array.Length; i++)
                added[i + values.Length] = array[i];
            return added;
        }

        public static int FindIndex<T>(this Il2CppArrayBase<T> array, System.Func<T, bool> predicate) {
            for (int i = 0; i < array.Length; i++)
                if (predicate.Invoke(array[i]))
                    return i;
            return -1;
        }

        public static T Last<T>(this List<T> list) => list[list.Count - 1];

        public static T Find<T>(this Il2CppArrayBase<T> array, System.Func<T, bool> predicate) {
            for(int i = 0; i < array.Count; i++)
                if (predicate(array[i]))
                    return array[i];
            return default;
        }

        public static void AddIfNotPresent<K, V>(this Dictionary<K, V> dict, K key, V value) {
            if (!dict.ContainsKey(key))
                dict.Add(key, value);
        }
    }
}
