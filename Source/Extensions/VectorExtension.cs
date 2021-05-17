using UnityEngine;

namespace HollowKnightTasInfo {
    internal static class VectorExtension {
        public static string ToSimpleString(this Vector2 vector2, int precision) {
            return $"{vector2.x.ToString($"F{precision}")}, {vector2.y.ToString($"F{precision}")}";
        }

        public static string ToSimpleString(this Vector3 vector3, int precision) {
            return $"{vector3.x.ToString($"F{precision}")}, {vector3.y.ToString($"F{precision}")}";
        }
    }
}