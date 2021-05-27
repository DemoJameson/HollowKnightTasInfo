using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HollowKnightTasInfo.Extensions {
    internal static class ReflectionExtensions {
        private const BindingFlags Flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> CachedFieldInfos = new();
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> CachedPropertyInfos = new();
        private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> CachedMethodInfos = new();

        public static FieldInfo GetFieldInfo(this Type type, string name) {
            if (!CachedFieldInfos.ContainsKey(type)) {
                CachedFieldInfos[type] = new Dictionary<string, FieldInfo>();
            }

            if (!CachedFieldInfos[type].ContainsKey(name)) {
                return CachedFieldInfos[type][name] = type.GetField(name, Flags);
            } else {
                return CachedFieldInfos[type][name];
            }
        }

        public static PropertyInfo GetPropertyInfo(this Type type, string name) {
            if (!CachedPropertyInfos.ContainsKey(type)) {
                CachedPropertyInfos[type] = new Dictionary<string, PropertyInfo>();
            }

            if (!CachedPropertyInfos[type].ContainsKey(name)) {
                return CachedPropertyInfos[type][name] = type.GetProperty(name, Flags);
            } else {
                return CachedPropertyInfos[type][name];
            }
        }

        public static MethodInfo GetMethodInfo(this Type type, string name, params Type[] types) {
            if (!CachedMethodInfos.ContainsKey(type)) {
                CachedMethodInfos[type] = new Dictionary<string, MethodInfo>();
            }

            string keyName = types.Length > 0 ? $"{name}-{string.Join("-", types.Select(t => t.FullName).ToArray())}" : name;
            
            if (!CachedMethodInfos[type].ContainsKey(keyName)) {
                try {
                    return CachedMethodInfos[type][keyName] = type.GetMethod(name, Flags, null, types, null);
                } catch {
                    return CachedMethodInfos[type][keyName] = null;
                }
            } else {
                return CachedMethodInfos[type][keyName];
            }
        }

        public static T GetFieldValue<T>(this object obj, string name) {
            object result = obj.GetType().GetFieldInfo(name)?.GetValue(obj);
            if (result == null) {
                return default;
            } else {
                return (T) result;
            }
        }

        public static T GetFieldValue<T>(this Type type, string name) {
            object result = type.GetFieldInfo(name)?.GetValue(null);
            if (result == null) {
                return default;
            } else {
                return (T) result;
            }
        }

        public static void SetFieldValue(this object obj, string name, object value) {
            obj.GetType().GetFieldInfo(name)?.SetValue(obj, value);
        }

        public static void SetFieldValue(this Type type, string name, object value) {
            type.GetFieldInfo(name)?.SetValue(null, value);
        }

        public static T GetPropertyValue<T>(this object obj, string name) {
            object result = obj.GetType().GetPropertyInfo(name)?.GetValue(obj, null);
            if (result == null) {
                return default;
            } else {
                return (T) result;
            }
        }

        public static T GetPropertyValue<T>(Type type, string name) {
            object result = type.GetPropertyInfo(name)?.GetValue(null, null);
            if (result == null) {
                return default;
            } else {
                return (T) result;
            }
        }

        public static void SetPropertyValue(this object obj, string name, object value) {
            if (obj.GetType().GetPropertyInfo(name) is {CanWrite: true} propertyInfo) {
                propertyInfo.SetValue(obj, value, null);
            }
        }

        public static void SetPropertyValue(this Type type, string name, object value) {
            if (type.GetPropertyInfo(name) is {CanWrite: true} propertyInfo) {
                propertyInfo.SetValue(null, value, null);
            }
        }

        public static T InvokeMethod<T>(this object obj, string name, params object[] parameters) {
            object result = obj.GetType().GetMethodInfo(name)?.Invoke(obj, parameters);
            if (result == null) {
                return default;
            } else {
                return (T) result;
            }
        }

        public static T InvokeMethod<T>(this Type type, string name, params object[] parameters) {
            object result = type.GetMethodInfo(name)?.Invoke(null, parameters);
            if (result == null) {
                return default;
            } else {
                return (T) result;
            }
        }

        public static void InvokeMethod(this object obj, string name, params object[] parameters) {
            obj.GetType().GetMethodInfo(name)?.Invoke(obj, parameters);
        }

        public static void InvokeMethod(this Type type, string name, params object[] parameters) {
            type.GetMethodInfo(name)?.Invoke(null, parameters);
        }
    }
}