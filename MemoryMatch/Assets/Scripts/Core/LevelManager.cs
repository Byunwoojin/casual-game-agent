using UnityEngine;

namespace MemoryMatch.Core
{
    /// <summary>
    /// 현재 레벨 인덱스 보관 싱글톤 — 씬 전환 시에도 유지.
    /// 첫 접근 시 자동 생성 (어떤 씬에서 시작하든 안전).
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        private static LevelManager _instance;

        public static LevelManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                if (!Application.isPlaying) return null;

                var existing = FindAnyObjectByType<LevelManager>(FindObjectsInactive.Include);
                if (existing != null) { _instance = existing; return _instance; }

                var go = new GameObject(nameof(LevelManager));
                _instance = go.AddComponent<LevelManager>();
                return _instance;
            }
        }

        public int currentLevelIndex = 0;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
