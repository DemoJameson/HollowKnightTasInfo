using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HollowKnightTasInfo.Utils {
    internal static class ConfigUtils {
        private const string ConfigFile = "./HollowKnightTasInfo.config";

        private static string defaultContent = @"# 该配置用于定制附加显示的数据，需要注意如果调用属性或者方法有可能会造成 desync
# 例如 HeroController.CanJump() 会修改 ledgeBufferSteps 字段，请查看源码确认是否安全。定制数据格式如下：
# {UnityObject子类名.字段/属性/方法.字段/属性/方法……}，只支持无参方法需要以()结尾
# {GameObjectName.字段/属性/方法.字段/属性/方法……}
# 支持配置多行
# 请不要 [Custom_Info_Template] 这一行

[Custom_Info_Template]
# canAttack: {HeroController.CanAttack()}
# geo: {HeroController.playerData.geo}";

        private static DateTime lastWriteTime;
        private static string customInfoTemplate = string.Empty;

        private static void ParseConfigFile() {
            if (!File.Exists(ConfigFile)) {
                File.WriteAllText(ConfigFile, defaultContent);
            }

            DateTime writeTime = File.GetLastWriteTime(ConfigFile);
            if (lastWriteTime != writeTime) {
                lastWriteTime = writeTime;
                customInfoTemplate = string.Empty;

                IEnumerable<string> contents = File.ReadAllLines(ConfigFile)
                    .Select(s => s.Trim()).Where(line => !line.StartsWith("#") && !string.IsNullOrEmpty(line));

                bool templateSection = false;
                foreach (string content in contents) {
                    if (content.StartsWith("[Custom_Info_Template]", StringComparison.OrdinalIgnoreCase)) {
                        templateSection = true;
                        continue;
                    }

                    if (templateSection) {
                        if (string.IsNullOrEmpty(customInfoTemplate)) {
                            customInfoTemplate = content;
                        } else {
                            customInfoTemplate += $"\n{content}";
                        }
                    }
                }
            }
        }

        public static string GetCustomInfoTemplate() {
            ParseConfigFile();
            return customInfoTemplate;
        }
    }
}