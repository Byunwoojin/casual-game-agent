using UnityEngine;
using PuzzlePals.Core;
using PuzzlePals.Data;
using PuzzlePals.Puzzle;
using PuzzlePals.Systems;

namespace PuzzlePals.Gameplay
{
    /// <summary>
    /// LevelManager.currentLevelIndex 로 Resources/Levels/Level_XXX.asset 를 로드해
    /// PuzzleBoard.Build() 에 위임하여 직소 조각/스냅존을 씬에 스폰한다.
    /// Awake 에서 자동 실행 → 같은 프레임의 모든 Start() 보다 먼저 끝남
    /// (GameplaySceneController.Start 가 SpawnedPieces 를 안전하게 읽을 수 있도록).
    ///
    /// 성인 피벗 (v0.4) 이후: 기존 piecePrefab/snapZonePrefab/pieceTrayBounds/ApplyBackground 제거.
    /// PuzzleBoard 가 런타임에 MeshRenderer 기반 조각을 직접 생성.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameplayLoader : MonoBehaviour
    {
        [Header("자동 실행")]
        [SerializeField] private bool buildOnAwake = true;

        public LevelDataSO   Level         { get; private set; }
        public PuzzlePiece[] SpawnedPieces { get; private set; } = System.Array.Empty<PuzzlePiece>();
        public SnapZone[]    SpawnedZones  { get; private set; } = System.Array.Empty<SnapZone>();

        private PuzzleBoard _board;

        private void Awake()
        {
            if (buildOnAwake) Build();
        }

        public void Build()
        {
            int idx     = LevelManager.Instance != null ? LevelManager.Instance.currentLevelIndex : 0;
            string path = $"Levels/Level_{(idx + 1):D3}";
            Level       = Resources.Load<LevelDataSO>(path);

            if (Level == null)
            {
                Debug.LogError($"[GameplayLoader] {path} 로드 실패 (LevelManager.currentLevelIndex={idx})");
                return;
            }

            if (Level.characterSprite == null)
            {
                Debug.LogWarning($"[GameplayLoader] {Level.name}.characterSprite 가 비어 있음 → 조각 생성 스킵");
                return;
            }

            // PuzzleBoard 재사용 또는 새로 부착
            if (_board == null) _board = GetComponent<PuzzleBoard>();
            if (_board == null) _board = gameObject.AddComponent<PuzzleBoard>();

            _board.Build(Level);

            // 성인 피벗 v0.4: L2+ 시작 인터스티셜 (L1 무광고)
            AdManager.Instance?.OnLevelStart(Level.levelNumber);

            // Step 4-B: 중간 진행 축하 컨트롤러 자동 부착 (idempotent)
            var celebration = GetComponent<ProgressCelebrationController>();
            if (celebration == null) celebration = gameObject.AddComponent<ProgressCelebrationController>();
            celebration.Init(_board, Level.levelNumber);

            // GameplaySceneController 호환 — 배열 형태로 노출
            SpawnedPieces = new PuzzlePiece[_board.Pieces.Count];
            for (int i = 0; i < SpawnedPieces.Length; i++) SpawnedPieces[i] = _board.Pieces[i];

            SpawnedZones = new SnapZone[_board.SnapZones.Count];
            for (int i = 0; i < SpawnedZones.Length; i++) SpawnedZones[i] = _board.SnapZones[i];
        }
    }
}
