using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HollowKnightTasInfo {
    internal static class ConfigManager {
        private const string ConfigFile = "./HollowKnightTasInfo.config";
        private static string defaultContent = @"
[Settings]
ShowKnightInfo = true
ShowCustomInfo = true
ShowSceneName = true
ShowTime = true
ShowHitbox = true
ShowHp = true

# 该配置用于定制附加显示的数据，需要注意如果调用属性或者方法有可能会造成 desync
# 例如 HeroController.CanJump() 会修改 ledgeBufferSteps 字段，请查看源码确认是否安全。定制数据格式如下：
# {UnityObject子类名.字段/属性/方法.字段/属性/方法……}，只支持无参方法需要以()结尾
# {GameObjectName.字段/属性/方法.字段/属性/方法……}
# 支持配置多行，并且同一行可以存在多个 {}

[Custom_Info_Template]
# canAttack: {HeroController.CanAttack()}
# geo: {HeroController.playerData.geo}
";

        private static DateTime lastWriteTime;
        private static readonly Dictionary<string, string> Settings = new();
        public static string CustomInfoTemplate { get; private set; } = string.Empty;
        public static bool ShowCustomInfo => GetSettingValue<bool>(nameof(ShowCustomInfo));
        public static bool ShowKnightInfo => GetSettingValue<bool>(nameof(ShowKnightInfo));
        public static bool ShowSceneName => GetSettingValue<bool>(nameof(ShowSceneName));
        public static bool ShowTime => GetSettingValue<bool>(nameof(ShowTime));
        public static bool ShowHitbox => GetSettingValue<bool>(nameof(ShowHitbox));
        public static bool ShowHp => GetSettingValue<bool>(nameof(ShowHp));
        public static bool CameraFollow => GetSettingValue<bool>(nameof(CameraFollow));
        public static float CameraZoom => GetSettingValue<float>(nameof(CameraZoom), 1f);

        public static void OnUpdate() {
            TryParseConfigFile();
        }

        private static T GetSettingValue<T>(string settingName, T defaultValue = default) {
            if (Settings.ContainsKey(settingName)) {
                string value = Settings[settingName];
                if (string.IsNullOrEmpty(value)) {
                    return defaultValue;
                }

                return (T) Convert.ChangeType(value, typeof(T));
            } else {
                return defaultValue;
            }
        }

        private static void TryParseConfigFile() {
            if (!File.Exists(ConfigFile)) {
                File.WriteAllText(ConfigFile, defaultContent);
            }

            DateTime writeTime = File.GetLastWriteTime(ConfigFile);
            if (lastWriteTime != writeTime) {
                lastWriteTime = writeTime;
                CustomInfoTemplate = string.Empty;
                Settings.Clear();

                IEnumerable<string> contents = File.ReadAllLines(ConfigFile)
                    .Select(s => s.Trim()).Where(line => !line.StartsWith("#") && !string.IsNullOrEmpty(line));

                bool customInfoSection = false;
                bool settingsSection = false;
                foreach (string content in contents) {
                    switch (content) {
                        case "[Custom_Info_Template]":
                            customInfoSection = true;
                            settingsSection = false;
                            continue;
                        case "[Settings]":
                            customInfoSection = false;
                            settingsSection = true;
                            continue;
                    }

                    if (customInfoSection) {
                        if (string.IsNullOrEmpty(CustomInfoTemplate)) {
                            CustomInfoTemplate = content;
                        } else {
                            CustomInfoTemplate += $"\n{content}";
                        }
                    } else if (settingsSection) {
                        string[] keyValue = content.Split('=').Select(s => s.Trim()).ToArray();
                        if (keyValue.Length != 2) {
                            continue;
                        }
                        Settings[keyValue[0]] = keyValue[1];
                    }
                }
            }
        }
    }
}