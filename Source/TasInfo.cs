using System;
using System.Text;
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

            DesyncChecker.BeforeUpdate();

            StringBuilder infoBuilder = new();
            try {
                if (!init) {
                    init = true;
                    OnInit(gameManager);
                }

                OnUpdate(gameManager, infoBuilder);
            } catch (Exception e) {
                Debug.LogException(e);
            }

            DesyncChecker.AfterUpdate(infoBuilder);

            GameManager.Info = infoBuilder.ToString();
        }

        // ReSharper disable once UnusedMember.Global
        public static void OnColliderCreate(GameObject gameObject) {
            HitboxInfo.UpdateHitbox(gameObject);
            HpInfo.TryAddEnemy(gameObject);
        }

        private static void OnInit(GameManager gameManager) {
            HpInfo.OnInit(gameManager);
            HitboxInfo.OnInit(gameManager);
            // RngInfo.OnInit();
            CustomInfo.OnInit();
#if DEBUG
            ShowHitboxes.Instance.Initialize();
#endif
        }

        private static void OnUpdate(GameManager gameManager, StringBuilder infoBuilder) {
            HeroInfo.OnUpdate(gameManager, infoBuilder);
            HpInfo.OnUpdate(gameManager, infoBuilder);
            HitboxInfo.OnUpdate(gameManager, infoBuilder);
            // RngInfo.OnUpdate(infoBuilder);
            CustomInfo.OnUpdate(gameManager, infoBuilder);
            TimeInfo.OnUpdate(gameManager, infoBuilder);
#if DEBUG
            ShowHitboxes.Instance.UpdateHitboxes();
#endif
        }
    }
}