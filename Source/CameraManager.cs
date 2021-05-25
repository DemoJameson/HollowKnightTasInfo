using System;
using HollowKnightTasInfo.Extensions;
using UnityEngine;

namespace HollowKnightTasInfo {
    public static class CameraManager {
        private static Vector3? cameraControllerPosition;
        private static Vector3? cameraTargetPosition;
        private static float? zoomFactor;

        public static void OnPreRender() {
            if (!ConfigManager.CameraFollow && (Math.Abs(ConfigManager.CameraZoom - 1f) < 0.001 || ConfigManager.CameraZoom <= 0f)) {
                return;
            }

            if (ConfigManager.CameraFollow) {
                if (GameManager._instance is not { } gameManager) {
                    return;
                }

                if (gameManager.hero_ctrl is not { } heroCtrl) {
                    return;
                }

                if (gameManager.cameraCtrl is not { } cameraCtrl) {
                    return;
                }

                if (cameraCtrl.GetFieldValue<CameraTarget>("camTarget") is not { } camTarget) {
                    return;
                }

                cameraControllerPosition = cameraCtrl.transform.position;
                cameraTargetPosition = camTarget.transform.position;

                Vector3 heroPosition = heroCtrl.transform.position;
                camTarget.transform.position = new Vector3(heroPosition.x, heroPosition.y, camTarget.transform.position.z);
                cameraCtrl.transform.position = new Vector3(heroPosition.x, heroPosition.y, cameraCtrl.transform.position.z);
            }

            if (ConfigManager.CameraZoom > 0f && Math.Abs(ConfigManager.CameraZoom - 1f) > 0.001) {
                // zoomFactor = GameCameras.instance.tk2dCam.ZoomFactor;
                // GameCameras.instance.tk2dCam.ZoomFactor *= ConfigManager.CameraZoom;
            }
        }

        public static void OnPostRender() {
            if (cameraControllerPosition != null || cameraTargetPosition != null) {
                if (GameManager._instance is not { } gameManager) {
                    return;
                }

                if (gameManager.cameraCtrl is not { } cameraCtrl) {
                    return;
                }

                if (cameraCtrl.GetFieldValue<CameraTarget>("camTarget") is not { } cameraTarget) {
                    return;
                }

                if (cameraControllerPosition != null) {
                    cameraCtrl.transform.position = cameraControllerPosition.Value;
                    cameraControllerPosition = null;
                }

                if (cameraTargetPosition != null) {
                    cameraTarget.transform.position = cameraTargetPosition.Value;
                    cameraTargetPosition = null;
                }
            }

            if (zoomFactor != null) {
                // GameCameras.instance.tk2dCam.ZoomFactor = zoomFactor.Value;
            }
        }
    }
}