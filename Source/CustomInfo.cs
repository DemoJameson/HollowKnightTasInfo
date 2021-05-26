using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HollowKnightTasInfo.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HollowKnightTasInfo {
    internal static class CustomInfo {
        private static readonly Regex BraceRegex = new(@"\{(.+?)\}");
        private static readonly Regex MethodRegex = new(@"^(\w+)\((.*?)\)$");
        private static readonly Dictionary<string, Type> CachedTypes = new();
        private static readonly Dictionary<string, object> CachedObjects = new();

        public static void OnInit() {
            foreach (Type type in typeof(GameManager).Assembly.GetTypes()) {
                if (type.FullName != null) {
                    CachedTypes[type.FullName] = type;
                }
            }
        }

        public static void OnUpdate(GameManager gameManager, StringBuilder infoBuilder) {
            string customTemplate = ConfigManager.CustomInfoTemplate;
            if (!ConfigManager.ShowCustomInfo || string.IsNullOrEmpty(customTemplate)) {
                return;
            }

            CachedObjects.Clear();

            string result = BraceRegex.Replace(customTemplate, match => {
                string matchText = match.Groups[1].Value;
                string[] splitText = matchText.Split('.').Select(s => s.Trim()).ToArray();
                if (splitText.Length <= 1) {
                    return "invalid template";
                }

                string typeNameOrObjectName = splitText[0];

                object obj;

                if (CachedObjects.ContainsKey(typeNameOrObjectName)) {
                    obj = CachedObjects[typeNameOrObjectName];
                } else {
                    obj = typeNameOrObjectName switch {
                        "GameManager" => gameManager,
                        "HeroController" => gameManager.hero_ctrl,
                        "PlayerData" => gameManager.hero_ctrl?.playerData,
                        "HeroControllerStates" => gameManager.hero_ctrl?.cState,
                        _ => null
                    };

                    if (obj == null) {
                        if (CachedTypes.TryGetValue(typeNameOrObjectName, out Type type) && type.IsSubclassOf(typeof(Object))) {
                            obj = Object.FindObjectOfType(type);
                        } else {
                            obj = GameObject.Find(typeNameOrObjectName);
                        }
                    }
                }

                if (obj != null) {
                    CachedObjects[typeNameOrObjectName] = obj;
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
                if (objType.GetPropertyInfo(memberName) is { } propertyInfo) {
                    result = propertyInfo.GetValue(result, null);
                } else if (objType.GetFieldInfo(memberName) is { } fieldInfo) {
                    result = fieldInfo.GetValue(result);
                } else if (MethodRegex.IsMatch(memberName)) {
                    Match match = MethodRegex.Match(memberName);
                    string methodName = match.Groups[1].Value;
                    object arg = match.Groups[2].Value;
                    if (string.Empty.Equals(arg)) {
                        result = result.InvokeMethod<object>(methodName);
                    } else if (result is GameObject gameObject) {
                        if (methodName == "LocateMyFSM") {
                            result = FSMUtility.LocateFSM(gameObject, arg.ToString());
                        } else if (methodName == "GetComponent") {
                            result = gameObject.GetComponent(arg.ToString());
                        }  else {
                            result = result.InvokeMethod<object>(methodName, arg);
                        }
                    } else {
                        result = result.InvokeMethod<object>(methodName, arg);
                    }
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