using UnityEngine;

namespace PuzzlePals.UI
{
    /// <summary>
    /// 카메라 viewport 를 목표 종횡비(기본 9:16) 로 letterbox.
    /// 화면 비율이 다르면 카메라 rect 가 줄어들고, 외부는 배경 카메라가 검정으로 채움.
    /// 게임 월드도 UI 와 동일한 비율로 보이게 보장.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public class LetterboxCamera : MonoBehaviour
    {
        [Tooltip("목표 종횡비 (width / height). 9:16 portrait = 0.5625")]
        [SerializeField] private float targetAspect = 9f / 16f;

        private Camera _cam;
        private float  _lastScreenAspect;

        private void OnEnable()
        {
            _cam = GetComponent<Camera>();
            Apply();
        }

        private void Update()
        {
            float currentAspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            if (Mathf.Abs(currentAspect - _lastScreenAspect) > 0.001f) Apply();
        }

        private void Apply()
        {
            if (_cam == null) return;
            if (Screen.height <= 0) return;

            float windowAspect = (float)Screen.width / Screen.height;
            float scaleHeight  = windowAspect / targetAspect;

            Rect rect = new Rect(0f, 0f, 1f, 1f);

            if (scaleHeight < 1f)
            {
                // 화면이 목표보다 좁음 (세로로 김) → 위/아래 letterbox bars
                rect.width  = 1f;
                rect.height = scaleHeight;
                rect.x      = 0f;
                rect.y      = (1f - scaleHeight) * 0.5f;
            }
            else
            {
                // 화면이 목표보다 넓음 (가로로 김) → 좌/우 pillarbox bars
                float scaleWidth = 1f / scaleHeight;
                rect.width  = scaleWidth;
                rect.height = 1f;
                rect.x      = (1f - scaleWidth) * 0.5f;
                rect.y      = 0f;
            }

            _cam.rect          = rect;
            _lastScreenAspect  = windowAspect;
        }
    }
}
