using System.Collections.Generic;
using UnityEngine;

namespace HollowKnightTasInfo.Utils {
    internal static class ScreenUtils {
        private const int ScreenEdge = 6;
        public static List<Vector2> GetIntersectionPoint(Vector2 start, Vector2 end) {
            int width = Screen.width - ScreenEdge;
            int height = Screen.height - ScreenEdge;
            Vector2[] borderPoints = {
                new(ScreenEdge, ScreenEdge),
                new(width, ScreenEdge),
                new(width, height),
                new(ScreenEdge, height),
            };

            List<Vector2> result = new();
            for (int i = 0; i < borderPoints.Length; i++) {
                if (FindIntersection(borderPoints[i], borderPoints[(i + 1) % borderPoints.Length], start, end) is { } vector2) {
                    result.Add(vector2);
                }
            }

            if (InsideOfScreen(start)) {
                result.Add(start);
            }

            if (InsideOfScreen(end)) {
                result.Add(end);
            }

            return result;
        }

        private static bool InsideOfScreen(Vector2 vector2) {
            return vector2.x >= ScreenEdge && vector2.x < Screen.width - ScreenEdge && vector2.y >= ScreenEdge && vector2.y < Screen.height - ScreenEdge;
        }

        private static Vector2? FindIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
            // Get the segments' parameters.
            float dx12 = p2.x - p1.x;
            float dy12 = p2.y - p1.y;
            float dx34 = p4.x - p3.x;
            float dy34 = p4.y - p3.y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((p1.x - p3.x) * dy34 + (p3.y - p1.y) * dx34)
                / denominator;
            if (float.IsInfinity(t1)) {
                // The lines are parallel (or close enough to it).
                return null;
            }

            float t2 = ((p3.x - p1.x) * dy12 + (p1.y - p3.y) * dx12) / -denominator;

            // Find the point of intersection.
            Vector2 intersection = new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            bool segmentsIntersect = t1 >= 0 && t1 <= 1 && t2 >= 0 && t2 <= 1;

            if (segmentsIntersect) {
                return intersection;
            }

            return null;
        }
    }
}