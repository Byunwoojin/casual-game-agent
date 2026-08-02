using System.Collections;
using UnityEngine;
using SpotDiff.Core;
using SpotDiff.Data;
using SpotDiff.Systems;

namespace SpotDiff.Gameplay
{
    /// <summary>
    /// Gameplay 씬 연결 스크립트 — 레벨 로드, DiffGameBoard 바인딩, 힌트/광고/완료 전환.
    /// </summary>
    public class GameplaySceneController : MonoBehaviour
    {
        [SerializeField] private DiffGameBoard _board;
        [SerializeField] private float _completeDelay = 1.2f;

        private LevelDataSO _level;
        private bool _completed;

        private void Start()
        {
            if (_board == null)
                _board = FindAnyObjectByType<DiffGameBoard>(FindObjectsInactive.Include);

            int idx = LevelManager.Instance != null ? LevelManager.Instance.currentLevelIndex : 0;
            _level = Resources.Load<LevelDataSO>($"Levels/Level_{(idx + 1):D3}");
            if (_level == null)
            {
                Debug.LogError($"[GameplaySceneController] Levels/Level_{(idx + 1):D3} 로드 실패");
                return;
            }

            AdjustBoardTopForHeader();
            _board.Build(_level);
            _board.OnCompleted += HandleComplete;

            var hintMgr = HintManager.Instance;
            if (hintMgr != null) hintMgr.OnHintRequested += HandleHintRequested;

            AdManager.Instance?.OnLevelStart(_level.levelNumber);
            AdManager.Instance?.ShowBanner();

            // AudioManager 강제 인스턴스화 — Awake 가 호출돼야 BGM 자동 재생 정책 발동.
            _ = AudioManager.Instance;
        }

        /// <summary>
        /// SceneSetupTool 이 심어둔 헤더 높이(168)는 SafeArea 상단 확장(SafeAreaTopExtender) 을 반영 못 함.
        /// 헤더 GameObject 의 실제 런타임 높이(노치 inset 포함)를 읽어서 보드 상단 오프셋을 다시 맞춘다.
        /// </summary>
        private void AdjustBoardTopForHeader()
        {
            if (_board == null) return;

            var headerGO = GameObject.Find("GameplayHeader");
            if (headerGO == null) return;

            var headerRt = headerGO.transform as RectTransform;
            var boardRt  = _board.transform as RectTransform;
            if (headerRt == null || boardRt == null) return;

            boardRt.offsetMax = new Vector2(boardRt.offsetMax.x, -headerRt.sizeDelta.y);
        }

        private void HandleHintRequested()
        {
            if (_board != null && _board.RequestHint())
                HintManager.Instance.ConsumeHint();
        }

        private void HandleComplete()
        {
            if (_completed) return;
            _completed = true;

            int idx = LevelManager.Instance != null ? LevelManager.Instance.currentLevelIndex : 0;
            SaveManager.MarkLevelComplete(idx);
            AdManager.Instance?.OnLevelComplete(_level.levelNumber);

            StartCoroutine(GoToCompleteAfterDelay());
        }

        private IEnumerator GoToCompleteAfterDelay()
        {
            yield return new WaitForSeconds(_completeDelay);
            SceneLoader.LoadScene(SceneLoader.Complete);
        }

        private void OnDestroy()
        {
            if (_board != null) _board.OnCompleted -= HandleComplete;

            var hintMgr = HintManager.Instance;
            if (hintMgr != null) hintMgr.OnHintRequested -= HandleHintRequested;

            AdManager.Instance?.HideBanner();
        }
    }
}
