using System.Collections.Generic;
using UnityEngine;

namespace PuzzlePals.Puzzle.Jigsaw
{
    /// <summary>
    /// 단순 다각형(simple polygon) 을 삼각형으로 분해. Ear-clipping 알고리즘.
    ///
    /// 조건: polygon 은 CCW 순서, 자기 교차 없음 (직소 조각 outline 은 모두 만족).
    /// 복잡도: O(n²). n ≤ 60 (조각 하나 최대 점 수) 이므로 실시간 가능.
    ///
    /// 직소 조각은 concave 변(홈)으로 인해 non-convex 다각형이 될 수 있어 fan-from-centroid 로는 부족.
    /// </summary>
    public static class EarClipTriangulator
    {
        /// <summary>
        /// 다각형 점 배열 → 삼각형 인덱스 배열 (3의 배수).
        /// 실패/이상 polygon 일 경우 가능한 한 많은 삼각형 반환 후 종료.
        /// </summary>
        public static int[] Triangulate(IReadOnlyList<Vector2> polygon)
        {
            int n = polygon.Count;
            if (n < 3) return new int[0];

            // 인덱스 링크드 리스트 (간단히 List<int> 로 대체)
            var indices = new List<int>(n);
            for (int i = 0; i < n; i++) indices.Add(i);

            // CCW 확인, CW 이면 뒤집음
            if (SignedArea(polygon) < 0f) indices.Reverse();

            var triangles = new List<int>(3 * (n - 2));
            int guard = 0;
            int guardMax = n * n + 1;

            while (indices.Count > 3)
            {
                if (++guard > guardMax) break;

                bool clipped = false;
                int count = indices.Count;
                for (int i = 0; i < count; i++)
                {
                    int prev = indices[(i - 1 + count) % count];
                    int curr = indices[i];
                    int next = indices[(i + 1) % count];

                    Vector2 a = polygon[prev];
                    Vector2 b = polygon[curr];
                    Vector2 c = polygon[next];

                    // b 가 convex vertex 인가 (CCW 기준 cross > 0)
                    if (Cross(a, b, c) <= 0f) continue;

                    // 어느 다른 polygon 점이 삼각형 abc 내부에 있는가?
                    bool hasInside = false;
                    for (int j = 0; j < count; j++)
                    {
                        if (j == (i - 1 + count) % count || j == i || j == (i + 1) % count) continue;
                        Vector2 p = polygon[indices[j]];
                        if (PointInTriangle(p, a, b, c))
                        {
                            hasInside = true;
                            break;
                        }
                    }
                    if (hasInside) continue;

                    // b 는 ear — 삼각형으로 잘라냄
                    triangles.Add(prev);
                    triangles.Add(curr);
                    triangles.Add(next);
                    indices.RemoveAt(i);
                    clipped = true;
                    break;
                }
                if (!clipped) break; // 안전장치 — ear 없음 (비정상 polygon)
            }

            if (indices.Count == 3)
            {
                triangles.Add(indices[0]);
                triangles.Add(indices[1]);
                triangles.Add(indices[2]);
            }

            return triangles.ToArray();
        }

        // (b-a) × (c-a) 의 z 성분
        private static float Cross(Vector2 a, Vector2 b, Vector2 c)
            => (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);

        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Cross(p, a, b);
            float d2 = Cross(p, b, c);
            float d3 = Cross(p, c, a);
            bool hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
            bool hasPos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(hasNeg && hasPos);
        }

        private static float SignedArea(IReadOnlyList<Vector2> poly)
        {
            float area = 0f;
            int n = poly.Count;
            for (int i = 0; i < n; i++)
            {
                Vector2 a = poly[i];
                Vector2 b = poly[(i + 1) % n];
                area += (b.x - a.x) * (b.y + a.y);
            }
            return -area * 0.5f; // CCW → +
        }
    }
}
