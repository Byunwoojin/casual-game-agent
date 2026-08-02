using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SlidingPuzzle.Data;
using SlidingPuzzle.Systems;

namespace SlidingPuzzle.Gameplay
{
    /// <summary>
    /// 슬라이딩 퍼즐(N-puzzle) 렌더링/입력 — 그리드 상태 로직은 SlidingGridState 에 위임.
    /// 원본 이미지를 N×N 정사각 타일로 잘라 빈 칸(blank) 하나를 남기고, 빈 칸과 인접한
    /// 타일을 탭하면 그 방향으로 슬라이드한다.
    ///
    /// 좌표 규약: 타일 조각(piece)의 "정답 슬롯" == pieceIndex (0..totalCells-2, 마지막 칸은 항상 blank).
    /// 텍스처는 y축이 아래가 0(bottom-left 원점)이라, 화면상 위쪽 행(row=0)이 텍스처에서는
    /// 가장 큰 y 구간에 대응 — BuildTiles 의 rb 계산에서 축을 뒤집는다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SlidingBoard : MonoBehaviour
    {
        [SerializeField] private RectTransform _previewRect;
        [SerializeField] private Image         _previewImage;
        [SerializeField] private RectTransform _gridAreaRect;
        [SerializeField] private RectTransform _tilesParent;

        public event System.Action OnCompleted;
        public event System.Action<int, int> OnProgressChanged;

        public LevelDataSO Level { get; private set; }
        public int   FoundCount => _state?.CountCorrect() ?? 0;
        public int   TotalCount => _state?.MovableCount ?? 0;
        public float Progress01 => _state == null || _state.MovableCount == 0 ? 0f : (float)FoundCount / _state.MovableCount;
        public bool  IsComplete => _state != null && _state.IsSolved();

        private SlidingGridState _state;
        private RectTransform[]  _tileRects;
        private float _cellSize;

        public void Build(LevelDataSO level)
        {
            EnsureUI();
            Level = level;
            _state = new SlidingGridState(level.gridSize);

            for (int i = _tilesParent.childCount - 1; i >= 0; i--)
                Destroy(_tilesParent.GetChild(i).gameObject);

            _previewImage.sprite         = level.sourceSprite;
            _previewImage.preserveAspect = true;

            LayoutBoardSquare();
            BuildTiles();
            _state.Shuffle(new System.Random());
            SnapAllTilesToCurrentSlots();
        }

        /// <summary>인접하지 않으면 무시(오답 사운드만). 인접하면 빈 칸과 스왑 + 진행률/완료 판정.</summary>
        internal void HandleTileClicked(int pieceIndex)
        {
            if (Level == null || IsComplete) return;

            int slot = _state.SlotOfPiece(pieceIndex);
            if (!_state.IsAdjacentToBlank(slot))
            {
                AudioManager.Instance?.PlayMiss();
                return;
            }

            _state.SwapBlankWith(slot);

            var rt     = _tileRects[pieceIndex];
            var newPos = SlotToLocalPos(_state.SlotOfPiece(pieceIndex));
            // DOTween UI 모듈(RectTransform.DOAnchorPos) 의존 회피 — DOTween.To 로 좌표만 직접 트윈
            DOTween.Kill(rt);
            DOTween.To(() => rt.anchoredPosition, v => rt.anchoredPosition = v, newPos, 0.15f)
                .SetEase(Ease.OutQuad);

            int correct = _state.CountCorrect();
            OnProgressChanged?.Invoke(correct, _state.MovableCount);
            AudioManager.Instance?.PlayFound();

            if (_state.IsSolved())
            {
                AudioManager.Instance?.PlayLevelComplete();
                OnCompleted?.Invoke();
            }
        }

        /// <summary>정위치가 아닌 타일 하나를 잠깐 금색으로 반짝임 — 자동 이동은 하지 않음(위치 안내만).</summary>
        public bool RequestHint()
        {
            if (_state == null) return false;

            for (int piece = 0; piece < _state.MovableCount; piece++)
            {
                if (_state.SlotOfPiece(piece) == piece) continue;

                var img      = _tileRects[piece].GetComponent<Image>();
                var original = img.color;
                // DOTween UI 모듈(Image.DOColor) 의존 회피 — DOTween.To 로 색상만 직접 트윈
                DOTween.Kill(img);
                DOTween.To(() => img.color, c => img.color = c, new Color(1f, 0.85f, 0.3f), 0.25f)
                    .SetLoops(6, LoopType.Yoyo)
                    .OnComplete(() => img.color = original);
                return true;
            }
            return false;
        }

        // ─────────────────────────────────────────────────────
        // PlayMode E2E 테스트 지원 — 셔플이 랜덤이라 미리 정해진 타일을 클릭할 수 없으므로,
        // 런타임에 "지금 빈 칸과 인접한 조각"을 질의해서 항상 유효한 탭 대상을 구할 수 있게 함.
        // ─────────────────────────────────────────────────────

        /// <summary>테스트 전용 — 현재 빈 칸과 인접한 조각의 pieceIndex (없으면 -1, 이론상 항상 존재).</summary>
        public int DebugGetAdjacentPieceIndex()
        {
            if (_state == null) return -1;
            for (int piece = 0; piece < _state.MovableCount; piece++)
            {
                int slot = _state.SlotOfPiece(piece);
                if (_state.IsAdjacentToBlank(slot)) return piece;
            }
            return -1;
        }

        /// <summary>테스트 전용 — pieceIndex 에 해당하는 타일 GameObject.</summary>
        public GameObject DebugGetTileGO(int pieceIndex)
            => (pieceIndex >= 0 && _tileRects != null && pieceIndex < _tileRects.Length) ? _tileRects[pieceIndex].gameObject : null;

        /// <summary>테스트 전용 — 인접 후보가 여러 개일 수 있어, 특정 조각이 "지금" 빈 칸과 인접한지 직접 질의.</summary>
        public bool DebugIsPieceAdjacentToBlank(int pieceIndex)
            => _state != null && _state.IsAdjacentToBlank(_state.SlotOfPiece(pieceIndex));

        // ─────────────────────────────────────────────────────
        // 레이아웃 / 타일 생성
        // ─────────────────────────────────────────────────────

        private Vector2 SlotToLocalPos(int slot)
        {
            int row = slot / _state.GridSize;
            int col = slot % _state.GridSize;
            float x = col * _cellSize + _cellSize * 0.5f;
            float y = -(row * _cellSize + _cellSize * 0.5f);
            return new Vector2(x, y);
        }

        private void SnapAllTilesToCurrentSlots()
        {
            for (int piece = 0; piece < _state.MovableCount; piece++)
                _tileRects[piece].anchoredPosition = SlotToLocalPos(_state.SlotOfPiece(piece));
        }

        private void LayoutBoardSquare()
        {
            const float pad = 24f;
            float availW = _gridAreaRect.rect.width;
            float availH = _gridAreaRect.rect.height;
            float side   = Mathf.Min(availW, availH) - pad * 2f;
            _cellSize = side / _state.GridSize;

            _tilesParent.anchorMin = _tilesParent.anchorMax = new Vector2(0.5f, 0.5f);
            _tilesParent.pivot     = new Vector2(0f, 1f);
            _tilesParent.sizeDelta = new Vector2(side, side);
            _tilesParent.anchoredPosition = new Vector2(-side * 0.5f, side * 0.5f);
        }

        private void BuildTiles()
        {
            int gridSize = _state.GridSize;
            _tileRects = new RectTransform[_state.MovableCount];
            var tex = Level.sourceSprite.texture;
            int texW = tex.width, texH = tex.height;

            // 나머지 없이 딱 떨어지지 않아도 이음매 없이 타일링되도록 경계를 누적 반올림으로 계산
            var colBoundary           = new int[gridSize + 1];
            var rowBoundaryFromBottom = new int[gridSize + 1];
            for (int i = 0; i <= gridSize; i++)
            {
                colBoundary[i]           = Mathf.RoundToInt(i * texW / (float)gridSize);
                rowBoundaryFromBottom[i] = Mathf.RoundToInt(i * texH / (float)gridSize);
            }

            const float tileGap = 4f;

            for (int piece = 0; piece < _state.MovableCount; piece++)
            {
                int homeRow = piece / gridSize; // 0 = 화면 위쪽 (정답 위치 기준)
                int homeCol = piece % gridSize;
                int rb = gridSize - 1 - homeRow; // 텍스처는 아래가 0이라 축 반전

                var srcRect = new Rect(
                    colBoundary[homeCol],
                    rowBoundaryFromBottom[rb],
                    colBoundary[homeCol + 1] - colBoundary[homeCol],
                    rowBoundaryFromBottom[rb + 1] - rowBoundaryFromBottom[rb]);

                var sprite = Sprite.Create(tex, srcRect, new Vector2(0.5f, 0.5f), 100f);

                var go = new GameObject($"Tile_{piece:D2}",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(_tilesParent, worldPositionStays: false);
                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot     = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(_cellSize - tileGap, _cellSize - tileGap);

                var img = go.GetComponent<Image>();
                img.sprite         = sprite;
                img.type           = Image.Type.Simple;
                img.preserveAspect = false;

                var click = go.AddComponent<TileClickTarget>();
                click.Board      = this;
                click.PieceIndex = piece;

                _tileRects[piece] = rt;
            }
        }

        private void EnsureUI()
        {
            if (_previewRect != null && _previewImage != null && _gridAreaRect != null && _tilesParent != null)
                return;

            const float previewH = 180f;

            var previewGo = new GameObject("Preview", typeof(RectTransform));
            previewGo.transform.SetParent(transform, worldPositionStays: false);
            _previewRect = (RectTransform)previewGo.transform;
            _previewRect.anchorMin = new Vector2(0f, 1f);
            _previewRect.anchorMax = new Vector2(1f, 1f);
            _previewRect.pivot     = new Vector2(0.5f, 1f);
            _previewRect.sizeDelta = new Vector2(0f, previewH);
            _previewRect.anchoredPosition = Vector2.zero;

            var thumbGo = new GameObject("Thumbnail",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            thumbGo.transform.SetParent(_previewRect, worldPositionStays: false);
            var thumbRt = (RectTransform)thumbGo.transform;
            thumbRt.anchorMin = thumbRt.anchorMax = new Vector2(0.5f, 0.5f);
            thumbRt.pivot     = new Vector2(0.5f, 0.5f);
            thumbRt.sizeDelta = new Vector2(previewH - 20f, previewH - 20f);
            _previewImage = thumbGo.GetComponent<Image>();
            _previewImage.raycastTarget = false;

            var gridAreaGo = new GameObject("GridArea", typeof(RectTransform));
            gridAreaGo.transform.SetParent(transform, worldPositionStays: false);
            _gridAreaRect = (RectTransform)gridAreaGo.transform;
            _gridAreaRect.anchorMin = Vector2.zero;
            _gridAreaRect.anchorMax = Vector2.one;
            _gridAreaRect.offsetMin = Vector2.zero;
            _gridAreaRect.offsetMax = new Vector2(0f, -previewH);

            var tilesGo = new GameObject("TilesParent", typeof(RectTransform));
            tilesGo.transform.SetParent(_gridAreaRect, worldPositionStays: false);
            _tilesParent = (RectTransform)tilesGo.transform;
        }

        /// <summary>각 타일에 부착 — 탭을 SlidingBoard.HandleTileClicked 로 전달.</summary>
        private class TileClickTarget : MonoBehaviour, IPointerClickHandler
        {
            public SlidingBoard Board;
            public int PieceIndex;
            public void OnPointerClick(PointerEventData eventData) => Board.HandleTileClicked(PieceIndex);
        }
    }
}
