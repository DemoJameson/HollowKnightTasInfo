using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalEnums;
using HollowKnightTasInfo.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HollowKnightTasInfo {
    internal static class HitboxInfo {
        private enum HitboxColor {
            Knight,
            Enemy,
            PeaceMonster,
            Trigger,
            Terrain
        }

        private class HitboxData {
            private readonly Collider2D collider;
            private HitboxColor hitboxColor;

            public HitboxData(Collider2D collider, HitboxColor hitboxColor) {
                this.collider = collider;
                this.hitboxColor = hitboxColor;
            }

            public string Key =>
                collider switch {
                    BoxCollider2D or EdgeCollider2D or PolygonCollider2D  => "LineHitbox",
                    CircleCollider2D => "CircleHitbox",
                    _ => "LineHitbox"
                };

            public override string ToString() {
                Camera camera = Camera.main;
                if (camera == null || collider == null || !collider.isActiveAndEnabled) {
                    return string.Empty;
                }

                if (hitboxColor == HitboxColor.PeaceMonster && IsDamageHero(collider)) {
                    hitboxColor = HitboxColor.Enemy;
                }

                return collider switch {
                    BoxCollider2D box => ToBoxInfo(box, camera),
                    EdgeCollider2D edge => ToEdgeInfo(edge, camera),
                    PolygonCollider2D poly => ToPolyInfo(poly, camera),
                    CircleCollider2D circle => ToCircleInfo(circle, camera),
                    _ => string.Empty
                };
            }

            private string ToBoxInfo(BoxCollider2D box, Camera camera) {
                Vector2 offset = box.offset;
                Vector2 halfSize = box.size / 2f;
                Vector2 topLeft = offset + new Vector2(-halfSize.x, halfSize.y);
                Vector2 topRight = offset + halfSize;
                Vector2 bottomLeft = offset - halfSize;
                Vector2 bottomRight = offset + new Vector2(halfSize.x, -halfSize.y);
                return ToLineInfo(camera, box.transform, new List<Vector2> {
                    topLeft, topRight, bottomRight, bottomLeft, topLeft
                });
            }

            private string ToEdgeInfo(EdgeCollider2D edge, Camera camera) {
                if (edge.points.Length == 0) {
                    return string.Empty;
                }

                return ToLineInfo(camera, edge.transform, edge.points.ToList());
            }

            private string ToPolyInfo(PolygonCollider2D poly, Camera camera) {
                if (poly.points.Length == 0) {
                    return string.Empty;
                }

                List<Vector2> points = new(poly.points);
                points.Add(points[0]);
                return ToLineInfo(camera, poly.transform, points);
            }

            private string ToLineInfo(Camera camera, Transform transform, List<Vector2> points) {
                List<string> result = new();
                for (var i = 0; i < points.Count - 1; i++) {
                    Vector2 point1 = WorldToScreenPoint(camera, transform, points[i]);
                    Vector2 point2 = WorldToScreenPoint(camera, transform, points[i+1]);

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


            private string ToCircleInfo(CircleCollider2D circle, Camera camera) {
                Vector2 offset = circle.offset;
                Vector2 center = WorldToScreenPoint(camera, circle.transform, offset);
                Vector2 centerRight = WorldToScreenPoint(camera, circle.transform, new Vector2(offset.x + circle.radius, offset.y));
                int radius = (int) Math.Abs(Math.Round(centerRight.x - center.x));
                int x = (int) Math.Round(center.x);
                int y = Screen.height - (int) Math.Round(center.y);

                if (x + radius < 0 || x - radius > Screen.width || y + radius < 0 || y - radius > Screen.height || radius == 0 || radius > Screen.height) {
                    return string.Empty;
                }

                return $"{x},{y},{radius},{hitboxColor}";
            }

            private static Vector2 WorldToScreenPoint(Camera camera, Transform transform, Vector2 point) {
                return camera.WorldToScreenPoint(transform.position + transform.localRotation * Vector3.Scale(point, transform.localScale));
            }
        }

        private static readonly Dictionary<Collider2D, HitboxData> Colliders = new();

        public static void OnInit(GameManager gameManager) {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (scene, nextScene) => {
                Colliders.Clear();

                if (gameManager.IsNonGameplayScene()) {
                    return;
                }

                foreach (Collider2D col in Object.FindObjectsOfType<Collider2D>()) {
                    UpdateHitbox(col);
                }
            };
        }

        public static void OnUpdate(GameManager gameManager, StringBuilder infoBuilder) {
            if (gameManager.IsNonGameplayScene()
                || gameManager.gameState != GameState.PLAYING
                || gameManager.hero_ctrl?.transitionState == HeroTransitionState.WAITING_TO_ENTER_LEVEL
            ) {
                return;
            }

            string hitboxInfo = GetAllInfo();
            if (!string.IsNullOrEmpty(hitboxInfo)) {
                infoBuilder.AppendLine(hitboxInfo);
            }
        }

        public static void UpdateHitbox(GameObject gameObject) {
            foreach (Collider2D collider2D in gameObject.GetComponents<Collider2D>()) {
                UpdateHitbox(collider2D);
            }
        }

        private static void UpdateHitbox(Collider2D col) {
            if (col == null || Colliders.ContainsKey(col)) {
                return;
            }

            if (col is BoxCollider2D or PolygonCollider2D or EdgeCollider2D or CircleCollider2D) {
                GameObject gameObject = col.gameObject;
                if (IsDamageHero(col)) {
                    Colliders.Add(col, new HitboxData(col, HitboxColor.Enemy));
                } else if (gameObject.LocateMyFSM("health_manager_enemy") || gameObject.LocateMyFSM("health_manager")) {
                    Colliders.Add(col, new HitboxData(col, HitboxColor.PeaceMonster));
                } else if (gameObject.layer == (int) PhysLayers.TERRAIN) {
                    Colliders.Add(col, new HitboxData(col, HitboxColor.Terrain));
                } else if (gameObject == HeroController.instance.gameObject && !col.isTrigger) {
                    Colliders.Add(col, new HitboxData(col, HitboxColor.Knight));
                } else if (col.GetComponent<TransitionPoint>()
                           || col.isTrigger && col.GetComponent<HazardRespawnTrigger>()
                ) {
                    Colliders.Add(col, new HitboxData(col, HitboxColor.Trigger));
                }
            }
        }

        private static bool IsDamageHero(Collider2D col) {
#if V1028
            return col.gameObject.LocateMyFSM("damages_hero");
#else
            return col.GetComponent<DamageHero>() || col.gameObject.LocateMyFSM("damages_hero");
#endif
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