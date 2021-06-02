using UnityEngine;

namespace Assembly_CSharp.TasInfo.mm.Source.Extensions {
    internal static class VectorExtension {
        public static string ToSimpleString(this Vector2 vector2, int precision, string separator = ", ") {
            return $"{vector2.x.ToString($"F{precision}")}{separator}{vector2.y.ToString($"F{precision}")}";
        }

        public static string ToSimpleString(this Vector3 vector3, int precision, string separator = ", ") {
            return $"{vector3.x.ToString($"F{precision}")}{separator} {vector3.y.ToString($"F{precision}")}";
        }
    }
}