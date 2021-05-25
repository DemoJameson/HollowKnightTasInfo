using System;
using System.Collections.Generic;
using System.Text;
using GlobalEnums;
using HollowKnightTasInfo.Utils;
using UnityEngine;

namespace HollowKnightTasInfo {
    internal static class HpInfo {
        private static readonly Dictionary<GameObject, HpData> EnemyPool = new();

        public static void OnInit(GameManager gameManager) {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (scene, nextScene) => {
                EnemyPool.Clear();

                if (gameManager.IsNonGameplayScene()) {
                    return;
                }

                GameObject[] rootGameObjects = nextScene.GetRootGameObjects();
                foreach (GameObject gameObject in rootGameObjects) {
                    TryAddEnemy(gameObject);
                }
            };
        }

        public static void OnUpdate(GameManager gameManager, StringBuilder infoBuilder) {
            if (gameManager.IsNonGameplayScene()
                || gameManager.gameState != GameState.PLAYING
                || gameManager.hero_ctrl?.transitionState == HeroTransitionState.WAITING_TO_ENTER_LEVEL
                || !ConfigManager.ShowHp
            ) {
                return;
            }

            string hpInfo = GetInfo();
            if (!string.IsNullOrEmpty(hpInfo)) {
                infoBuilder.AppendLine(hpInfo);
            }
        }

        public static void TryAddEnemy(GameObject gameObject) {
            if (gameObject == null) {
                return;
            }

            if (!EnemyPool.ContainsKey(gameObject)
                && ((PhysLayers) gameObject.layer is PhysLayers.ENEMIES or PhysLayers.HERO_ATTACK || gameObject.CompareTag("Boss"))
                && !IgnoreObject(gameObject)) {
                PlayMakerFSM playMakerFsm = gameObject.LocateMyFSM("health_manager_enemy");
                if (playMakerFsm == null) {
                    playMakerFsm = gameObject.LocateMyFSM("health_manager");
                }

                if (playMakerFsm != null) {
                    EnemyPool.Add(gameObject, new HpData(gameObject, playMakerFsm));
                }
            }

            foreach (Transform childTransform in gameObject.transform) {
                TryAddEnemy(childTransform.gameObject);
            }
        }

        private static bool IgnoreObject(GameObject gameObject) {
            string name = gameObject.name;
            if (name.IndexOf("Hornet Barb", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (name.IndexOf("Needle Tink", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (name.IndexOf("worm", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (name.IndexOf("Laser Turret", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (name.IndexOf("Deep Spikes", StringComparison.OrdinalIgnoreCase) >= 0) return true;

            return false;
        }

        private static string GetInfo() {
            return HkUtils.Join(",", EnemyPool.Values, "HP=");
        }

        private class HpData {
            private readonly GameObject gameObject;
            private readonly PlayMakerFSM fsm;
            private int Hp => fsm.FsmVariables.GetFsmInt("HP").Value;

            public HpData(GameObject gameObject, PlayMakerFSM fsm) {
                this.gameObject = gameObject;
                this.fsm = fsm;
            }

            public override string ToString() {
                if (Camera.main == null || gameObject == null || !gameObject.activeInHierarchy || Hp <= 0) {
                    return string.Empty;
                }

                Vector2 enemyPos = Camera.main.WorldToScreenPoint(gameObject.transform.position);
                enemyPos.y = Screen.height - enemyPos.y;

                int x = (int) enemyPos.x;
                int y = (int) enemyPos.y;

                if (x < 0 || x > Screen.width || y < 0 || y > Screen.height) {
                    return string.Empty;
                }

                return $"{x},{y},{Hp}";
            }
        }
    }
}