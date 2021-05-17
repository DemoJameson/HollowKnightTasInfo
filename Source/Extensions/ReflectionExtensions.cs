using System;
using System.Collections.Generic;
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
        
        public static MethodInfo GetMethodInfo(this Type type, string name) {
            if (!CachedMethodInfos.ContainsKey(type)) {
                CachedMethodInfos[type] = new Dictionary<string, MethodInfo>();
            }

            if (!CachedMethodInfos[type].ContainsKey(name)) {
                return CachedMethodInfos[type][name] = type.GetMethod(name, Flags);
            } else {
                return CachedMethodInfos[type][name];
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
        
        public static void SetFieldValue(this object obj, string name, object value) {
            obj.GetType().GetFieldInfo(name)?.SetValue(obj, value);
        }
        
        public static T GetPropertyValue<T>(this object obj, string name) {
            object result = obj.GetType().GetPropertyInfo(name)?.GetValue(obj, null);
            if (result == null) {
                return default;
            } else {
                return (T) result;
            }
        }
        
        public static void SetPropertyValue(this object obj, string name, object value) {
            obj.GetType().GetPropertyInfo(name)?.SetValue(obj, value, null);
        }
        
        public static T InvokeMethod<T>(this object obj, string name, params object[] parameters) {
            object result = obj.GetType().GetMethodInfo(name)?.Invoke(obj, parameters);
            if (result == null) {
                return default;
            } else {
                return (T) result;
            }
        }
    }
}