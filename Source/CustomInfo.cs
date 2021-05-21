using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HollowKnightTasInfo.Extensions;
using HollowKnightTasInfo.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HollowKnightTasInfo {
    internal static class CustomInfo {
        private static readonly Regex BraceRegex = new(@"\{(.+?)\}");
        private static readonly Dictionary<string, Type> CachedTypes = new();

        public static void OnInit() {
            foreach (Type type in typeof(GameManager).Assembly.GetTypes()) {
                if (type.FullName != null) {
                    CachedTypes[type.FullName] = type;
                }
            }
        }

        public static void OnUpdate(GameManager gameManager, StringBuilder infoBuilder) {
            string customTemplate = ConfigUtils.GetCustomInfoTemplate();
            if (string.IsNullOrEmpty(customTemplate)) {
                return;
            }

            string result = BraceRegex.Replace(customTemplate, match => {
                string matchText = match.Groups[1].Value;
                string[] splitText = matchText.Split('.').Select(s => s.Trim()).ToArray();
                if (splitText.Length <= 1) {
                    return "invalid template";
                }

                string typeNameOrObjectName = splitText[0];

                Object obj;
                if (CachedTypes.ContainsKey(typeNameOrObjectName) && CachedTypes[typeNameOrObjectName] is { } type &&
                    type.IsSubclassOf(typeof(Object))) {
                    obj = typeNameOrObjectName switch {
                        "GameManager" => gameManager,
                        "HeroController" => gameManager.hero_ctrl,
                        _ => Object.FindObjectOfType(type)
                    };
                } else {
                    obj = GameObject.Find(typeNameOrObjectName);
                }

                if (obj != null) {
                    return FormatValue(GetMemberValue(obj, splitText.Skip(1)));
                }

                return string.Empty;
            });

            if (!string.IsNullOrEmpty(result)) {
                infoBuilder.AppendLine(result);
            }
        }



        private static string FormatValue(object obj) {
            return obj switch {
                null => string.Empty,
                Vector2 vector2 => vector2.ToSimpleString(3),
                float floatValue => $"{floatValue:F3}",
                _ => obj.ToString()
            };
        }

        private static object GetMemberValue(object obj, IEnumerable<string> memberNames) {
            object result = obj;
            foreach (string memberName in memberNames) {
                Type objType = result.GetType();
                if (memberName.EndsWith("()")) {
                    if (objType.GetMethodInfo(memberName.Replace("()", "")) is { } methodInfo) {
                        result = methodInfo.Invoke(result, null);
                    } else {
                        return null;
                    }
                } else if (objType.GetPropertyInfo(memberName) is { } propertyInfo) {
                    result = propertyInfo.GetValue(result, null);
                } else if (objType.GetFieldInfo(memberName) is { } fieldInfo) {
                    result = fieldInfo.GetValue(result);
                } else {
                    return $"{memberName} not found";
                }

                if (result == null) {
                    return null;
                }
            }

            return result;
        }
    }
}