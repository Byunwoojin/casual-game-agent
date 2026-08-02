using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SpotDiff.Data;
using SpotDiff.Systems;

namespace SpotDiff.Gameplay
{
    /// <summary>
    /// 틀린그림찾기 핵심 게임플레이 — 원본/변형 이미지를 좌우로 나란히 보여주되,
    /// 정답 판정은 오른쪽 "변형" 이미지를 탭했을 때만 처리한다 (원본은 대조용, 비인터랙티브).
    ///
    /// 좌표 규약: LevelDataSO.diffRegions 는 sourceImageWidth/Height 기준 픽셀 좌표 (좌상단 원점, y 아래로 증가).
    /// 변형 이미지는 Image(preserveAspect) 로 표시되므로, 탭 지점을 실제 표시 영역(letterbox 보정) 기준
    /// 0~1 정규화한 뒤 픽셀 좌표로 역산해서 diffRegions 와 비교한다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DiffGameBoard : MonoBehaviour
    {
        [Tooltip("정답 판정 여유 — 원본 픽셀 기준. 조각이 작아도 모바일에서 탭하기 쉽게.")]
        [SerializeField] private float _tapToleranceExtraPx = 40f;

        [SerializeField] private RectTransform _originalImageRect;
        [SerializeField] private Image         _originalImage; // 대조용 — 탭 판정 없음

        [SerializeField] private RectTransform _imageRect;      // 변형 이미지 — 탭 판정 대상
        [SerializeField] private Image         _image;
        [SerializeField] private Transform     _markersParent;

        /// <summary>모든 diff 를 찾으면 한 번 발행.</summary>
        public event Action OnCompleted;

        /// <summary>발견 개수 변경 시 (foundCount, totalCount).</summary>
        public event Action<int, int> OnProgressChanged;

        public LevelDataSO Level { get; private set; }
        public int   FoundCount => _foundCount;
        public int   TotalCount => _total;
        public float Progress01 => _total == 0 ? 0f : (float)_foundCount / _total;
        public bool  IsComplete => _total > 0 && _foundCount >= _total;

        private bool[] _found;
        private int    _foundCount;
        private int    _total;

        /// <summary>
        /// 자체 UI 트리(이미지 + 마커 레이어)가 없으면 만들고, 있으면 재사용 — LevelSelectController 등과
        /// 동일한 "idempotent 런타임 셀프 빌드" 패턴. 헤더/배너 영역을 뺀 사각 영역에 채워짐.
        /// </summary>
        public void Build(LevelDataSO level)
        {
            EnsureUI();

            Level = level;

            for (int i = _markersParent.childCount - 1; i >= 0; i--)
                Destroy(_markersParent.GetChild(i).gameObject);

            _originalImage.sprite         = level.originalSprite;
            _originalImage.preserveAspect = true;
            _originalImage.color          = Color.white;

            _image.sprite         = level.transformedSprite;
            _image.preserveAspect = true;
            _image.color          = Color.white;

            _total       = level.diffRegions?.Length ?? 0;
            _found       = new bool[_total];
            _foundCount  = 0;
        }

        /// <summary>변형 이미지 탭 판정 — TransformedImageClickTarget 이 전달.</summary>
        internal void HandleTransformedImageClick(PointerEventData eventData)
        {
            if (Level == null || IsComplete) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _imageRect, eventData.position, eventData.pressEventCamera, out var localPoint))
                return;

            var displayRect = GetDisplayedImageRect();
            if (!displayRect.Contains(localPoint))
                return; // letterbox 여백 탭 — 무시

            float u = (localPoint.x - displayRect.xMin) / displayRect.width;
            float v = (localPoint.y - displayRect.yMin) / displayRect.height;
            float px = u * Level.sourceImageWidth;
            float py = (1f - v) * Level.sourceImageHeight; // UI y-up → 이미지 y-down 보정

            if (!TryFindAt(px, py))
            {
                AudioManager.Instance?.PlayMiss();
                SpawnMissMarker(localPoint);
            }
        }

        private bool TryFindAt(float px, float py)
        {
            for (int i = 0; i < Level.diffRegions.Length; i++)
            {
                if (_found[i]) continue;
                if (Level.diffRegions[i].IsHit(px, py, _tapToleranceExtraPx))
                {
                    MarkFound(i);
                    return true;
                }
            }
            return false;
        }

        private void MarkFound(int index)
        {
            _found[index] = true;
            _foundCount++;

            SpawnMarker(Level.diffRegions[index], pulsing: false);

            OnProgressChanged?.Invoke(_foundCount, _total);
            AudioManager.Instance?.PlayFound();

            if (_foundCount >= _total)
            {
                AudioManager.Instance?.PlayLevelComplete();
                OnCompleted?.Invoke();
            }
        }

        /// <summary>힌트 — 미발견 diff 하나를 반짝여서 위치만 알려줌 (자동 발견 처리 아님). 성공 시 true.</summary>
        public bool RequestHint()
        {
            for (int i = 0; i < _found.Length; i++)
            {
                if (_found[i]) continue;
                SpawnMarker(Level.diffRegions[i], pulsing: true);
                return true;
            }
            return false;
        }

        // ─────────────────────────────────────────────────────
        // PlayMode E2E 테스트 지원 — 실제 필드가 diff_result.json 기반이라 좌표가 결정적이므로,
        // 화면 좌표를 역산해서 실제 탭과 동일한 경로(HandleTransformedImageClick)를 태울 수 있게 함.
        // ─────────────────────────────────────────────────────

        /// <summary>테스트 전용 — index 번째 diffRegion 중심점에 대응하는 실제 화면 좌표.</summary>
        public Vector2 DebugGetScreenPositionForRegion(int index)
        {
            var r = Level.diffRegions[index];
            var displayRect = GetDisplayedImageRect();
            float u = r.centerX / Level.sourceImageWidth;
            float v = 1f - (r.centerY / Level.sourceImageHeight);
            var localPoint = new Vector2(displayRect.xMin + u * displayRect.width, displayRect.yMin + v * displayRect.height);
            var worldPoint = _imageRect.TransformPoint(localPoint);
            var canvas = _imageRect.GetComponentInParent<Canvas>();
            var cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
            return RectTransformUtility.WorldToScreenPoint(cam, worldPoint);
        }

        /// <summary>테스트 전용 — 변형 이미지(탭 판정 대상)의 클릭 타겟 GameObject.</summary>
        public GameObject DebugTransformedImageGO => _image != null ? _image.gameObject : null;

        /// <summary>오답 탭 위치에 짧게 X 표시 — 정답 여부와 무관하게 "탭이 인식됐다"는 시각 피드백.</summary>
        private void SpawnMissMarker(Vector2 localPoint)
        {
            var go = new GameObject("MissMarker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(_markersParent, worldPositionStays: false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(84f, 84f);
            rt.anchoredPosition = localPoint;

            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.sprite = XSprite();
            img.color  = new Color(0.85f, 0.25f, 0.25f, 0.95f);

            // 바로 보이게 스케일 1로 시작 (트윈 실패해도 화면엔 뜸) — 트윈은 덤 연출.
            go.transform.localScale = Vector3.one;
            go.transform.DOPunchScale(Vector3.one * 0.3f, 0.25f, 6, 0.6f);
            Destroy(go, 0.9f);
        }

        private void SpawnMarker(DiffRegion r, bool pulsing)
        {
            var displayRect = GetDisplayedImageRect();

            float u = r.centerX / Level.sourceImageWidth;
            float v = 1f - (r.centerY / Level.sourceImageHeight);
            float localX = displayRect.xMin + u * displayRect.width;
            float localY = displayRect.yMin + v * displayRect.height;

            var go = new GameObject(pulsing ? "HintMarker" : "FoundMarker",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(_markersParent, worldPositionStays: false);

            float sizePx = Mathf.Max(displayRect.width / Level.sourceImageWidth * Mathf.Max(r.width, r.height) * 2.0f, 84f);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(sizePx, sizePx);
            rt.anchoredPosition = new Vector2(localX, localY);

            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.sprite = RingSprite();
            img.type   = Image.Type.Simple;
            img.color  = pulsing ? new Color(1f, 0.82f, 0.2f, 0.95f) : new Color(0.3f, 0.85f, 0.5f, 0.95f);

            // 바로 보이게 스케일 1로 시작 (트윈 실패해도 화면엔 뜸) — 트윈은 덤 연출.
            go.transform.localScale = Vector3.one;

            if (pulsing)
            {
                go.transform.DOScale(1.15f, 0.3f).SetLoops(6, LoopType.Yoyo);
                // 5초 후 자동 제거 — 힌트는 영구 표시가 아니라 위치 안내
                Destroy(go, 5f);
            }
            else
            {
                go.transform.DOPunchScale(Vector3.one * 0.25f, 0.35f, 6, 0.6f);
            }
        }

        private static Sprite _ringSpriteCache;
        private static Sprite RingSprite()
        {
            if (_ringSpriteCache != null) return _ringSpriteCache;

            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size / 2f, size / 2f);
            const float outerR = 30f, innerR = 22f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float a = (d <= outerR && d >= innerR) ? 1f : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;

            _ringSpriteCache = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _ringSpriteCache;
        }

        private static Sprite _xSpriteCache;
        private static Sprite XSprite()
        {
            if (_xSpriteCache != null) return _xSpriteCache;

            const int size = 64;
            const float thickness = 8f;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float fx = x + 0.5f, fy = y + 0.5f;
                float d1 = Mathf.Abs(fy - fx) * 0.70710678f;              // 대각선 \ 까지 거리
                float d2 = Mathf.Abs(fy - (size - fx)) * 0.70710678f;     // 대각선 / 까지 거리
                float d  = Mathf.Min(d1, d2);
                float a  = d <= thickness * 0.5f ? 1f : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;

            _xSpriteCache = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _xSpriteCache;
        }

        /// <summary>Image(preserveAspect) 실제 표시 영역 — RectTransform.rect 기준 letterbox 보정.</summary>
        private Rect GetDisplayedImageRect()
        {
            Rect full = _imageRect.rect;
            if (_image.sprite == null) return full;

            float spriteAspect = (float)_image.sprite.texture.width / _image.sprite.texture.height;
            float rectAspect   = full.width / full.height;

            float w, h;
            if (spriteAspect > rectAspect) { w = full.width;  h = w / spriteAspect; }
            else                           { h = full.height; w = h * spriteAspect; }

            float x = full.x + (full.width  - w) * 0.5f;
            float y = full.y + (full.height - h) * 0.5f;
            return new Rect(x, y, w, h);
        }

        /// <summary>
        /// 상(원본, 비인터랙티브) / 하(변형, 탭 판정) 2분할 UI 생성 — 이미 있으면 재사용.
        /// 좌우 분할 대신 위/아래로 쌓아서 각 이미지가 화면 폭을 온전히 써서 더 크게 보이게 함
        /// (원본 이미지가 702×768~714 로 가로세로 비슷해서, 좌우 분할이면 letterbox 여백이 커져 작아 보임).
        /// </summary>
        private void EnsureUI()
        {
            if (_imageRect != null && _image != null && _markersParent != null
                && _originalImageRect != null && _originalImage != null) return;

            const float halfGap = 8f;

            // 상단 — 원본 (대조용, 탭 무시)
            var origGo = new GameObject("OriginalImage",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            origGo.transform.SetParent(transform, worldPositionStays: false);
            _originalImageRect = (RectTransform)origGo.transform;
            _originalImageRect.anchorMin = new Vector2(0f, 0.5f);
            _originalImageRect.anchorMax = new Vector2(1f, 1f);
            _originalImageRect.offsetMin = new Vector2(0f, halfGap);
            _originalImageRect.offsetMax = Vector2.zero;
            _originalImage = origGo.GetComponent<Image>();
            _originalImage.raycastTarget = false;

            // 하단 — 변형 (탭 판정 대상)
            var imgGo = new GameObject("TransformedImage",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imgGo.transform.SetParent(transform, worldPositionStays: false);
            _imageRect = (RectTransform)imgGo.transform;
            _imageRect.anchorMin = new Vector2(0f, 0f);
            _imageRect.anchorMax = new Vector2(1f, 0.5f);
            _imageRect.offsetMin = Vector2.zero;
            _imageRect.offsetMax = new Vector2(0f, -halfGap);
            _image = imgGo.GetComponent<Image>();
            _image.raycastTarget = true; // 탭 수신

            var clickTarget = imgGo.AddComponent<TransformedImageClickTarget>();
            clickTarget.Board = this;

            var markersGo = new GameObject("Markers", typeof(RectTransform));
            markersGo.transform.SetParent(imgGo.transform, worldPositionStays: false);
            var mRt = (RectTransform)markersGo.transform;
            mRt.anchorMin = Vector2.zero;
            mRt.anchorMax = Vector2.one;
            mRt.offsetMin = Vector2.zero;
            mRt.offsetMax = Vector2.zero;
            _markersParent = markersGo.transform;
        }

        /// <summary>변형 이미지에만 부착 — 탭을 DiffGameBoard.HandleTransformedImageClick 으로 전달.</summary>
        private class TransformedImageClickTarget : MonoBehaviour, IPointerClickHandler
        {
            public DiffGameBoard Board;
            public void OnPointerClick(PointerEventData eventData) => Board.HandleTransformedImageClick(eventData);
        }
    }
}
