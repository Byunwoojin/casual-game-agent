using UnityEngine;
using UnityEngine.UI;

namespace PuzzlePals.UI
{
    /// <summary>
    /// SafeArea (a) 패턴 — 헤더 GameObject 가 노치 위까지 화면 끝으로 stretch + 자식 컨테이너만 SafeArea inset.
    ///
    /// 부착된 RectTransform 의 sizeDelta.y 를 (baseHeight + safeArea top inset) 으로 매 프레임 동기화.
    /// 자식 컨테이너 (innerContent) 는 anchor stretch full + offsetMax.y = -topInsetRef 로 SafeArea 안에 위치.
    /// → 헤더 background (흰색) 이 노치 영역까지 그려지고, 자식 (텍스트/버튼) 은 SafeArea 안.
    ///
    /// SafeAreaFitter (RectTransform 자체를 SafeArea 안으로 옮김) 와 다름 —
    /// 이건 헤더 색상이 노치까지 이어지는 모바일 표준 패턴 용.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaTopExtender : MonoBehaviour
    {
        [Tooltip("자식 컨테이너 — top inset 만큼 push 됨. 보통 헤더 안의 Toolbar/Inner 자식.")]
        public RectTransform innerContent;

        private RectTransform _rt;
        private float         _baseHeight;
        private Rect          _lastSafeArea;
        private Vector2Int    _lastScreenSize;

        private void OnEnable()
        {
            _rt = (RectTransform)transform;
            _baseHeight = _rt.sizeDelta.y;
            Apply();
        }

        /// <summary>외부에서 innerContent 를 set 한 직후 호출 — Apply 즉시 재실행.</summary>
        public void Refresh()
        {
            if (_rt == null) _rt = (RectTransform)transform;
            // _baseHeight 는 OnEnable 시 측정된 값 유지 — sizeDelta 가 이미 base + inset 으로 변경된 상태에서
            //   재호출해도 base 가 누적되지 않게 함.
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea != _lastSafeArea
                || Screen.width  != _lastScreenSize.x
                || Screen.height != _lastScreenSize.y)
            {
                Apply();
            }
        }

        private void Apply()
        {
            _lastSafeArea   = Screen.safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            if (Screen.height <= 0) return;

            // Top inset (device px). 일부 시뮬레이터/디바이스가 SafeArea 를 비정상 reported (0 또는 매우 큼).
            //   상식 범위 (Screen.height 의 1~10%) 밖이면 표준 status bar 추정값 (4%) fallback.
            float topInsetDevice = Screen.height - (Screen.safeArea.y + Screen.safeArea.height);
            float minInset = Mathf.Max(75f, Screen.height * 0.04f);  // 표준 status bar
            float maxInset = Screen.height * 0.10f;                  // 노치 큰 디바이스도 10% 이내
            if (topInsetDevice < minInset || topInsetDevice > maxInset)
                topInsetDevice = minInset;

            // Canvas RT 의 logical height 기준 변환 (ScaleWithScreenSize 모드 정확)
            var canvas = GetComponentInParent<Canvas>();
            float canvasH = 1920f;
            if (canvas != null)
            {
                var canvasRt = canvas.transform as RectTransform;
                if (canvasRt != null) canvasH = canvasRt.rect.height;
            }
            float topInsetRef = topInsetDevice * (canvasH / Screen.height);

            // Header height 확장 — base + safeArea top
            _rt.sizeDelta = new Vector2(_rt.sizeDelta.x, _baseHeight + topInsetRef);

            // 자식 컨테이너 — Header 안에서 top inset 만큼 push
            if (innerContent != null)
            {
                innerContent.anchorMin = new Vector2(0f, 0f);
                innerContent.anchorMax = new Vector2(1f, 1f);
                innerContent.offsetMin = Vector2.zero;
                innerContent.offsetMax = new Vector2(0f, -topInsetRef);
            }
        }
    }
}
