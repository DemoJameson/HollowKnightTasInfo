using System;
using UnityEngine;

namespace HollowKnightTasInfo {
    internal static class CameraManager {
        private static Vector2? cameraOffset;

        public static Vector2 CameraOffset {
            get {
                if (cameraOffset != null) {
                    return cameraOffset.Value;
                }

                if (ConfigManager.CameraFollow && GameManager._instance.hero_ctrl is { } heroCtrl &&
                    GameManager._instance.cameraCtrl is { } cameraCtrl) {
                    cameraOffset = cameraCtrl.transform.position - heroCtrl.transform.position;
                } else {
                    cameraOffset = Vector2.zero;
                }
                
                return cameraOffset ?? Vector2.zero;
            }
        }

        private static Vector3? cameraControllerPosition;
        private static float? fieldOfView;

        public static void OnPreRender(GameManager gameManager) {
            if (gameManager.IsNonGameplayScene()) {
                return;
            }

            if (gameManager.cameraCtrl is not { } cameraCtrl) {
                return;
            }

            if (ConfigManager.CameraFollow) {
                if (gameManager.hero_ctrl is not { } heroCtrl) {
                    return;
                }

                cameraControllerPosition = cameraCtrl.transform.position;
                Vector3 heroPosition = heroCtrl.transform.position;
                cameraCtrl.transform.position = new Vector3(heroPosition.x, heroPosition.y, cameraCtrl.transform.position.z);
            }

            if (ConfigManager.IsCameraZoom) {
                fieldOfView = cameraCtrl.cam.fieldOfView;
                cameraCtrl.cam.fieldOfView *= ConfigManager.CameraZoom;
            }
        }

        public static void OnPostRender(GameManager gameManager) {
            cameraOffset = null;
            if (gameManager.cameraCtrl is not { } cameraCtrl) {
                return;
            }

            if (cameraControllerPosition != null) {
                cameraCtrl.transform.position = cameraControllerPosition.Value;
                cameraControllerPosition = null;
            }

            if (fieldOfView != null) {
                cameraCtrl.cam.fieldOfView = fieldOfView.Value;
                fieldOfView = null;
            }
        }
    }
}