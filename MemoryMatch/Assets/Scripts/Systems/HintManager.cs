using System;
using UnityEngine;

namespace MemoryMatch.Systems
{
    /// <summary>
    /// 힌트 재화 관리자 — 장르 독립적 (Core Spec).
    ///
    /// 정책 (Puzzle Pals 와 동일 — 일 2회 무료 자정 리셋, 최대 14일치 stockpile, 광고 시청 무제한):
    /// - 일 2회 무료 (새 날짜 진입 시 +2 누적, 최대 14일치)
    /// - 광고 시청 시 +1 (cap 없음)
    ///
    /// 장르별로 "힌트가 실제로 뭘 하는지"는 이 클래스가 모름 — <see cref="OnHintRequested"/> 이벤트만 발행.
    /// 구독자(게임플레이 컨트롤러)가 실제 처리 후 성공하면 <see cref="ConsumeHint"/> 를 호출해야 차감됨.
    /// (Puzzle Pals 의 HintManager 는 UseHint() 안에서 FindObjectsByType&lt;PuzzlePiece&gt; 를 직접 호출해
    ///  재화 관리와 장르별 힌트 연출이 한 메서드에 결합돼 있었음 — 이 프로젝트에서는 분리해서 시작.)
    /// </summary>
    public class HintManager : MonoBehaviour
    {
        public const int DailyFreeHints   = 2;
        public const int MaxStockpileDays = 14;

        public int      hintCount;
        public DateTime lastResetDate;

        public event Action<int> OnHintCountChanged;

        /// <summary>힌트 사용 요청 — 구독자가 실제 처리(예: 미발견 diff 하나 반짝임) 후 성공 시 ConsumeHint 호출.</summary>
        public event Action OnHintRequested;

        public void NotifyHintCountChanged() => OnHintCountChanged?.Invoke(hintCount);

        private Func<DateTime> _getNow = () => DateTime.Now;
        public void SetTimeProvider(Func<DateTime> getNow) => _getNow = getNow;

        private static HintManager _instance;
        public static HintManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                if (!Application.isPlaying) return null;

                var existing = FindAnyObjectByType<HintManager>(FindObjectsInactive.Include);
                if (existing != null) { _instance = existing; return _instance; }

                var go = new GameObject(nameof(HintManager));
                _instance = go.AddComponent<HintManager>();
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            hintCount     = SaveManager.LoadHintCount();
            lastResetDate = SaveManager.LoadHintLastResetDate();
            EnsureDailyReset(_getNow());
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void Update() => EnsureDailyReset(_getNow());

        /// <summary>힌트 보유 시 OnHintRequested 발행만 함 — 차감은 구독자가 ConsumeHint() 로 확정.</summary>
        public void UseHint()
        {
            if (hintCount <= 0)
            {
                Debug.Log("[HintManager] hintCount == 0 — no-op (0개 상태에서 광고 진입은 UI 가 처리)");
                return;
            }
            OnHintRequested?.Invoke();
        }

        /// <summary>구독자가 힌트를 실제로 소모했을 때만 호출 — 1 차감.</summary>
        public void ConsumeHint()
        {
            if (hintCount <= 0) return;
            hintCount--;
            SaveManager.SaveHintCount(hintCount);
            NotifyHintCountChanged();
        }

        public void OnRewardedHintEarned()
        {
            hintCount++;
            SaveManager.SaveHintCount(hintCount);
            NotifyHintCountChanged();
        }

        public void AddPurchasedHints(int amount)
        {
            hintCount += amount;
            SaveManager.SaveHintCount(hintCount);
            NotifyHintCountChanged();
        }

        private void EnsureDailyReset(DateTime now)
        {
            DateTime today = now.Date;

            if (lastResetDate == DateTime.MinValue)
            {
                hintCount     += DailyFreeHints;
                lastResetDate = today;
                SaveManager.SaveHintCount(hintCount);
                SaveManager.SaveHintLastResetDate(lastResetDate);
                NotifyHintCountChanged();
                return;
            }

            if (today <= lastResetDate.Date) return;

            int daysPassed = Mathf.Min(MaxStockpileDays, (int)(today - lastResetDate.Date).TotalDays);
            hintCount     += DailyFreeHints * daysPassed;
            lastResetDate = today;
            SaveManager.SaveHintCount(hintCount);
            SaveManager.SaveHintLastResetDate(lastResetDate);
            NotifyHintCountChanged();
        }
    }
}
