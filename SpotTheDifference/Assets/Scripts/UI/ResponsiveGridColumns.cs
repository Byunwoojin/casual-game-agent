using UnityEngine;
using UnityEngine.UI;

namespace SpotDiff.UI
{
    /// <summary>
    /// GridLayoutGroup 의 컬럼 수를 실제 rect 폭에 따라 자동 조정.
    /// 좁은 폰 세로 → 적은 컬럼, 태블릿 / 가로 → 많은 컬럼.
    /// cellSize / spacing / padding 기반으로 적합한 constraintCount 계산.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GridLayoutGroup))]
    [RequireComponent(typeof(RectTransform))]
    public class ResponsiveGridColumns : MonoBehaviour
    {
        [SerializeField] private int minColumns = 3;
        [SerializeField] private int maxColumns = 7;

        private GridLayoutGroup _grid;
        private RectTransform   _rt;

        private void OnEnable()
        {
            _grid = GetComponent<GridLayoutGroup>();
            _rt   = (RectTransform)transform;
            Apply();
        }

        // Unity 콜백 — rect 크기 변경 시 자동 호출 (캔버스 리사이즈, 기기 회전 등)
        private void OnRectTransformDimensionsChange()
        {
            if (_grid != null) Apply();
        }

        private void Apply()
        {
            float width     = _rt.rect.width;
            float cellW     = _grid.cellSize.x;
            float spacingX  = _grid.spacing.x;
            int   padding   = _grid.padding.left + _grid.padding.right;

            if (width <= 0f || cellW <= 0f) return;

            // N 컬럼 필요 폭: N*cell + (N-1)*spacing + padding
            // 풀어서 N = floor((width - padding + spacing) / (cell + spacing))
            int cols = Mathf.FloorToInt((width - padding + spacingX) / (cellW + spacingX));
            cols = Mathf.Clamp(cols, minColumns, maxColumns);

            // 변화가 있을 때만 적용 — OnRectTransformDimensionsChange 루프 방지
            if (_grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount
                && _grid.constraintCount == cols) return;

            _grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            _grid.constraintCount = cols;
        }
    }
}
