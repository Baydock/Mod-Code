
using System.Runtime.InteropServices;

namespace BTD7 {
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public override string ToString() {
            return $"{left},{right},{top},{bottom}";
        }
    }
}
