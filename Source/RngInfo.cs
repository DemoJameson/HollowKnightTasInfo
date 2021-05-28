using System.Text;
using UnityEngine;

namespace Assembly_CSharp.TasInfo.mm.Source {
    public static class RngInfo {
        private static ulong rollTimes = 0;
        private static Random.State lastState;

        public static void OnInit() {
            lastState = Random.state;
        }

        public static void OnPreRender(StringBuilder infoBuilder) {
            Random.State origState = Random.state;
            Random.state = lastState;
            int increaseTimes = 0;
            while (!origState.Equals(Random.state)) {
                float _ = Random.value;
                rollTimes++;
                increaseTimes++;
            }
            lastState = Random.state;

            if (ConfigManager.ShowRng) {
                infoBuilder.AppendLine($"RNG: {rollTimes} +{increaseTimes}");
            }

            Random.state = origState;
        }
    }
}