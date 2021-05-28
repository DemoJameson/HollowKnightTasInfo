using System.Collections.Generic;
using System.Text;

namespace Assembly_CSharp.TasInfo.mm.Source.Utils {
    internal static class StringUtils {
        public static string Join<T>(string separator, IEnumerable<T> values, string prefix = "") {
            separator ??= "";

            bool firstValue = true;
            StringBuilder stringBuilder = new();
            foreach (T value in values) {
                if (string.IsNullOrEmpty(value?.ToString())) {
                    continue;
                }

                stringBuilder.Append(firstValue ? prefix : separator);
                firstValue = false;
                stringBuilder.Append(value);
            }

            return stringBuilder.ToString();
        }
    }
}