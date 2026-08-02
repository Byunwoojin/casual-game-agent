using UnityEngine;

namespace MemoryMatch.Systems
{
    /// <summary>
    /// 파티클 burst 재생용 싱글톤. 씬에 없으면 자동 생성(프리팹 미설정 시 무동작).
    /// </summary>
    [DisallowMultipleComponent]
    public class VFXManager : MonoBehaviour
    {
        private static VFXManager _instance;

        public static VFXManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                if (!Application.isPlaying) return null;

                var existing = FindAnyObjectByType<VFXManager>(FindObjectsInactive.Include);
                if (existing != null) { _instance = existing; return _instance; }

                var go = new GameObject(nameof(VFXManager));
                _instance = go.AddComponent<VFXManager>();
                return _instance;
            }
        }

        [Header("Particle Prefabs (선택)")]
        [Tooltip("스냅 성공 시 별 5 burst 권장")]
        [SerializeField] private ParticleSystem snapBurstPrefab;

        [Tooltip("레벨 클리어 시 무지개 폭발 권장")]
        [SerializeField] private ParticleSystem completeBurstPrefab;

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

        public void PlaySnapBurst(Vector3 worldPos)     => Spawn(snapBurstPrefab, worldPos);
        public void PlayCompleteBurst(Vector3 worldPos) => Spawn(completeBurstPrefab, worldPos);

        private static void Spawn(ParticleSystem prefab, Vector3 worldPos)
        {
            if (prefab == null) return;

            var ps = Instantiate(prefab, worldPos, Quaternion.identity);
            ps.Play();

            float life = ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(ps.gameObject, life);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
