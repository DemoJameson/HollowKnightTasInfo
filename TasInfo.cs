using System;
using System.Text;
using UnityEngine;
// ReSharper disable Unity.NoNullPropagation

namespace HollowKnightTasInfo {
    public static class TasInfo {
        private static string heroInfo = "";
        private static float inGameTime = 0f;
        public static void UpdateInfo() {
            if (GameManager._instance?.hero_ctrl is { } heroController) {
                StringBuilder infoBuilder = new();
                infoBuilder.AppendLine($"pos: {heroController.transform.position.ToSimpleString(5)}");
                infoBuilder.AppendLine($"vel: {heroController.current_velocity.ToSimpleString(3)}");
                infoBuilder.AppendLine(heroController.hero_state.ToString());
                heroInfo = infoBuilder.ToString();
                MergeInfo();
            }
        }

        public static void UpdateInGameTime() {
            if (GameManager._instance is { } gameManager) {
                inGameTime += Time.deltaTime;
                MergeInfo();
            }
        }

        private static void MergeInfo() {
            GameManager.Info = $"{heroInfo}\n{inGameTime}";
        }
    }

    internal static class Vector2Extension {
        public static string ToSimpleString(this Vector2 vector2, int precision) {
            return $"{vector2.x.ToString($"F{precision}")}, {vector2.y.ToString($"F{precision}")}";
        }

        public static string ToSimpleString(this Vector3 vector3, int precision) {
            return $"{vector3.x.ToString($"F{precision}")}, {vector3.y.ToString($"F{precision}")}";
        }
    }
}