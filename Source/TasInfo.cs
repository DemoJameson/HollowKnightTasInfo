using System;
using System.Text;
using HollowKnightTasInfo.Utils;
using UnityEngine;

namespace HollowKnightTasInfo {
    // ReSharper disable once UnusedType.Global
    public static class TasInfo {
        private static bool init;

        // ReSharper disable once UnusedMember.Global
        // After CameraController.LateUpdate
        public static void Update() {
            if (GameManager._instance is not { } gameManager) {
                return;
            }

            StringBuilder infoBuilder = new();

            try {
                DesyncChecker.BeforeUpdate();
                if (!init) {
                    init = true;
                    OnInit(gameManager);
                }

                OnUpdate(gameManager, infoBuilder);

                DesyncChecker.AfterUpdate(infoBuilder);
            } catch (Exception e) {
                Debug.LogException(e);
            }

            GameManager.TasInfo = infoBuilder.ToString();
        }

        // ReSharper disable once UnusedMember.Global
        public static void OnColliderCreate(GameObject gameObject) {
            HitboxInfo.UpdateHitbox(gameObject);
            HpInfo.TryAddEnemy(gameObject);
        }

        private static void OnInit(GameManager gameManager) {
            HpInfo.OnInit(gameManager);
            CustomInfo.OnInit();
            HitboxInfo.OnInit(gameManager);
        }

        private static void OnUpdate(GameManager gameManager, StringBuilder infoBuilder) {
            // 放第一位，先更新 settings
            ConfigManager.OnUpdate();
            
            HeroInfo.OnUpdate(gameManager, infoBuilder);
            CustomInfo.OnUpdate(gameManager, infoBuilder);
            TimeInfo.OnUpdate(gameManager, infoBuilder);
            HpInfo.OnUpdate(gameManager, infoBuilder);
            HitboxInfo.OnUpdate(gameManager, infoBuilder);
        }
    }
}