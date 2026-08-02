using UnityEngine;

namespace SlidingPuzzle.UI
{
    /// <summary>
    /// RectTransform 을 Screen.safeArea 에 맞춤 — iOS 노치 / Android 네비게이션 바 회피.
    /// 붙인 대상의 anchorMin / anchorMax 를 매 프레임 safeArea 비율로 동기화한다.
    /// (offsetMin/Max 는 유지되므로 내부 마진이 있으면 safeArea 내부에서 추가로 적용된다)
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rt;
        private Rect          _lastSafeArea;
        private Vector2Int    _lastScreenSize;
        private Vector2       _savedOffsetMin;
        private Vector2       _savedOffsetMax;

        private void OnEnable()
        {
            _rt = GetComponent<RectTransform>();
            _savedOffsetMin = _rt.offsetMin;
            _savedOffsetMax = _rt.offsetMax;
            Apply();
        }

        private void Update()
        {
            // 기기 회전 / 스플릿뷰 / 에디터 Game view 크기 변경 대응
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

            if (Screen.width <= 0 || Screen.height <= 0) return;

            var sa = Screen.safeArea;
            var anchorMin = sa.position;
            var anchorMax = sa.position + sa.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _rt.anchorMin = anchorMin;
            _rt.anchorMax = anchorMax;
            _rt.offsetMin = _savedOffsetMin; // 사용자가 지정한 내부 마진 유지
            _rt.offsetMax = _savedOffsetMax;
        }
    }
}
