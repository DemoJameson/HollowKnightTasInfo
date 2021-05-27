using UnityEngine;

namespace HollowKnightTasInfo.Extensions {
    internal static class VectorExtension {
        public static string ToSimpleString(this Vector2 vector2, int precision, string separator = ", ") {
            return $"{vector2.x.ToString($"F{precision}")}{separator}{vector2.y.ToString($"F{precision}")}";
        }

        public static string ToSimpleString(this Vector3 vector3, int precision, string separator = ", ") {
            return $"{vector3.x.ToString($"F{precision}")}{separator} {vector3.y.ToString($"F{precision}")}";
        }

        public static Vector2 WorldPosition(this Transform transform, Vector2 point = new()) {
            do {
                point = transform.localPosition + transform.localRotation * Vector3.Scale(point, transform.localScale);
                transform = transform.parent;
            } while (transform != null);

            return point;
        }
    }
}