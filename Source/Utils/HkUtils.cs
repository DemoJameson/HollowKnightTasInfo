using UnityEngine;

namespace Assembly_CSharp.TasInfo.mm.Source.Utils {
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

            return playMakerFsm.FsmVariables.FindFsmBool("Open").Value;
        }
    }
}