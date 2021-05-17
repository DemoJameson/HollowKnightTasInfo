using System;
using System.Collections.Generic;
using System.Text;
using GlobalEnums;
using HollowKnightTasInfo.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HollowKnightTasInfo {
    public static class HitboxInfo {
        private enum HitboxColor {
            Knight,
            Enemy,
            PeaceMonster,
            Trigger,
            Terrain
        }

        private class HitboxData {
            private readonly Collider2D collider;
            private readonly HitboxColor hitboxColor;

            public HitboxData(Collider2D collider, HitboxColor hitboxColor) {
                this.collider = collider;
                this.hitboxColor = hitboxColor;
            }

            public string Key =>
                collider switch {
                    BoxCollider2D => "BoxHitbox",
                    EdgeCollider2D => "EdgeHitbox",
                    PolygonCollider2D => "PolyHitbox",
                    _ => "BoxHitbox"
                };

            public override string ToString() {
                Camera camera = Camera.main;
                if (camera == null || collider == null || !collider.isActiveAndEnabled) {
                    return string.Empty;
                }

                return collider switch {
                    BoxCollider2D box => ToBoxInfo(box, camera),
                    EdgeCollider2D edge => ToEdgeInfo(edge, camera),
                    PolygonCollider2D poly => ToPolyInfo(poly, camera),
                    _ => string.Empty
                };
            }

            private string ToBoxInfo(BoxCollider2D box, Camera camera) {
                Vector2 offset = box.offset;
                Vector2 halfSize = box.size / 2f;
                Vector2 topLeft = WorldToScreenPoint(camera, box.transform, offset + new Vector2(-halfSize.x, halfSize.y));
                Vector2 bottomRight = WorldToScreenPoint(camera, box.transform, offset + new Vector2(halfSize.x, -halfSize.y));

                int width = (int) Math.Round(Math.Abs(bottomRight.x - topLeft.x));
                int height = (int) Math.Round(Math.Abs(topLeft.y - bottomRight.y));
                int x = (int) Math.Round(Math.Min(topLeft.x, bottomRight.x));
                int y = Screen.height - (int) Math.Round(Math.Max(topLeft.y, bottomRight.y));

                if (x + width < 0 || x > Screen.width || y + height < 0 || y > Screen.height) {
                    return string.Empty;
                }

                return $"{x},{y},{width},{height},{hitboxColor}";
            }

            private string ToEdgeInfo(EdgeCollider2D edge, Camera camera) {
                Vector2 transformPosition = edge.transform.position;
                List<string> result = new();
                for (var i = 0; i < edge.points.Length - 1; i++) {
                    Vector2 point1 = camera.WorldToScreenPoint(transformPosition + edge.points[i]);
                    Vector2 point2 = camera.WorldToScreenPoint(transformPosition + edge.points[i + 1]);

                    if (point1.x > point2.x || point1.y > point2.y) {
                        Vector2 temp = point1;
                        point1 = point2;
                        point2 = temp;
                    }

                    int width = (int) Math.Round(point2.x - point1.x);
                    int height = (int) Math.Round(point2.y - point1.y);

                    width = Math.Max(width, 1);
                    height = Math.Max(height, 1);

                    if (width == 1 && height == 1) {
                        continue;
                    }

                    int x = (int) Math.Round(point1.x);
                    int y = Screen.height - (int) Math.Round(point1.y) - height;

                    if (x + width < 0 || x > Screen.width || y + height < 0 || y > Screen.height) {
                        continue;
                    }

                    result.Add($"{x},{y},{width},{height},{hitboxColor}");
                }

                return HkUtils.Join(",", result);
            }

            private string ToPolyInfo(PolygonCollider2D poly, Camera camera) {
                if (poly.points.Length == 0) {
                    return string.Empty;
                }

                Transform transform = poly.transform;
                List<string> result = new();
                for (var i = 0; i < poly.points.Length; i++) {
                    Vector2 point1 = WorldToScreenPoint(camera, transform, poly.points[i]);
                    int next = i == poly.points.Length - 1 ? 0 : i + 1;
                    Vector2 point2 = WorldToScreenPoint(camera, transform, poly.points[next]);

                    List<Vector2> intersectionPoints = ScreenUtils.GetIntersectionPoint(point1, point2);
                    if (intersectionPoints.Count != 2) {
                        continue;
                    }

                    point1 = intersectionPoints[0];
                    point2 = intersectionPoints[1];

                    int x1 = (int) Math.Round(point1.x);
                    int y1 = Screen.height - (int) Math.Round(point1.y);
                    int x2 = (int) Math.Round(point2.x);
                    int y2 = Screen.height - (int) Math.Round(point2.y);

                    result.Add($"{x1},{y1},{x2},{y2},{hitboxColor}");
                }

                return HkUtils.Join(",", result);
            }

            private static Vector2 WorldToScreenPoint(Camera camera, Transform transform, Vector2 point) {
                Vector3 localScale = transform.localScale;
                Quaternion localRotation = transform.localRotation;
                return camera.WorldToScreenPoint(transform.position + localRotation * new Vector3(point.x * localScale.x, point.y * localScale.y, 0));
            }
        }

        private static readonly Dictionary<Collider2D, HitboxData> Colliders = new();

        public static void OnInit(GameManager gameManager) {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (scene, nextScene) => {
                Colliders.Clear();

                if (gameManager.IsNonGameplayScene()) {
                    return;
                }

                UpdateHitbox();
            };
        }

        public static void OnUpdate(GameManager gameManager, StringBuilder infoBuilder) {
            if (gameManager.IsNonGameplayScene()
                || gameManager.gameState != GameState.PLAYING
                || gameManager.hero_ctrl?.transitionState == HeroTransitionState.WAITING_TO_ENTER_LEVEL
                || HkUtils.InInventory()
            ) {
                return;
            }

            UpdateHitbox();
            string hitboxInfo = GetAllInfo();
            if (!string.IsNullOrEmpty(hitboxInfo)) {
                infoBuilder.AppendLine(hitboxInfo);
            }
        }

        private static void UpdateHitbox() {
            foreach (Collider2D col in Object.FindObjectsOfType<Collider2D>()) {
                if (Colliders.ContainsKey(col)) {
                    continue;
                }

                if (col is BoxCollider2D or PolygonCollider2D or EdgeCollider2D) {
                    GameObject gameObject = col.gameObject;
                    if (gameObject.layer == (int) PhysLayers.TERRAIN) {
                        Colliders.Add(col, new HitboxData(col, HitboxColor.Terrain));
#if V1028
                    } else if (col.gameObject.LocateMyFSM("damages_hero")) {
#elif V1221
                    } else if (col.GetComponent<DamageHero>() || gameObject.LocateMyFSM("damages_hero")) {
#endif
                        Colliders.Add(col, new HitboxData(col, HitboxColor.Enemy));
                    } else if (gameObject.LocateMyFSM("health_manager_enemy") || gameObject.LocateMyFSM("health_manager")) {
                        Colliders.Add(col, new HitboxData(col, HitboxColor.PeaceMonster));
                    } else if (gameObject == HeroController.instance.gameObject && !col.isTrigger) {
                        Colliders.Add(col, new HitboxData(col, HitboxColor.Knight));
                    } else if (col.GetComponent<TransitionPoint>()
                               || col.isTrigger && col.GetComponent<HazardRespawnTrigger>()
                    ) {
                        Colliders.Add(col, new HitboxData(col, HitboxColor.Trigger));
                    }
                }
            }
        }

        private static string GetAllInfo() {
            Dictionary<string, StringBuilder> results = new();

            foreach (HitboxData hitboxData in Colliders.Values) {
                string hitboxInfo = hitboxData.ToString();

                if (string.IsNullOrEmpty(hitboxInfo)) {
                    continue;
                }

                StringBuilder result;
                if (results.ContainsKey(hitboxData.Key)) {
                    result = results[hitboxData.Key];
                } else {
                    result = new StringBuilder();
                    results[hitboxData.Key] = result;
                }

                result.Append(result.Length > 0 ? $",{hitboxInfo}" : $"{hitboxData.Key}={hitboxInfo}");
            }

            return HkUtils.Join("\n", results.Values);
        }
    }
}