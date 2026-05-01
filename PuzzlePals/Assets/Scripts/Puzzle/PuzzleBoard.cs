using System.Collections.Generic;
using UnityEngine;
using PuzzlePals.Data;
using PuzzlePals.Puzzle.Jigsaw;
using PuzzlePals.Systems;
using PuzzlePals.UI;

namespace PuzzlePals.Puzzle
{
    /// <summary>
    /// 직소 보드 오케스트레이터 — LevelDataSO 한 장 받아서:
    ///   1) (cols × rows) 결정 + 센터 크롭으로 정사각 셀
    ///   2) **카메라 viewport 에 맞춰 보드 크기 자동 스케일** — 보드·뱅크·툴바 공간이 모두 화면 안에
    ///   3) 각 셀 outline → Mesh → PuzzlePiece GameObject
    ///   4) 각 셀 snap 위치 → SnapZone GameObject
    ///   5) 회색 보드 배경 패널
    ///   6) **하단 흰색 PieceBank** 생성 + 전 조각 뱅크로 초기 배치 (shuffle)
    ///
    /// 좌표 규약: 보드 중심 = (0, boardCenterY) in world. 뱅크는 그 아래.
    /// UV 매핑: 전체 원본 텍스처 기준 (크롭 영역 UV 범위만 사용).
    /// </summary>
    public class PuzzleBoard : MonoBehaviour
    {
        [Header("Containers (비어있으면 self transform)")]
        [SerializeField] private Transform _piecesContainer;
        [SerializeField] private Transform _snapZonesContainer;

        [Header("보드 배경 — 기본값은 DesignTokens.BoardSurface 로 자동 적용")]
        [SerializeField] private Color _boardBackgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        [Header("Viewport 여유공간 (world units)")]
        [SerializeField] private float _reservedTopH    = 2.4f;   // 상단 툴바 공간 (220px / scale)
        [SerializeField] private float _reservedBottomH = 2.5f;   // 하단 뱅크 + 보드~뱅크 간격 (cellH × 1.85 + verticalMargin 수용)
        [SerializeField] private float _sideMargin      = 0.3f;
        [Tooltip("배너 광고 영역 (320×50 dp). 카메라 viewport 하단에서 이만큼 비워둠. 디바이스 빌드 후 미세 조정.")]
        [SerializeField] private float _bannerReserveH  = 1.0f;

        [Header("조각 뱅크")]
        [SerializeField] private float _bankHeightRelative = 0.9f; // _reservedBottomH 에 대한 비율

        public IReadOnlyList<PuzzlePiece> Pieces    => _pieces;
        public IReadOnlyList<SnapZone>    SnapZones => _snapZones;

        public JigsawGrid Grid           { get; private set; }
        public Rect       CropRectPx     { get; private set; }
        public Vector2    BoardSizeWorld { get; private set; }
        public float      BoardCenterY   { get; private set; }
        public PieceBank  Bank           { get; private set; }
        public int        Rows => Grid?.Rows ?? 0;
        public int        Cols => Grid?.Cols ?? 0;

        // 드래그 가능 영역 (헤더 ~ 배너 사이) — PuzzlePiece OnPointerUp 시 영역 밖이면 clamp/복귀
        public float DraggableTopY    { get; private set; }
        public float DraggableBottomY { get; private set; }

        // Phase 2 인접 짝맞춤용 — 셀 크기 + (row,col) → piece 인덱스
        public float CellW { get; private set; }
        public float CellH { get; private set; }

        /// <summary>인접 짝맞춤 tolerance — 셀 폭의 30%. 가까이 놓으면 짝 합체.</summary>
        [SerializeField] private float _neighborSnapToleranceFactor = 0.3f;

        private readonly List<PuzzlePiece>  _pieces    = new List<PuzzlePiece>();
        private readonly List<SnapZone>     _snapZones = new List<SnapZone>();
        private readonly Stack<PuzzlePiece> _undoStack = new Stack<PuzzlePiece>(); // Undo 용
        private readonly Dictionary<(int row, int col), PuzzlePiece> _pieceByCoord
            = new Dictionary<(int, int), PuzzlePiece>();
        private GameObject _boardBackground;
        private GameObject _bankGO;

        private void OnEnable()  => PuzzlePiece.OnAnyPieceSnapped += HandlePieceSnapped;
        private void OnDisable() => PuzzlePiece.OnAnyPieceSnapped -= HandlePieceSnapped;

        private void HandlePieceSnapped(PuzzlePiece p)
        {
            if (p != null && _pieces.Contains(p)) _undoStack.Push(p);
        }

        /// <summary>최근 스냅된 조각 1개를 unsnap 해서 뱅크에 복귀.</summary>
        public bool Undo()
        {
            while (_undoStack.Count > 0)
            {
                var p = _undoStack.Pop();
                if (p == null || !p.IsSnapped) continue; // stale 항목 스킵

                p.Unsnap();
                if (Bank != null) Bank.InsertAt(p, p.transform.position.x);
                return true;
            }
            return false;
        }

        public void Build(LevelDataSO level)
        {
            Clear();

            if (level == null || level.characterSprite == null)
            {
                Debug.LogError($"[PuzzleBoard] Level 데이터 또는 sprite 누락");
                return;
            }

            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[PuzzleBoard] Camera.main 없음 — 뷰포트 계산 불가");
                return;
            }
            // 디자인 시스템 크림 톤으로 카메라 배경 통일 (성인 피벗 v0.4)
            cam.backgroundColor = DesignTokens.BgPage;

            Sprite sprite   = level.characterSprite;
            Rect spriteRect = sprite.rect;
            int fullWpx     = Mathf.RoundToInt(spriteRect.width);
            int fullHpx     = Mathf.RoundToInt(spriteRect.height);
            float fullAspect = (float)fullWpx / fullHpx;

            // 1) 그리드 (No-crop 모드) — DetermineGrid 로 이미지 종횡비에 가장 가까운 팩터 쌍.
            //    셀은 정사각이 아닐 수 있음 (이미지 보존 우선).
            var (cols, rows) = JigsawGrid.DetermineGrid(fullAspect, level.pieceCount);
            CropRectPx       = new Rect(0, 0, fullWpx, fullHpx); // 전체 이미지 사용
            Grid             = JigsawGrid.Generate(fullAspect, level.pieceCount, level.levelNumber);

            // 2) 카메라 viewport 기준 "사용 가능" 영역
            float camHalfH  = cam.orthographicSize;
            float camHalfW  = camHalfH * cam.aspect;
            float viewW     = 2f * camHalfW;
            float viewH     = 2f * camHalfH;

            // 배너 영역만큼 가용 세로 줄임 — PieceBank 가 자동으로 그만큼 위로 올라감 (배너에 안 가림)
            float availableH = Mathf.Max(1f, viewH - _reservedTopH - _reservedBottomH - _bannerReserveH);
            float availableW = Mathf.Max(1f, viewW - _sideMargin * 2f);

            // 3) 보드 크기 (종횡비 유지 + 영역 안 fit). no-crop 이므로 이미지 종횡비 그대로.
            float boardAspect = fullAspect;
            float boardW, boardH;
            if (availableW / availableH > boardAspect)
            {
                boardH = availableH;
                boardW = boardH * boardAspect;
            }
            else
            {
                boardW = availableW;
                boardH = boardW / boardAspect;
            }
            BoardSizeWorld = new Vector2(boardW, boardH);

            // 4) 보드 중심 Y — 사용 가능 영역 중앙. 하단 배너 영역도 reservedBottom 처럼 빼서 PieceBank/보드 모두 위로 올림.
            float availableTopY    =  camHalfH - _reservedTopH;
            float availableBottomY = -camHalfH + _reservedBottomH + _bannerReserveH;
            BoardCenterY           = (availableTopY + availableBottomY) * 0.5f;

            // 드래그 가능 영역 노출 — PuzzlePiece 가 영역 밖 drop 시 OriginalPosition 복귀에 사용
            DraggableTopY    = availableTopY;
            DraggableBottomY = availableBottomY;

            float boardBottomY = BoardCenterY - boardH * 0.5f;

            // 5) 셀 크기 (world units, 정사각)
            float cellW = boardW / Grid.Cols;
            float cellH = boardH / Grid.Rows;
            CellW = cellW;
            CellH = cellH;

            // 6) UV 계산 — pxToWorld scale: 크롭 픽셀 → world 유닛
            float pxToWorld = boardW / CropRectPx.width;
            Vector2 fullTexSizeWorld     = new Vector2(fullWpx * pxToWorld, fullHpx * pxToWorld);
            Vector2 cropOriginInFullWorld = new Vector2(
                (spriteRect.x + CropRectPx.x) * pxToWorld,
                (spriteRect.y + CropRectPx.y) * pxToWorld);

            // 7) 공유 material — Sprites/Default (URP 2D 호환).
            var mat = new Material(Shader.Find("Sprites/Default"))
            {
                name        = $"PuzzlePiece_Level{level.levelNumber}",
                mainTexture = sprite.texture,
            };

            Transform piecesParent = _piecesContainer != null ? _piecesContainer : transform;
            Transform zonesParent  = _snapZonesContainer != null ? _snapZonesContainer : transform;

            // 8) 보드 배경 (회색 직사각형, 보드 중심에 배치)
            SpawnBoardBackground(boardW, boardH, BoardCenterY);

            // 9) 셀 루프
            for (int r = 0; r < Grid.Rows; r++)
            for (int c = 0; c < Grid.Cols; c++)
            {
                var edges = Grid.GetEdges(r, c);

                var outline = JigsawPieceShape.BuildOutline(cellW, cellH, edges);

                Vector2 pivotLocal = new Vector2(cellW * 0.5f, cellH * 0.5f);
                for (int i = 0; i < outline.Count; i++) outline[i] -= pivotLocal;

                Vector2 cellCenterInBoard = new Vector2(
                    c * cellW + cellW * 0.5f,
                    r * cellH + cellH * 0.5f);

                Vector3 cellCenterWorld = new Vector3(
                    cellCenterInBoard.x - boardW * 0.5f,
                    cellCenterInBoard.y - boardH * 0.5f + BoardCenterY,
                    0f);

                Vector2 pivotInFullTex = cropOriginInFullWorld + cellCenterInBoard;

                var mesh = JigsawMeshBuilder.Build(outline, pivotInFullTex, fullTexSizeWorld);

                // SnapZone
                var zoneGO = new GameObject($"SnapZone_{r:D2}_{c:D2}");
                zoneGO.transform.SetParent(zonesParent, worldPositionStays: false);
                zoneGO.transform.localPosition = cellCenterWorld;
                var zone = zoneGO.AddComponent<SnapZone>();
                _snapZones.Add(zone);

                // PuzzlePiece
                var pieceGO = new GameObject($"Piece_{r:D2}_{c:D2}");
                pieceGO.transform.SetParent(piecesParent, worldPositionStays: false);
                var piece = pieceGO.AddComponent<PuzzlePiece>();
                piece.Initialize(mesh, mat, outline, zone);
                // 스냅 threshold 를 셀 크기에 비례하게 — 셀 대비 35%
                piece.SnapThreshold = cellW * 0.35f;
                piece.Row   = r;
                piece.Col   = c;
                piece.Board = this;

                _pieces.Add(piece);
                _pieceByCoord[(r, c)] = piece;
            }

            // 10) PieceBank 생성 + 조각 shuffle 해서 전부 뱅크에 넣음
            // 직소 탭(요철)이 셀 ±25% 돌출 → cellH × 1.85 (보호 1.5 + 상하 마진)
            float bankHeight = cellH * 1.85f;
            _bankGO = new GameObject("PieceBank");
            _bankGO.transform.SetParent(transform, worldPositionStays: false);
            Bank = _bankGO.AddComponent<PieceBank>();
            Bank.Setup(cam, boardBottomY, cellW, bankHeight);

            var shuffled = new List<PuzzlePiece>(_pieces);
            var rng = new System.Random(level.levelNumber * 31 + 7);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }
            foreach (var p in shuffled) Bank.Add(p);

            Debug.Log($"[PuzzleBoard] Level {level.levelNumber} 빌드 완료 — " +
                      $"그리드 {Grid.Cols}×{Grid.Rows}, " +
                      $"원본 {fullWpx}×{fullHpx}px → 크롭 {CropRectPx.width:F0}×{CropRectPx.height:F0}px " +
                      $"(손실 W:{(1f-CropRectPx.width/fullWpx)*100f:F1}% / H:{(1f-CropRectPx.height/fullHpx)*100f:F1}%), " +
                      $"보드 {boardW:F2}×{boardH:F2} world, " +
                      $"셀 {cellW:F2} world, 뷰포트 {viewW:F2}×{viewH:F2}");
        }

        private void SpawnBoardBackground(float widthWorld, float heightWorld, float centerY)
        {
            _boardBackground = new GameObject("BoardBackground");
            _boardBackground.transform.SetParent(transform, worldPositionStays: false);
            _boardBackground.transform.localPosition = new Vector3(0f, centerY, 0f);
            _boardBackground.transform.localScale    = new Vector3(widthWorld, heightWorld, 1f);

            var sr = _boardBackground.AddComponent<SpriteRenderer>();
            var tex = Texture2D.whiteTexture;
            sr.sprite       = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                            new Vector2(0.5f, 0.5f), tex.width);
            // SerializeField 값 우선, 미설정(투명) 이면 DesignTokens.BoardSurface 폴백
            sr.color        = _boardBackgroundColor.a > 0f ? _boardBackgroundColor : DesignTokens.BoardSurface;
            sr.sortingOrder = -10;
        }

        public void Clear()
        {
            foreach (var p in _pieces) if (p != null) DestroyImmediate(p.gameObject);
            foreach (var z in _snapZones) if (z != null) DestroyImmediate(z.gameObject);
            _pieces.Clear();
            _snapZones.Clear();
            _undoStack.Clear();
            _pieceByCoord.Clear();
            Grid = null;

            if (_boardBackground != null)
            {
                if (Application.isPlaying) Destroy(_boardBackground);
                else                       DestroyImmediate(_boardBackground);
                _boardBackground = null;
            }
            if (_bankGO != null)
            {
                if (Application.isPlaying) Destroy(_bankGO);
                else                       DestroyImmediate(_bankGO);
                _bankGO = null;
                Bank = null;
            }

            // 씬 전체에서 stale PuzzlePiece / SnapZone 정리 (피벗 전 시스템 잔해 / 수동 배치 잔해 포함)
            DestroyAllInScene<PuzzlePiece>();
            DestroyAllInScene<SnapZone>();
        }

        private static void DestroyAllInScene<T>() where T : Component
        {
            var found = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var comp in found)
            {
                if (comp == null || comp.gameObject == null) continue;
                if (Application.isPlaying) Destroy(comp.gameObject);
                else                       DestroyImmediate(comp.gameObject);
            }
        }

        public bool IsComplete()
        {
            if (_pieces.Count == 0) return false;
            foreach (var p in _pieces)
                if (p == null || !p.IsSnapped) return false;
            return true;
        }

        // ─────────────────────────────────────────────────────
        // 인접 짝맞춤 (Phase 2)
        // ─────────────────────────────────────────────────────

        /// <summary>(row, col) 위치의 piece 반환 (없으면 null).</summary>
        public PuzzlePiece GetPieceAt(int row, int col)
        {
            _pieceByCoord.TryGetValue((row, col), out var p);
            return p;
        }

        /// <summary>
        /// PuzzlePiece.OnPointerUp 에서 호출 — a 의 4-방향 인접 piece 와의 짝맞춤 검사 후 합체.
        /// 첫 매칭 1쌍만 처리 (한 번 합체 시 나머지는 다음 드롭에 처리 — 단순화).
        /// </summary>
        public void TryNeighborPair(PuzzlePiece a)
        {
            if (a == null || a.IsSnapped || a.IsInBank) return;
            if (CellW <= 0f) return; // Build 전 호출 방어

            float tolerance = CellW * _neighborSnapToleranceFactor;

            // 4-방향 (상하좌우, 대각선 제외)
            var candidates = new PuzzlePiece[]
            {
                GetPieceAt(a.Row - 1, a.Col),
                GetPieceAt(a.Row + 1, a.Col),
                GetPieceAt(a.Row, a.Col - 1),
                GetPieceAt(a.Row, a.Col + 1),
            };

            foreach (var b in candidates)
            {
                if (NeighborPairing.ShouldPair(a, b, CellW, CellH, tolerance))
                {
                    NeighborPairing.AlignAndMerge(a, b, CellW, CellH);
                    // Phase 3: 합체 시점 효과음 + 작은 파티클 (a 와 b 사이)
                    // 합체 ≠ 정답 안착 — PlayDing (정답용) 대신 PlayPair (합체 전용) 사용.
                    Vector3 burstPos = (a.transform.position + b.transform.position) * 0.5f;
                    VFXManager.Instance?.PlaySnapBurst(burstPos);
                    AudioManager.Instance?.PlayPair();
                    return;
                }
            }
        }
    }
}
