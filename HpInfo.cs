using System;
using System.Collections.Generic;
using System.Text;
using GlobalEnums;
using UnityEngine;

namespace HollowKnightTasInfo {
    public record HpData {
        private readonly GameObject gameObject;
        private readonly PlayMakerFSM fsm;
        private readonly int maxHp;
        private int Hp => fsm.FsmVariables.GetFsmInt("HP").Value;

        public HpData(GameObject gameObject, PlayMakerFSM fsm) {
            this.gameObject = gameObject;
            this.fsm = fsm;
            maxHp = Hp;
        }

        public override string ToString() {
            if (Camera.main == null || !gameObject.activeSelf || Hp <= 0) {
                return string.Empty;
            }

            Vector2 enemyPos = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            enemyPos.y = Screen.height - enemyPos.y;

            int x = (int)enemyPos.x;
            int y = (int)enemyPos.y;

            if (x < 0 || x > Screen.width || y < 0 || y > Screen.height) {
                return string.Empty;
            }

            return $"{x},{y},{Hp}/{maxHp}";
        }
    }

    public static class HpInfo {
        private static bool init;
        private static readonly Dictionary<GameObject, HpData> EnemyPool = new();

        public static void Init(GameManager gameManager) {
            if (init) {
                return;
            }

            init = true;

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (scene, nextScene) => {
                EnemyPool.Clear();

                if (gameManager.IsNonGameplayScene()) {
                    return;
                }

                GameObject[] rootGameObjects = nextScene.GetRootGameObjects();
                if (rootGameObjects != null) {
                    foreach (GameObject gameObject in rootGameObjects) {
                        TryAddEnemy(gameObject);
                    }
                }
            };
        }

        private static void TryAddEnemy(GameObject gameObject) {
            if (((PhysLayers) gameObject.layer is PhysLayers.ENEMIES or PhysLayers.HERO_ATTACK || gameObject.CompareTag("Boss")) &&
                !IgnoreObject(gameObject.name) && !EnemyPool.ContainsKey(gameObject)) {
                PlayMakerFSM playMakerFsm = FSMUtility.LocateFSM(gameObject, "health_manager_enemy");
                if (playMakerFsm == null) {
                    playMakerFsm = FSMUtility.LocateFSM(gameObject, "health_manager");
                }

                if (playMakerFsm != null) {
                    EnemyPool.Add(gameObject, new HpData(gameObject, playMakerFsm));
                }
            }

            EnemyDescendants(gameObject.transform);
        }

        private static void EnemyDescendants(Transform transform) {
            foreach (Transform childTransform in transform) {
                TryAddEnemy(childTransform.gameObject);
            }
        }

        private static bool IgnoreObject(string name) {
            if (name.IndexOf("Hornet Barb", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (name.IndexOf("Needle Tink", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (name.IndexOf("worm", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (name.IndexOf("Laser Turret", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (name.IndexOf("Deep Spikes", StringComparison.OrdinalIgnoreCase) >= 0) return true;

            return false;
        }

        public static string GetInfo() {
            StringBuilder result = new();

            if (EnemyPool.Count > 0) {
                foreach (HpData hpData in EnemyPool.Values) {
                    string hpInfo = hpData.ToString();
                    if (string.IsNullOrEmpty(hpInfo)) {
                        continue;
                    }

                    if (result.Length == 0) {
                        result.Append("HP:");
                    }

                    result.Append($"{hpData},");
                }

                if (result.Length > 0) {
                    result.Remove(result.Length - 1, 1);
                }
            }

            return result.ToString();
        }
    }
}