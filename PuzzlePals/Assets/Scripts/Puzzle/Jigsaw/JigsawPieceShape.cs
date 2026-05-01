using System.Collections.Generic;
using UnityEngine;

namespace PuzzlePals.Puzzle.Jigsaw
{
    /// <summary>
    /// 셀 크기와 4변 EdgeKind 로 한 조각의 outline polygon 생성.
    ///
    /// 조각 local 좌표 규약: (0, 0) = 셀 좌하단. 셀 크기 = (cellW, cellH).
    /// 탭(convex) 은 셀 바깥으로, 홈(concave) 은 셀 안쪽으로 돌출.
    ///
    /// 탭 모양: 고정 unit tab polyline (12점) — 목에서 부푼 구슬 형태.
    /// 이 polyline 이 각 변 길이에 맞게 스케일·회전·평행이동 되어 배치됨.
    ///
    /// Outline 순서: 좌하 → 우하 → 우상 → 좌상 → 좌하 (CCW).
    /// </summary>
    public static class JigsawPieceShape
    {
        // 탭 크기 (변 길이 대비). 0.25 = 변의 25% 만큼 바깥/안쪽으로 돌출.
        public const float TabSizeRatio = 0.25f;

        // hairline 방지 outline 확장 — uniform scale 은 양각/음각 polyline 짝맞춤 mismatch 발생.
        // 정지 상태 hairline 은 mesh.colors=white 명시로 해결됨.
        // drag 중 leader 양각 가장자리 sub-pixel rasterization 톱니는 GPU 의 inherent — 모바일 기기 해상도에서 거의 안 보임.
        public const float OutlineExpansion = 0f;

        // 유닛 탭 (convex, edge: (0,0) → (1,0), 탭 +y 방향). x∈[0,1], y∈[0, TabSizeRatio*대략] 범위.
        // 중앙(0.35~0.65) 목 + 상단 구슬 형태. 12점.
        // y 값은 최종적으로 TabSizeRatio 로 스케일됨 (아래 BuildSide 참고).
        private static readonly Vector2[] UnitTabConvex = new Vector2[]
        {
            new Vector2(0.00f,  0.00f), // start
            new Vector2(0.35f,  0.00f), // 목 시작 (왼쪽)
            new Vector2(0.37f,  0.24f), // 목 왼쪽 → 구슬 바깥 오버행
            new Vector2(0.30f,  0.48f), // 구슬 왼쪽 하단
            new Vector2(0.30f,  0.72f), // 구슬 왼쪽 상단
            new Vector2(0.40f,  1.00f), // 구슬 꼭대기 왼쪽
            new Vector2(0.60f,  1.00f), // 구슬 꼭대기 오른쪽
            new Vector2(0.70f,  0.72f), // 구슬 오른쪽 상단
            new Vector2(0.70f,  0.48f), // 구슬 오른쪽 하단
            new Vector2(0.63f,  0.24f), // 목 오른쪽 오버행
            new Vector2(0.65f,  0.00f), // 목 끝 (오른쪽)
            new Vector2(1.00f,  0.00f), // end
        };

        /// <param name="cellW">셀 너비</param>
        /// <param name="cellH">셀 높이</param>
        /// <param name="edges">4변 EdgeKind</param>
        /// <returns>local 좌표 outline (CCW, 폐곡선 — 마지막 점 != 첫 점)</returns>
        public static List<Vector2> BuildOutline(float cellW, float cellH, CellEdges edges)
        {
            var outline = new List<Vector2>(60);

            // 변마다: (start, end, 안쪽방향) 정의 후 BuildSide 호출
            // 시작점만 넣고 end 는 다음 변이 그릴 것이므로 start+중간점만 append (마지막 점은 제외하여 중복 방지).
            // 순회: 하단(L→R) → 우(B→T) → 상(R→L) → 좌(T→B)

            // 하단 변: (0,0) → (cellW, 0), 탭 바깥 방향 = -y (셀 아래)
            AppendSide(outline, new Vector2(0, 0), new Vector2(cellW, 0), new Vector2(0, -1), edges.Bottom, cellW);

            // 우측 변: (cellW, 0) → (cellW, cellH), 탭 바깥 방향 = +x
            AppendSide(outline, new Vector2(cellW, 0), new Vector2(cellW, cellH), new Vector2(1, 0), edges.Right, cellH);

            // 상단 변: (cellW, cellH) → (0, cellH), 탭 바깥 방향 = +y
            AppendSide(outline, new Vector2(cellW, cellH), new Vector2(0, cellH), new Vector2(0, 1), edges.Top, cellW);

            // 좌측 변: (0, cellH) → (0, 0), 탭 바깥 방향 = -x
            AppendSide(outline, new Vector2(0, cellH), new Vector2(0, 0), new Vector2(-1, 0), edges.Left, cellH);

            // hairline 방지 — 셀 중심 기준 균일 scale 적용. 양각/음각 모두 같은 비율로 확장되어 짝맞춤 보존.
            if (OutlineExpansion > 0f)
            {
                Vector2 center = new Vector2(cellW * 0.5f, cellH * 0.5f);
                float   scale  = 1f + OutlineExpansion;
                for (int i = 0; i < outline.Count; i++)
                    outline[i] = center + (outline[i] - center) * scale;
            }

            return outline;
        }

        // Flat: 시작점만 추가 (끝점은 다음 변이 시작점으로 추가).
        // Convex: unit tab 을 outwardDir 방향으로 scale·회전 후 추가.
        // Concave: unit tab 을 -outwardDir 방향 (=셀 안쪽) 으로 추가.
        private static void AppendSide(List<Vector2> outline, Vector2 start, Vector2 end,
                                       Vector2 outwardDir, EdgeKind kind, float edgeLen)
        {
            if (kind == EdgeKind.Flat)
            {
                outline.Add(start);
                return;
            }

            // edge 방향 = start → end 단위벡터
            Vector2 edgeDir = (end - start).normalized;
            // tab y 방향 결정: Convex → outward, Concave → inward (= -outward)
            Vector2 tabDir = (kind == EdgeKind.Convex) ? outwardDir : -outwardDir;
            // tab 높이 = 변 길이 × TabSizeRatio
            float tabHeight = edgeLen * TabSizeRatio;

            // unit (x, y) → world 변환. 마지막 점 (1.00, 0) 은 다음 변이 추가하므로 제외.
            // hairline 처리는 BuildOutline 끝에서 piece 중심 기준 균일 scale 로 일괄 적용.
            for (int i = 0; i < UnitTabConvex.Length - 1; i++)
            {
                Vector2 u = UnitTabConvex[i];
                Vector2 p = start + edgeDir * (u.x * edgeLen) + tabDir * (u.y * tabHeight);
                outline.Add(p);
            }
        }
    }
}
