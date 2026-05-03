using UnityEngine;
using UnityEngine.SceneManagement;

namespace PuzzlePals.Systems
{
    /// <summary>
    /// 단순 SFX 재생용 싱글톤. 씬에 없으면 자동 생성(클립은 비어 있으므로 무음).
    /// 디자이너가 만든 인스턴스에 클립을 채워 넣으면 그 인스턴스가 사용된다.
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

        [Header("SFX Clips (선택)")]
        [SerializeField] private AudioClip popClip;           // [미사용] 조각 집기 — 호출 자리 제거됨 (2026-04-27)
        [SerializeField] private AudioClip dingClip;          // 50%/75%/edges 축하 토스트
        [SerializeField] private AudioClip pairClip;          // 합체 + 정답 안착 (사용자 결정 2026-05-01)
        [SerializeField] private AudioClip fanfareClip;       // 레벨 클리어 (마지막 piece 안착 시점, Gameplay 씬)
        [SerializeField] private AudioClip levelCompleteClip; // Complete 씬 진입 시 (fanfare 와 차별 톤)

        [Header("BGM (선택) — Assets/Audio/BGM/ 폴더 스캔으로 자동 채움")]
        [SerializeField] private AudioClip[] bgmClips;   // 여러 트랙 셔플 순환 (직전 곡 회피)

        [Header("Mix (PlayerPrefs 와 동기화)")]
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float bgmVolume = 0.7f;

        // SettingsPopupUI 와 공유하는 PlayerPrefs 키
        private const string SfxPrefKey = "Settings_SfxVolume";
        private const string BgmPrefKey = "Settings_BgmVolume";

        // BGM 은 Gameplay 씬에서만 재생. 다른 씬 진입 시 즉시 stop (사용자 결정 2026-05-03).
        private const string GameplaySceneName = "Gameplay";

        private AudioSource _sfxSource;
        private AudioSource _bgmSource;

        private int  _currentBgmIdx       = -1;
        private bool _bgmStoppedManually  = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // PlayerPrefs 에 저장된 값 우선
            sfxVolume = PlayerPrefs.GetFloat(SfxPrefKey, sfxVolume);
            bgmVolume = PlayerPrefs.GetFloat(BgmPrefKey, bgmVolume);

            // SFX 소스 — PlayOneShot 용
            _sfxSource = GetComponent<AudioSource>();
            if (_sfxSource == null) _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.loop        = false;

            // 인스펙터 미할당 클립 절차적 합성 fallback — 외부 자산 의존 없음
            if (dingClip == null)          dingClip          = GenerateDing();
            if (pairClip == null)          pairClip          = GeneratePair();
            if (fanfareClip == null)       fanfareClip       = GenerateFanfare();
            if (levelCompleteClip == null) levelCompleteClip = GenerateLevelComplete();

            // BGM 소스 — 별도 AudioSource. loop = false (한 곡 끝나면 PlayNextBgm 으로 다음 곡 셔플 재생)
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            _bgmSource.loop        = false;
            _bgmSource.volume      = bgmVolume;

            // BGM 시작 정책:
            //   1) 현재 씬이 Gameplay (게임 페이지에서만 BGM 재생)
            //   2) bgmVolume > 0 (사용자 슬라이더 조정 여부 무관 — 첫 실행에서도 자동 재생)
            // bgmClips 가 비어있으면 PlayNextBgm 가 자동으로 no-op 처리.
            bool inGameplay = SceneManager.GetActiveScene().name == GameplaySceneName;
            int  clipCount  = bgmClips != null ? bgmClips.Length : 0;
            Debug.Log($"[AudioManager] Awake — scene={SceneManager.GetActiveScene().name}, " +
                      $"inGameplay={inGameplay}, bgmVolume={bgmVolume:F2}, bgmClips.Length={clipCount}");
            if (inGameplay && bgmVolume > 0f)
            {
                PlayNextBgm();
            }
            else
            {
                _bgmStoppedManually = true;
                Debug.Log("[AudioManager] BGM 자동 재생 skip — Gameplay 외 씬이거나 bgmVolume=0");
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        // 씬 전환 정책: Gameplay 진입 시 BGM 시작 (사용자 정책 충족 시), 그 외 씬 진입 시 즉시 stop.
        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == GameplaySceneName)
            {
                if (bgmVolume > 0f
                    && bgmClips != null && bgmClips.Length > 0
                    && _bgmSource != null && !_bgmSource.isPlaying)
                {
                    _bgmStoppedManually = false;
                    PlayNextBgm();
                }
            }
            else
            {
                // 다른 페이지 진입 — 즉시 stop. Update 의 자동 진행도 차단.
                _bgmStoppedManually = true;
                if (_bgmSource != null && _bgmSource.isPlaying) _bgmSource.Stop();
            }
        }

        // 한 곡이 끝나면 다음 곡으로 자동 진행. SetBgmVolume 0 으로 무음 상태여도 트랙 전환은 계속됨 (의도).
        // StopBgm() 명시 호출 시에는 _bgmStoppedManually 로 자동 진행 차단.
        private void Update()
        {
            if (_bgmStoppedManually) return;
            if (_bgmSource == null || _bgmSource.isPlaying) return;
            if (bgmClips == null || bgmClips.Length == 0) return;
            PlayNextBgm();
        }

        public void PlayPop()           => PlayOneShot(popClip);
        public void PlayDing()          => PlayOneShot(dingClip);
        public void PlayPair()          => PlayOneShot(pairClip);
        public void PlayFanfare()       => PlayOneShot(fanfareClip);
        public void PlayLevelComplete() => PlayOneShot(levelCompleteClip);

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

            // 무음 정책 상태 (Awake 가 자동 재생 skip 했거나, 사용자가 0 으로 내려서 멈춤) 에서
            // 슬라이더로 0 이상 올리면 BGM 즉시 시작 — 단 Gameplay 씬에서만.
            // 다른 페이지에서 슬라이더 조정 시엔 PlayerPrefs 만 저장, 다음 Gameplay 진입 시점에 시작.
            if (bgmVolume > 0f && _bgmStoppedManually
                && SceneManager.GetActiveScene().name == GameplaySceneName)
            {
                _bgmStoppedManually = false;
                if (_bgmSource != null && !_bgmSource.isPlaying
                    && bgmClips != null && bgmClips.Length > 0)
                {
                    PlayNextBgm();
                }
            }
        }

        public void PlayBgm()
        {
            _bgmStoppedManually = false;
            if (_bgmSource == null) return;
            if (_bgmSource.clip == null) { PlayNextBgm(); return; }
            if (!_bgmSource.isPlaying) _bgmSource.Play();
        }

        public void StopBgm()
        {
            _bgmStoppedManually = true;
            if (_bgmSource != null && _bgmSource.isPlaying) _bgmSource.Stop();
        }

        // 다음 트랙을 셔플 (직전 곡 회피) 로 선택해 즉시 재생.
        // bgmClips 가 1곡이면 그대로 그 곡 반복. 비어있으면 no-op.
        private void PlayNextBgm()
        {
            if (bgmClips == null || bgmClips.Length == 0)
            {
                Debug.LogWarning("[AudioManager] PlayNextBgm 호출됐지만 bgmClips 비어있음 — " +
                                 "Gameplay 씬 AudioManager 인스펙터의 bgmClips 배열에 AudioClip 자산을 할당하세요.");
                return;
            }

            int next;
            if (bgmClips.Length == 1)
            {
                next = 0;
            }
            else
            {
                // 직전 곡 회피 — 같은 곡이 연속으로 재생되지 않도록
                int safety = 8;
                do { next = Random.Range(0, bgmClips.Length); }
                while (next == _currentBgmIdx && --safety > 0);
            }

            var clip = bgmClips[next];
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] bgmClips[{next}] 가 null — 인스펙터에서 빈 슬롯 확인");
                return; // 빈 슬롯 보호
            }

            _currentBgmIdx = next;
            _bgmSource.clip = clip;
            _bgmSource.Play();
            Debug.Log($"[AudioManager] BGM 재생 시작 — clip={clip.name}, length={clip.length:F1}s, " +
                      $"volume={_bgmSource.volume:F2}, isPlaying={_bgmSource.isPlaying}");
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, sfxVolume);
        }

        /// <summary>
        /// 절차적 ding 사운드 — 사인파 880Hz (A5) + exponential envelope decay 0.15초.
        /// dingClip 인스펙터 미할당 시 fallback. 외부 자산 의존 없음.
        /// </summary>
        private static AudioClip GenerateDing()
        {
            const float freq = 880f;   // A5 음
            const float dur  = 0.15f;
            const int   rate = 44100;
            int n = (int)(rate * dur);
            float[] samples = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t   = (float)i / rate;
                float env = Mathf.Exp(-3f * t / dur);              // 빠른 decay
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.3f;
            }
            var clip = AudioClip.Create("ProcDing", n, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// 절차적 level complete (Complete 씬 진입) 사운드 — major chord swell.
        /// C major chord (C4-E4-G4-C5 동시) + 부드러운 attack 0.1초 + slow sustain 1초.
        /// fanfare arpeggio 와 차별 — 풍부한 화음으로 도착감 + 성취 인정.
        /// </summary>
        private static AudioClip GenerateLevelComplete()
        {
            const int   rate = 44100;
            const float dur  = 1.0f;
            // 한 옥타브 올림 (이전 C4-C5 너무 낮아 모바일 audio 에서 약함)
            float[] freqs   = { 523.25f, 659.26f, 783.99f, 1046.50f }; // C5, E5, G5, C6
            int n           = (int)(rate * dur);
            float[] samples = new float[n];

            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                // 짧은 attack 0.02초 (이전 0.1초는 시작에 silent → ramp up 으로 끊김 느낌) + slow decay (sustain)
                float attackEnv = Mathf.Min(1f, t / 0.02f);
                float decayEnv  = Mathf.Exp(-1.5f * t / dur);
                float env       = attackEnv * decayEnv;
                float wave = 0f;
                for (int j = 0; j < freqs.Length; j++)
                    wave += Mathf.Sin(2f * Mathf.PI * freqs[j] * t);
                wave /= freqs.Length;
                samples[i] = wave * env * 0.5f;
            }

            var clip = AudioClip.Create("ProcLevelComplete", n, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// 절차적 fanfare (레벨 클리어) 사운드 — C major triad arpeggio (C5-E5-G5-C6).
        /// 각 노트 0.1초 + 빠른 decay. 4 노트 총 0.4초 정도.
        /// fanfareClip 인스펙터 미할당 시 fallback.
        /// </summary>
        private static AudioClip GenerateFanfare()
        {
            const int   rate     = 44100;
            const float noteDur  = 0.1f;
            float[]     freqs    = { 523.25f, 659.26f, 783.99f, 1046.50f }; // C5, E5, G5, C6
            int notesCount       = freqs.Length;
            int samplesPerNote   = (int)(rate * noteDur);
            int n                = samplesPerNote * notesCount;
            float[] samples      = new float[n];

            for (int i = 0; i < n; i++)
            {
                int noteIdx = Mathf.Min(i / samplesPerNote, notesCount - 1);
                float t     = (float)(i % samplesPerNote) / rate;
                float env   = Mathf.Exp(-3f * t / noteDur);
                samples[i]  = Mathf.Sin(2f * Mathf.PI * freqs[noteIdx] * t) * env * 0.25f;
            }

            var clip = AudioClip.Create("ProcFanfare", n, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// 절차적 pair (합체) 사운드 — 짧은 click 느낌. 두 piece 가 끼워지는 만족감.
        /// 800Hz + 1200Hz 화음 (5도) + 빠른 decay 0.06초.
        /// </summary>
        private static AudioClip GeneratePair()
        {
            const float f1   = 800f;
            const float f2   = 1200f;  // 800 × 1.5 = 5도 위
            const float dur  = 0.06f;
            const int   rate = 44100;
            int n = (int)(rate * dur);
            float[] samples = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t    = (float)i / rate;
                float env  = Mathf.Exp(-10f * t / dur);  // 빠른 decay (click 느낌)
                float wave = (Mathf.Sin(2f * Mathf.PI * f1 * t)
                            + Mathf.Sin(2f * Mathf.PI * f2 * t)) * 0.5f;
                samples[i] = wave * env * 0.25f;
            }
            var clip = AudioClip.Create("ProcPair", n, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
