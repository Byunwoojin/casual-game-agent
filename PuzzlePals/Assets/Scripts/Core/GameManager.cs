using UnityEngine;

namespace PuzzlePals.Core
{
    public enum GameState { Playing, Paused, Complete }

    /// <summary>게임 전체 상태 관리 싱글톤</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; set; } = GameState.Playing;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>PuzzleCompleteChecker.OnPuzzleComplete 이벤트 핸들러</summary>
        public void OnPuzzleComplete()
        {
            State = GameState.Complete;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
