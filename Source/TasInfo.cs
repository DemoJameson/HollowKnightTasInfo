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
            CustomInfo.OnInit();
#if DEBUG
            ShowHitboxes.Instance.Initialize();
#endif
        }

        private static void OnUpdate(GameManager gameManager, StringBuilder infoBuilder) {
            HeroInfo.OnUpdate(gameManager, infoBuilder);
            HpInfo.OnUpdate(gameManager, infoBuilder);
            HitboxInfo.OnUpdate(gameManager, infoBuilder);
            CustomInfo.OnUpdate(gameManager, infoBuilder);
            TimeInfo.OnUpdate(gameManager, infoBuilder);
#if DEBUG
            ShowHitboxes.Instance.UpdateHitboxes();
#endif
        }
    }
}