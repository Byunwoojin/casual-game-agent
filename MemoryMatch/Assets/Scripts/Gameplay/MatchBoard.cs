using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MemoryMatch.Data;
using MemoryMatch.Systems;
using MemoryMatch.UI;

namespace MemoryMatch.Gameplay
{
    /// <summary>
    /// 메모리 매칭(카드 뒤집기) 렌더링/입력 — 그리드 상태 로직은 MemoryMatchState 에 위임.
    /// 카드를 탭하면 뒤집고, 두 번째 카드를 탭한 시점에 동기 비교 — 일치하면 그대로 유지,
    /// 불일치하면 잠깐 보여준 뒤 자동으로 다시 가림 (그 동안 추가 입력 잠금).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MatchBoard : MonoBehaviour
    {
        [SerializeField] private RectTransform _gridAreaRect;
        [SerializeField] private RectTransform _cardsParent;

        private const float MismatchDelay = 0.7f;
        private const float HintPeekDelay = 1.5f; // 두 장을 동시에 봐야 해서 기존(1장 기준)보다 살짝 늘림

        public event System.Action OnCompleted;
        public event System.Action<int, int> OnProgressChanged;

        public LevelDataSO Level { get; private set; }
        public int   FoundCount => _state?.MatchedPairs ?? 0;
        public int   TotalCount => _state?.PairCount ?? 0;
        public float Progress01 => _state == null || _state.PairCount == 0 ? 0f : (float)FoundCount / _state.PairCount;
        public bool  IsComplete => _state != null && _state.IsComplete;

        private MemoryMatchState _state;
        private Image[] _cardImages;
        private Sprite  _backSprite;
        private bool    _inputLocked;

        public void Build(LevelDataSO level)
        {
            EnsureUI();
            Level = level;
            _state = new MemoryMatchState(level.pairCount, new System.Random());
            _inputLocked = false;

            for (int i = _cardsParent.childCount - 1; i >= 0; i--)
                Destroy(_cardsParent.GetChild(i).gameObject);

            LayoutAndBuildCards();
        }

        internal void HandleCardClicked(int index)
        {
            if (Level == null || IsComplete || _inputLocked) return;

            var result = _state.FlipCard(index);
            if (result == MemoryMatchState.FlipResult.Invalid) return;

            ShowCardFace(index);
            if (result == MemoryMatchState.FlipResult.FirstFlip) return;

            if (result == MemoryMatchState.FlipResult.Match)
            {
                AudioManager.Instance?.PlayFound();
                OnProgressChanged?.Invoke(_state.MatchedPairs, _state.PairCount);

                if (_state.IsComplete)
                {
                    AudioManager.Instance?.PlayLevelComplete();
                    OnCompleted?.Invoke();
                }
            }
            else
            {
                AudioManager.Instance?.PlayMiss();
                _inputLocked = true;
                StartCoroutine(ResolveMismatchAfterDelay());
            }
        }

        private IEnumerator ResolveMismatchAfterDelay()
        {
            yield return new WaitForSeconds(MismatchDelay);
            int first = _state.PendingFirst, second = _state.PendingSecond;
            _state.ResolveMismatch();
            if (first >= 0)  HideCardFace(first);
            if (second >= 0) HideCardFace(second);
            _inputLocked = false;
        }

        /// <summary>못 맞춘 짝 두 장을 잠깐 동시에 보여줬다 가림 — 자동 매치 아님, 위치만 안내.</summary>
        public bool RequestHint()
        {
            if (_state == null || _inputLocked || IsComplete) return false;
            var pair = _state.FindHintPair();
            if (pair == null) return false;

            _inputLocked = true;
            ShowCardFace(pair.Value.first);
            ShowCardFace(pair.Value.second);
            StartCoroutine(HideHintPairAfterDelay(pair.Value.first, pair.Value.second));
            return true;
        }

        private IEnumerator HideHintPairAfterDelay(int first, int second)
        {
            yield return new WaitForSeconds(HintPeekDelay);
            HideCardFace(first);
            HideCardFace(second);
            _inputLocked = false;
        }

        private void ShowCardFace(int index)
        {
            int pairId = _state.PairIdAt(index);
            var img = _cardImages[index];
            img.sprite = Level.cardSprites[pairId % Level.cardSprites.Length];
            img.type   = Image.Type.Simple;
            img.color  = Color.white;

            img.transform.DOKill();
            img.transform.localScale = Vector3.one;
            img.transform.DOPunchScale(Vector3.one * 0.08f, 0.2f, 4, 0.5f);
        }

        private void HideCardFace(int index)
        {
            var img = _cardImages[index];
            img.sprite = _backSprite;
            img.type   = Image.Type.Sliced;
            img.color  = PaletteTokens.Active.SurfaceAlt;
        }

        // ─────────────────────────────────────────────────────
        // PlayMode E2E 테스트 지원 — 셔플이 랜덤이라 미리 정해진 카드를 클릭할 수 없으므로,
        // 런타임에 "지금 아직 안 맞춘 짝"과 각 카드의 pairId 를 질의해서 항상 유효한 탭 대상을 구함.
        // ─────────────────────────────────────────────────────

        /// <summary>테스트 전용 — 아직 못 맞춘 짝의 (첫 카드, 둘째 카드) 인덱스. 없으면 null.</summary>
        public (int first, int second)? DebugFindMatchingPair() => _state?.FindHintPair();

        /// <summary>테스트 전용 — index 번째 카드의 pairId.</summary>
        public int DebugPairIdAt(int index) => _state.PairIdAt(index);

        /// <summary>테스트 전용 — index 번째 카드 GameObject.</summary>
        public GameObject DebugGetCardGO(int index)
            => (_cardImages != null && index >= 0 && index < _cardImages.Length) ? _cardImages[index].gameObject : null;

        // ─────────────────────────────────────────────────────
        // 레이아웃 / 카드 생성
        // ─────────────────────────────────────────────────────

        private static (int cols, int rows) GridDimsFor(int cardCount)
        {
            const int cols = 4;
            int rows = Mathf.CeilToInt(cardCount / (float)cols);
            return (cols, rows);
        }

        private void LayoutAndBuildCards()
        {
            int cardCount = _state.CardCount;
            var (cols, rows) = GridDimsFor(cardCount);

            const float pad = 24f, gap = 16f;
            float availW = _gridAreaRect.rect.width  - pad * 2f;
            float availH = _gridAreaRect.rect.height - pad * 2f;
            float cellW = (availW - gap * (cols - 1)) / cols;
            float cellH = (availH - gap * (rows - 1)) / rows;
            float cell  = Mathf.Min(cellW, cellH);

            float gridW = cell * cols + gap * (cols - 1);
            float gridH = cell * rows + gap * (rows - 1);

            _cardsParent.anchorMin = _cardsParent.anchorMax = new Vector2(0.5f, 0.5f);
            _cardsParent.pivot     = new Vector2(0f, 1f);
            _cardsParent.sizeDelta = new Vector2(gridW, gridH);
            _cardsParent.anchoredPosition = new Vector2(-gridW * 0.5f, gridH * 0.5f);

            _cardImages = new Image[cardCount];

            for (int i = 0; i < cardCount; i++)
            {
                int col = i % cols;
                int row = i / cols;
                float x = col * (cell + gap) + cell * 0.5f;
                float y = -(row * (cell + gap) + cell * 0.5f);

                var go = new GameObject($"Card_{i:D2}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(_cardsParent, worldPositionStays: false);
                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot     = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(cell, cell);
                rt.anchoredPosition = new Vector2(x, y);

                var img = go.GetComponent<Image>();
                img.sprite = _backSprite;
                img.type   = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 1f;
                img.color  = PaletteTokens.Active.SurfaceAlt;

                var click = go.AddComponent<CardClickTarget>();
                click.Board     = this;
                click.CardIndex = i;

                _cardImages[i] = img;
            }
        }

        private void EnsureUI()
        {
            if (_gridAreaRect != null && _cardsParent != null) return;

            _backSprite = PaletteUI.RoundedRect(16);

            var gridAreaGo = new GameObject("GridArea", typeof(RectTransform));
            gridAreaGo.transform.SetParent(transform, worldPositionStays: false);
            _gridAreaRect = (RectTransform)gridAreaGo.transform;
            _gridAreaRect.anchorMin = Vector2.zero;
            _gridAreaRect.anchorMax = Vector2.one;
            _gridAreaRect.offsetMin = Vector2.zero;
            _gridAreaRect.offsetMax = Vector2.zero;

            var cardsGo = new GameObject("CardsParent", typeof(RectTransform));
            cardsGo.transform.SetParent(_gridAreaRect, worldPositionStays: false);
            _cardsParent = (RectTransform)cardsGo.transform;
        }

        /// <summary>각 카드에 부착 — 탭을 MatchBoard.HandleCardClicked 로 전달.</summary>
        private class CardClickTarget : MonoBehaviour, IPointerClickHandler
        {
            public MatchBoard Board;
            public int CardIndex;
            public void OnPointerClick(PointerEventData eventData) => Board.HandleCardClicked(CardIndex);
        }
    }
}
