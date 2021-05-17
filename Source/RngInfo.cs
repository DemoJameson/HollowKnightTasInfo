using System.Text;
using UnityEngine;

namespace HollowKnightTasInfo {
    public static class RngInfo {
        private static ulong rollTimes = 0;
        private static Random.State lastState;

        public static void OnInit() {
            lastState = Random.state;
        }
        
        public static void OnUpdate(StringBuilder infoBuilder) {
            Random.State origState = Random.state;
            Random.state = lastState;
            while (!origState.Equals(Random.state)) {
                float _ = Random.value;
                rollTimes++;
            }
            lastState = Random.state;

            infoBuilder.AppendLine($"RNG: {rollTimes}");

            Random.state = origState;
        }
    }
}