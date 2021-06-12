using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assembly_CSharp.TasInfo.mm.Source.Utils;
using GlobalEnums;
using UnityEngine;

namespace Assembly_CSharp.TasInfo.mm.Source {
    internal static class HitboxInfo {
        private static readonly Dictionary<Collider2D, HitboxData> Colliders = new();

        public static void OnInit() {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (_, _) => RefreshInfo();
        }

        public static void RefreshInfo() {
            Colliders.Clear();

            if (GameManager.instance.IsNonGameplayScene()) {
                return;
            }

            foreach (Collider2D col in Resources.FindObjectsOfTypeAll<Collider2D>()) {
                TryAddHitbox(col);
            }
        }

        public static void OnPreRender(GameManager gameManager, StringBuilder infoBuilder) {
            if (gameManager.IsNonGameplayScene() || !ConfigManager.ShowHitbox) {
                return;
            }

            string hitboxInfo = GetAllInfo();
            if (!string.IsNullOrEmpty(hitboxInfo)) {
                infoBuilder.AppendLine(hitboxInfo);
            }
        }

        public static void TryAddHitbox(GameObject gameObject) {
            foreach (Collider2D collider2D in gameObject.GetComponentsInChildren<Collider2D>(true)) {
                TryAddHitbox(collider2D);
            }
        }

        private static void TryAddHitbox(Collider2D col) {
            if (col == null || Colliders.ContainsKey(col)) {
                return;
            }

            if (col is BoxCollider2D or PolygonCollider2D or EdgeCollider2D or CircleCollider2D) {
                GameObject gameObject = col.gameObject;
                if (IsDamageHero(col)) {
                    Colliders.Add(col, new HitboxData(col, HitboxType.Enemy));
                } else if (gameObject.LocateMyFSM("health_manager_enemy") || gameObject.LocateMyFSM("health_manager")) {
                    Colliders.Add(col, new HitboxData(col, HitboxType.Harmless));
                } else if (gameObject.layer == (int) PhysLayers.TERRAIN) {
                    Colliders.Add(col, new HitboxData(col, HitboxType.Terrain));
                } else if (gameObject == HeroController.instance.gameObject && !col.isTrigger) {
                    Colliders.Add(col, new HitboxData(col, HitboxType.Knight));
                } else if (gameObject.LocateMyFSM("damages_enemy") || gameObject.name == "Damager" && gameObject.LocateMyFSM("Damage")) {
                    Colliders.Add(col, new HitboxData(col, HitboxType.Attack));
                } else if (col.CompareTag("Geo") || !col.isTrigger && col.GetComponent<GeoRock>() || gameObject.LocateMyFSM("Chest Control")) {
                    Colliders.Add(col, new HitboxData(col, HitboxType.Harmless));
                } else if (col.isTrigger && (
                        col.GetComponent<TransitionPoint>()
                        || col.GetComponent<HazardRespawnTrigger>()
                        || gameObject.name.StartsWith("Alert Range")
                    )
                ) {
                    Colliders.Add(col, new HitboxData(col, HitboxType.Trigger));
                } else {
                    Colliders.Add(col, new HitboxData(col, HitboxType.Other));
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

                result.Append(result.Length > 0 ? $"|{hitboxInfo}" : $"{hitboxData.Key}={hitboxInfo}");
            }

            return StringUtils.Join("\n", results.Values);
        }

        internal enum HitboxType {
            Knight,
            Attack,
            Enemy,
            Harmless,
            Trigger,
            Terrain,
            Other
        }

        private class HitboxData {
            private readonly Collider2D collider;
            private readonly HitboxType hitboxType;

            public HitboxData(Collider2D collider, HitboxType hitboxType) {
                this.collider = collider;
                this.hitboxType = hitboxType;
            }

            private string ColorValue => ConfigManager.GetHitboxColorValue(hitboxType);

            public string Key =>
                collider switch {
                    BoxCollider2D or EdgeCollider2D or PolygonCollider2D => "LineHitbox",
                    CircleCollider2D => "CircleHitbox",
                    _ => "LineHitbox"
                };

            public override string ToString() {
                Camera camera = Camera.main;
                if (camera == null || collider == null || !collider.isActiveAndEnabled || string.IsNullOrEmpty(ColorValue)) {
                    return string.Empty;
                }

                if (!ConfigManager.ShowOtherHitbox && hitboxType == HitboxType.Other) {
                    return string.Empty;
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
                Vector2 halfSize = box.size / 2f;
                Vector2 topLeft = new(-halfSize.x, halfSize.y);
                Vector2 topRight = halfSize;
                Vector2 bottomLeft = -halfSize;
                Vector2 bottomRight = new(halfSize.x, -halfSize.y);
                return ToLineInfo(camera, box, new List<Vector2> {
                    topLeft, topRight, bottomRight, bottomLeft, topLeft
                });
            }

            private string ToEdgeInfo(EdgeCollider2D edge, Camera camera) {
                if (edge.points.Length == 0) {
                    return string.Empty;
                }

                return ToLineInfo(camera, edge, edge.points.ToList());
            }

            private string ToPolyInfo(PolygonCollider2D poly, Camera camera) {
                if (poly.points.Length == 0) {
                    return string.Empty;
                }

                List<Vector2> points = new(poly.points);
                points.Add(points[0]);
                return ToLineInfo(camera, poly, points);
            }

            private string ToLineInfo(Camera camera, Collider2D collider2D, List<Vector2> points) {
                Transform transform = collider2D.transform;
                Vector2 offset = collider2D.offset;

                List<string> result = new();
                for (int i = 0; i < points.Count - 1; i++) {
                    Vector2 point1 = ScreenUtils.WorldToScreenPoint(camera, transform, points[i] + offset);
                    Vector2 point2 = ScreenUtils.WorldToScreenPoint(camera, transform, points[i + 1] + offset);

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

                    result.Add($"{x1}|{y1}|{x2}|{y2}|{ColorValue}");
                }

                return StringUtils.Join("|", result);
            }

            private string ToCircleInfo(CircleCollider2D circle, Camera camera) {
                Vector2 offset = circle.offset;
                Vector2 center = ScreenUtils.WorldToScreenPoint(camera, circle.transform, offset);
                Vector2 centerRight = ScreenUtils.WorldToScreenPoint(camera, circle.transform, new Vector2(offset.x + circle.radius, offset.y));
                int radius = (int) Math.Round(Vector2.Distance(center, centerRight));
                int x = (int) Math.Round(center.x);
                int y = Screen.height - (int) Math.Round(center.y);

                if (x + radius < 0 || x - radius > Screen.width || y + radius < 0 || y - radius > Screen.height || radius == 0 ||
                    radius > Screen.width) {
                    return string.Empty;
                }

                return $"{x}|{y}|{radius}|{ColorValue}";
            }
        }
    }
}