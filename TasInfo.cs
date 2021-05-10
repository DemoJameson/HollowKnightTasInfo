using System.Text;
using UnityEngine;

namespace HollowKnightTasInfo {
    public static class TasInfo {
        public static long Mark = 1234567890123456789;
        public static string Info;

        public static void Update() {
            StringBuilder infoBuilder = new();
            if (GameManager.instance is { } gameManager) {
                if (gameManager.hero_ctrl is { } heroController) {
                    infoBuilder.AppendLine($"pos: {heroController.transform.position.ToSimpleString(6)}");
                    infoBuilder.AppendLine($"vel: {heroController.current_velocity.ToSimpleString(3)}");
                    infoBuilder.AppendLine(heroController.hero_state.ToString());
                }
            }

            Info = infoBuilder.ToString();
        }
    }

    public static class Vector2Extension {
        public static string ToSimpleString(this Vector2 vector2, int precision) {
            return $"{vector2.x.ToString($"F{precision}")}, {vector2.y.ToString($"F{precision}")}";
        }

        public static string ToSimpleString(this Vector3 vector3, int precision) {
            return $"{vector3.x.ToString($"F{precision}")}, {vector3.y.ToString($"F{precision}")}";
        }
    }
}