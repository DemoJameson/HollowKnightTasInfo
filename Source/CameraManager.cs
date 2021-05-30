using UnityEngine;

namespace Assembly_CSharp.TasInfo.mm.Source {
    internal static class CameraManager {
        private static Vector3? cameraControllerPosition;
        private static Vector3? cameraParentPosition;

        public static void OnPreRender(GameManager gameManager) {
            if (gameManager.IsNonGameplayScene()) {
                return;
            }

            if (gameManager.cameraCtrl is not { } cameraCtrl) {
                return;
            }

            Transform cameraCtrlTransform = cameraCtrl.transform;

            if (ConfigManager.IsCameraZoom || ConfigManager.CameraFollow) {
                cameraControllerPosition = cameraCtrlTransform.position;

                if (ConfigManager.IsCameraZoom) {
                    Vector3 position = cameraCtrlTransform.position;
                    cameraCtrlTransform.position = new Vector3(position.x, position.y, position.z * ConfigManager.CameraZoom);
                }

                if (ConfigManager.CameraFollow && gameManager.hero_ctrl is {} heroCtrl) {
                    Vector3 heroPosition = heroCtrl.transform.position;
                    cameraCtrlTransform.position = new Vector3(heroPosition.x, heroPosition.y, cameraCtrlTransform.position.z);
                }
            }

            if (!ConfigManager.CameraFollow && ConfigManager.DisableCameraShake && GameCameras.instance.cameraParent is {} cameraParent) {
                cameraParentPosition = cameraParent.position;
                cameraParent.position = Vector3.zero;
            }
        }

        public static void OnPostRender(GameManager gameManager) {
            if (gameManager.cameraCtrl is not { } cameraCtrl) {
                return;
            }

            if (cameraControllerPosition != null) {
                cameraCtrl.transform.position = cameraControllerPosition.Value;
                cameraControllerPosition = null;
            }

            if (cameraParentPosition != null && GameCameras.instance.cameraParent is {} cameraParent) {
                cameraParent.position = cameraParentPosition.Value;
                cameraParentPosition = null;
            }
        }
    }
}