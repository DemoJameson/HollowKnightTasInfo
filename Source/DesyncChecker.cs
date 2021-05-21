using System.Text;
using UnityEngine;

namespace HollowKnightTasInfo {
    internal static class DesyncChecker {
        private static Random.State savedRandomState;
        private static int desyncFrame = 0;
        public static void BeforeUpdate() {
            savedRandomState = Random.state;
        }

        public static void AfterUpdate(StringBuilder infoBuilder) {
            if (desyncFrame == 0 && !savedRandomState.Equals(Random.state)) {
                desyncFrame = Time.frameCount;
            }

            if (desyncFrame > 0) {
                infoBuilder.Append($"desync at frame {desyncFrame}");
            }
        }
    }
}