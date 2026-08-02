using UnityEngine;
using UnityEngine.SceneManagement;

namespace SlidingPuzzle.Systems
{
    /// <summary>
    /// 단순 SFX/BGM 재생 싱글톤. 절차적 합성 fallback — 외부 오디오 자산 없이도 무음이 아님.
    /// (MyGames/PuzzlePals 의 AudioManager 패턴 재사용 — 절차적 합성 기법만 유지, 메서드명은 장르 중립으로 정리)
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;

        public static AudioManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                if (!Application.isPlaying) return null;

                var existing = FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
                if (existing != null) { _instance = existing; return _instance; }

                var go = new GameObject(nameof(AudioManager));
                _instance = go.AddComponent<AudioManager>();
                return _instance;
            }
        }

        [Header("SFX Clips (선택 — 비어있으면 절차적 합성)")]
        [SerializeField] private AudioClip foundClip;         // 정답(diff 발견)
        [SerializeField] private AudioClip missClip;          // 오답 탭
        [SerializeField] private AudioClip levelCompleteClip; // 레벨 클리어 (Gameplay 마지막 발견 시점)
        [SerializeField] private AudioClip completeSceneClip; // Complete 씬 진입

        [Header("BGM (선택) — Assets/Audio/BGM/ 폴더 스캔으로 자동 채움")]
        [SerializeField] private AudioClip[] bgmClips;

        [Header("Mix (PlayerPrefs 와 동기화)")]
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.7f;

        private const string SfxPrefKey = "Settings_SfxVolume";
        private const string BgmPrefKey = "Settings_BgmVolume";
        private const string GameplaySceneName = "Gameplay";

        private AudioSource _sfxSource;
        private AudioSource _bgmSource;
        private int  _currentBgmIdx      = -1;
        private bool _bgmStoppedManually = false;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            sfxVolume = PlayerPrefs.GetFloat(SfxPrefKey, sfxVolume);
            bgmVolume = PlayerPrefs.GetFloat(BgmPrefKey, bgmVolume);

            // AudioManager 는 DontDestroyOnLoad 로 항상 살아있음 — AudioListener 도 여기 하나만 두면
            // 씬마다 카메라를 만들지 않아도 "no audio listener" 경고가 안 뜬다.
            if (GetComponent<AudioListener>() == null) gameObject.AddComponent<AudioListener>();

            _sfxSource = GetComponent<AudioSource>();
            if (_sfxSource == null) _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.loop        = false;

            if (foundClip == null)         foundClip         = GenerateFound();
            if (missClip == null)          missClip          = GenerateMiss();
            if (levelCompleteClip == null) levelCompleteClip = GenerateFanfare();
            if (completeSceneClip == null) completeSceneClip = GenerateCompleteSwell();

            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            _bgmSource.loop        = false;
            _bgmSource.volume      = bgmVolume;

            bool inGameplay = SceneManager.GetActiveScene().name == GameplaySceneName;
            if (inGameplay && bgmVolume > 0f) PlayNextBgm();
            else _bgmStoppedManually = true;
        }

        private void OnEnable()  => SceneManager.sceneLoaded += HandleSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= HandleSceneLoaded;

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == GameplaySceneName)
            {
                if (bgmVolume > 0f && bgmClips != null && bgmClips.Length > 0
                    && _bgmSource != null && !_bgmSource.isPlaying)
                {
                    _bgmStoppedManually = false;
                    PlayNextBgm();
                }
            }
            else
            {
                _bgmStoppedManually = true;
                if (_bgmSource != null && _bgmSource.isPlaying) _bgmSource.Stop();
            }
        }

        private void Update()
        {
            if (_bgmStoppedManually) return;
            if (_bgmSource == null || _bgmSource.isPlaying) return;
            if (bgmClips == null || bgmClips.Length == 0) return;
            PlayNextBgm();
        }

        public void PlayFound()         => PlayOneShot(foundClip);
        public void PlayMiss()          => PlayOneShot(missClip);
        public void PlayLevelComplete() => PlayOneShot(levelCompleteClip);
        public void PlayCompleteScene() => PlayOneShot(completeSceneClip);

        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxPrefKey, sfxVolume);
        }

        public void SetBgmVolume(float value)
        {
            bgmVolume = Mathf.Clamp01(value);
            if (_bgmSource != null) _bgmSource.volume = bgmVolume;
            PlayerPrefs.SetFloat(BgmPrefKey, bgmVolume);

            if (bgmVolume > 0f && _bgmStoppedManually && SceneManager.GetActiveScene().name == GameplaySceneName)
            {
                _bgmStoppedManually = false;
                if (_bgmSource != null && !_bgmSource.isPlaying && bgmClips != null && bgmClips.Length > 0)
                    PlayNextBgm();
            }
        }

        public void StopBgm()
        {
            _bgmStoppedManually = true;
            if (_bgmSource != null && _bgmSource.isPlaying) _bgmSource.Stop();
        }

        private void PlayNextBgm()
        {
            if (bgmClips == null || bgmClips.Length == 0) return;

            int next;
            if (bgmClips.Length == 1) next = 0;
            else
            {
                int safety = 8;
                do { next = Random.Range(0, bgmClips.Length); }
                while (next == _currentBgmIdx && --safety > 0);
            }

            var clip = bgmClips[next];
            if (clip == null) return;

            _currentBgmIdx  = next;
            _bgmSource.clip = clip;
            _bgmSource.Play();
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, sfxVolume);
        }

        // ── 절차적 합성 (외부 자산 의존 없음) ──────────────────

        private static AudioClip GenerateFound()
        {
            const float freq = 880f, dur = 0.15f; const int rate = 44100;
            int n = (int)(rate * dur);
            var samples = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                float env = Mathf.Exp(-3f * t / dur);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.3f;
            }
            var clip = AudioClip.Create("ProcFound", n, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateMiss()
        {
            const float freq = 220f, dur = 0.12f; const int rate = 44100;
            int n = (int)(rate * dur);
            var samples = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                float env = Mathf.Exp(-6f * t / dur);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.2f;
            }
            var clip = AudioClip.Create("ProcMiss", n, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateFanfare()
        {
            const int rate = 44100; const float noteDur = 0.1f;
            float[] freqs = { 523.25f, 659.26f, 783.99f, 1046.50f };
            int samplesPerNote = (int)(rate * noteDur);
            int n = samplesPerNote * freqs.Length;
            var samples = new float[n];
            for (int i = 0; i < n; i++)
            {
                int noteIdx = Mathf.Min(i / samplesPerNote, freqs.Length - 1);
                float t = (float)(i % samplesPerNote) / rate;
                float env = Mathf.Exp(-3f * t / noteDur);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freqs[noteIdx] * t) * env * 0.25f;
            }
            var clip = AudioClip.Create("ProcFanfare", n, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateCompleteSwell()
        {
            const int rate = 44100; const float dur = 1.0f;
            float[] freqs = { 523.25f, 659.26f, 783.99f, 1046.50f };
            int n = (int)(rate * dur);
            var samples = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                float attackEnv = Mathf.Min(1f, t / 0.02f);
                float decayEnv  = Mathf.Exp(-1.5f * t / dur);
                float env = attackEnv * decayEnv;
                float wave = 0f;
                foreach (var f in freqs) wave += Mathf.Sin(2f * Mathf.PI * f * t);
                wave /= freqs.Length;
                samples[i] = wave * env * 0.5f;
            }
            var clip = AudioClip.Create("ProcCompleteSwell", n, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
