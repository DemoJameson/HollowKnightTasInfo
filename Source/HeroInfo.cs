using System.Collections.Generic;
using System.Text;
using HollowKnightTasInfo.Extensions;
using HollowKnightTasInfo.Utils;
using UnityEngine;

namespace HollowKnightTasInfo {
    internal static class HeroInfo {
        private static readonly List<string> HeroStates = new() {
            "Attack",
            "Jump",
            "DoubleJump",
            "Dash",
            "SuperDash",
            "TakeDamage",
            "WallJump",
            "Cast",
        };

        private static Vector3 lastPosition = Vector3.zero;
        private static float frameRate => Time.unscaledDeltaTime == 0 ? 0 : 1 / Time.unscaledDeltaTime;

        public static void OnUpdate(GameManager gameManager, StringBuilder infoBuilder) {
            if (gameManager.hero_ctrl is { } heroController && ConfigManager.ShowKnightInfo) {
                Vector3 position = heroController.transform.position;
                infoBuilder.AppendLine($"pos: {position.ToSimpleString(5)}");
                infoBuilder.AppendLine($"{heroController.hero_state} vel: {heroController.current_velocity.ToSimpleString(3)}");
                infoBuilder.AppendLine($"diff vel: {((position - lastPosition) * frameRate).ToSimpleString(3)}");
                lastPosition = position;

                // CanJump 中会改变该字段的值，所以需要备份稍微还原
                int ledgeBufferSteps = heroController.GetFieldValue<int>(nameof(ledgeBufferSteps));

                List<string> results = new();
                foreach (string heroState in HeroStates) {
                    if (heroController.InvokeMethod<bool>($"Can{heroState}")) {
                        if (heroState != "TakeDamage") {
                            results.Add(heroState);
                        }
                    } else if (heroState == "TakeDamage") {
                        results.Add("Invincible");
                    }
                }

                heroController.SetFieldValue(nameof(ledgeBufferSteps), ledgeBufferSteps);

                if (results.Count > 0) {
                    infoBuilder.AppendLine(HkUtils.Join(" ", results));
                }
            }
        }
    }
}