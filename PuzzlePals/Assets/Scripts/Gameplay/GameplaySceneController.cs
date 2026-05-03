using System.Collections;
using DG.Tweening;
using UnityEngine;
using PuzzlePals.Core;
using PuzzlePals.Puzzle;
using PuzzlePals.Systems;

namespace PuzzlePals.Gameplay
{
    /// <summary>
    /// Gameplay 씬 전용 연결 스크립트
    /// - 매 프레임 PuzzleCompleteChecker.Check() 호출
    /// - 완료 이벤트 수신 시 1.5초 딜레이 후 CompleteUI 활성화
    /// </summary>
    public class GameplaySceneController : MonoBehaviour
    {
        [SerializeField] private PuzzleCompleteChecker _checker;
        [SerializeField] private GameObject            _completeUI;
        [SerializeField] private float                 _completeDelay = 1.5f;

        // 선택 — GameplayLoader 가 씬에서 동적으로 조각을 스폰할 때 그 결과를 직접 받는다.
        // 비어 있으면 기존처럼 FindObjectsByType 로 폴백 (placeholder 씬 / 기존 PlayMode 테스트 호환).
        [SerializeField] private GameplayLoader        _loader;

        private PuzzlePiece[] _pieces;
        private bool          _completed;

        private void Start()
        {
            // _loader 가 인스펙터에서 미할당이면 씬에서 검색 (폴백)
            if (_loader == null)
                _loader = FindAnyObjectByType<GameplayLoader>(FindObjectsInactive.Include);

            // _checker 도 동일 폴백 (씬에 PuzzleCompleteChecker GO 가 있으면 자동 연결)
            if (_checker == null)
                _checker = FindAnyObjectByType<PuzzleCompleteChecker>(FindObjectsInactive.Include);

            _pieces = (_loader != null && _loader.SpawnedPieces != null && _loader.SpawnedPieces.Length > 0)
                ? _loader.SpawnedPieces
                : FindObjectsByType<PuzzlePiece>(FindObjectsInactive.Exclude);

            if (_checker != null)
                _checker.OnPuzzleComplete += HandleComplete;

            if (_completeUI != null)
                _completeUI.SetActive(false);

            // 화면 하단 배너 광고 표시 (모든 레벨, L1 포함). PieceBank 아래 영역에 SDK 가 native overlay 로 그림.
            PuzzlePals.Systems.AdManager.Instance?.ShowBanner();

            // AudioManager 강제 인스턴스화 — Awake 가 호출되어야 BGM 자동 재생 정책 발동.
            // 싱글톤 자동 생성 패턴이라 누군가 Instance 를 한 번 호출해줘야 함 (piece 스냅 전엔 호출처 없음).
            _ = PuzzlePals.Systems.AudioManager.Instance;
        }

        private void Update()
        {
            // Restart (또는 외부 재빌드) 감지 — loader 가 새 SpawnedPieces 배열을 발행했으면 _pieces 갱신
            if (_loader != null && _loader.SpawnedPieces != null
                && _loader.SpawnedPieces.Length > 0
                && _pieces != _loader.SpawnedPieces)
            {
                _pieces    = _loader.SpawnedPieces;
                _completed = false;
                if (_completeUI != null) _completeUI.SetActive(false);
            }

            if (_completed || _checker == null || _pieces == null) return;
            _checker.Check(_pieces);
        }

        private void HandleComplete()
        {
            if (_completed) return;
            _completed = true;

            int idx = LevelManager.Instance != null ? LevelManager.Instance.currentLevelIndex : 0;
            int levelNumber = (_loader != null && _loader.Level != null) ? _loader.Level.levelNumber : (idx + 1);
            Debug.Log($"[GameplaySceneController] HandleComplete fired (level index={idx}, number={levelNumber})");

            SaveManager.MarkLevelComplete(idx);
            // 성인 피벗 v0.4: L2+ 완료 인터스티셜 (L1 무광고)
            AdManager.Instance?.OnLevelComplete(levelNumber);

            PlayCompleteJuice();
            CharacterCompleteAnimation.Play(_pieces);

            StartCoroutine(ShowCompleteAfterDelay());
        }

        // GDD §Game Feel: 완성 = 무지개 파티클 폭발 + DOShakePosition(0.4, 0.3, 10, 45) + 팡파레
        private static void PlayCompleteJuice()
        {
            if (!Application.isPlaying) return;

            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.DOKill();
                cam.transform.DOShakePosition(
                    duration: 0.4f, strength: 0.3f, vibrato: 10, randomness: 45f, snapping: false, fadeOut: true);
            }

            VFXManager.Instance?.PlayCompleteBurst(Vector3.zero);
            AudioManager.Instance?.PlayFanfare();
        }

        private IEnumerator ShowCompleteAfterDelay()
        {
            yield return new WaitForSeconds(_completeDelay);
            Debug.Log("[GameplaySceneController] CompleteUI 활성화");
            if (_completeUI != null)
                _completeUI.SetActive(true);

            yield return new WaitForSeconds(1.0f);
            Debug.Log("[GameplaySceneController] Complete 씬으로 자동 전환 시도");
            SceneLoader.LoadScene(SceneLoader.Complete);
        }

        private void OnDestroy()
        {
            if (_checker != null)
                _checker.OnPuzzleComplete -= HandleComplete;

            // Gameplay 외 씬은 배너 미표시 — 이탈 시 native banner 숨김.
            PuzzlePals.Systems.AdManager.Instance?.HideBanner();
        }
    }
}
