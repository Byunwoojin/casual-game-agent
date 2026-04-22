using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzlePals.Puzzle.Jigsaw
{
    public enum EdgeKind : byte { Flat, Convex, Concave }

    public struct CellEdges
    {
        public EdgeKind Top, Right, Bottom, Left;
    }

    /// <summary>
    /// 직소 그리드 — 주어진 이미지 종횡비·조각수·seed 로부터:
    ///  1) 최적 (cols × rows) 그리드 계산 (cols·rows = pieceCount 정확 일치, 종횡비 근사)
    ///  2) 내부 간선마다 Convex/Concave 랜덤 배정 (seed 고정)
    ///  3) 이웃 조각과 짝 맞춤 보장 (A.Convex ↔ B.Concave)
    ///  4) 이미지 경계 간선은 모두 Flat
    ///
    /// 순수 데이터 — MonoBehaviour 없음, EditMode 테스트 용이.
    /// </summary>
    public class JigsawGrid
    {
        public int Cols { get; }
        public int Rows { get; }

        // 내부 간선 저장. (r,c) 의 Top == _horizEdges[r][c] (r < Rows-1 일 때만 의미있음)
        // (r,c) 의 Right == _vertEdges[r][c] (c < Cols-1 일 때만 의미있음)
        private readonly EdgeKind[,] _horizEdges; // [Rows-1, Cols]
        private readonly EdgeKind[,] _vertEdges;  // [Rows,   Cols-1]

        private JigsawGrid(int cols, int rows, EdgeKind[,] horiz, EdgeKind[,] vert)
        {
            Cols = cols;
            Rows = rows;
            _horizEdges = horiz;
            _vertEdges  = vert;
        }

        /// <summary>pieceCount 를 그대로 (cols × rows) 로 분해 — 이미지 종횡비에 가장 근접.
        /// 셀이 정사각이 아닐 수 있음 (이미지 종횡비가 cols/rows 와 다르면).</summary>
        public static (int cols, int rows) DetermineGrid(float imageAspect, int pieceCount)
        {
            if (pieceCount < 1) throw new ArgumentException("pieceCount must be >= 1");
            if (imageAspect <= 0f) imageAspect = 1f;

            int bestCols = 1, bestRows = pieceCount;
            float bestDiff = float.MaxValue;
            for (int cols = 1; cols <= pieceCount; cols++)
            {
                if (pieceCount % cols != 0) continue;
                int rows = pieceCount / cols;
                float diff = Mathf.Abs((float)cols / rows - imageAspect);
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    bestCols = cols;
                    bestRows = rows;
                }
            }
            return (bestCols, bestRows);
        }

        /// <summary>
        /// 정사각 셀 보장용 그리드 + 크롭 선택. (cols × rows = pieceCount 불변,
        /// cols/rows 가 이미지 종횡비에 가장 근접한 팩터 쌍 선택 → 크롭 손실 최소)
        /// </summary>
        public static (int cols, int rows) DetermineSquareCellGrid(float imageAspect, int pieceCount)
        {
            // 정사각 셀 = cols/rows 가 image aspect 에 가까울수록 크롭 손실 적음
            // → DetermineGrid 와 목적지는 동일 (cols/rows ≈ imgAspect 최소화)
            return DetermineGrid(imageAspect, pieceCount);
        }

        /// <summary>
        /// (cols, rows) 그리드로 정사각 셀 쓰려면 필요한 이미지 센터 크롭 영역 (픽셀 좌표).
        /// 이미지가 target_aspect 보다 넓으면 좌우 크롭, 좁으면 상하 크롭. 항상 중앙 기준.
        /// </summary>
        public static Rect ComputeSquareCropRect(int imageWpx, int imageHpx, int cols, int rows)
        {
            float imgAspect    = (float)imageWpx / imageHpx;
            float targetAspect = (float)cols / rows;

            if (imgAspect > targetAspect)
            {
                // 이미지가 target 보다 와이드 → 좌우 크롭
                int finalW = Mathf.RoundToInt(imageHpx * targetAspect);
                int x      = (imageWpx - finalW) / 2;
                return new Rect(x, 0, finalW, imageHpx);
            }
            else
            {
                // 이미지가 target 보다 톨 → 상하 크롭
                int finalH = Mathf.RoundToInt(imageWpx / targetAspect);
                int y      = (imageHpx - finalH) / 2;
                return new Rect(0, y, imageWpx, finalH);
            }
        }

        public static JigsawGrid Generate(float imageAspect, int pieceCount, int seed)
        {
            var (cols, rows) = DetermineGrid(imageAspect, pieceCount);
            var rng = new System.Random(seed);

            // 내부 수평 간선: (Rows-1) × Cols (각 행 위에 있는 간선)
            var horiz = new EdgeKind[Mathf.Max(0, rows - 1), cols];
            for (int r = 0; r < rows - 1; r++)
                for (int c = 0; c < cols; c++)
                    horiz[r, c] = rng.Next(2) == 0 ? EdgeKind.Convex : EdgeKind.Concave;

            // 내부 수직 간선: Rows × (Cols-1) (각 열 오른쪽 간선)
            var vert = new EdgeKind[rows, Mathf.Max(0, cols - 1)];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols - 1; c++)
                    vert[r, c] = rng.Next(2) == 0 ? EdgeKind.Convex : EdgeKind.Concave;

            return new JigsawGrid(cols, rows, horiz, vert);
        }

        /// <summary>(row, col) 셀의 4변 EdgeKind. 이웃과 짝맞춤 자동 처리.</summary>
        public CellEdges GetEdges(int row, int col)
        {
            if (row < 0 || row >= Rows || col < 0 || col >= Cols)
                throw new ArgumentOutOfRangeException();

            var e = new CellEdges();

            // Top: 이 셀의 위쪽 간선 = horiz[row, col]. 셀이 "아래쪽"에서 본 방향.
            // 짝 맞춤 규약: 아래 셀 기준 Convex 면 위 셀의 Bottom은 Concave.
            // 여기서는 저장된 방향을 아래 셀의 Top 방향으로 해석.
            e.Top = (row == Rows - 1)
                ? EdgeKind.Flat
                : _horizEdges[row, col];

            // Bottom: 아래 셀의 Top 을 뒤집은 것
            e.Bottom = (row == 0)
                ? EdgeKind.Flat
                : Invert(_horizEdges[row - 1, col]);

            // Right: 저장된 방향 = 왼쪽 셀의 Right
            e.Right = (col == Cols - 1)
                ? EdgeKind.Flat
                : _vertEdges[row, col];

            // Left: 왼쪽 셀의 Right 를 뒤집은 것
            e.Left = (col == 0)
                ? EdgeKind.Flat
                : Invert(_vertEdges[row, col - 1]);

            return e;
        }

        private static EdgeKind Invert(EdgeKind k)
        {
            switch (k)
            {
                case EdgeKind.Convex:  return EdgeKind.Concave;
                case EdgeKind.Concave: return EdgeKind.Convex;
                default:               return EdgeKind.Flat;
            }
        }

        /// <summary>디버그: 모든 셀의 edge 검증 — 인접 셀 간 짝 맞춤 OK? (테스트용)</summary>
        public bool VerifyNeighborPairing()
        {
            for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
            {
                var here = GetEdges(r, c);

                if (r < Rows - 1)
                {
                    var above = GetEdges(r + 1, c);
                    if (Invert(here.Top) != above.Bottom) return false;
                }
                if (c < Cols - 1)
                {
                    var right = GetEdges(r, c + 1);
                    if (Invert(here.Right) != right.Left) return false;
                }
            }
            return true;
        }
    }
}
