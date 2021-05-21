using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HollowKnightTasInfo.Utils {
    internal static class HkUtils {
        public static bool InInventory() {
            GameObject inventoryTop = GameObject.FindGameObjectWithTag("Inventory Top");
            if (inventoryTop == null) {
                return false;
            }

            PlayMakerFSM playMakerFsm = FSMUtility.LocateFSM(inventoryTop, "Inventory Control");
            if (playMakerFsm == null) {
                return false;
            }

            return playMakerFsm.FsmVariables.GetFsmBool("Open").Value;
        }

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

        public static int Clamp(int value, int minValue, int maxValue) {
            if (value < minValue) {
                return minValue;
            }

            if (value > maxValue) {
                return maxValue;
            }

            return value;
        }
    }
}