using System;
using System.Collections.Generic;
using UnityEngine;
using PuzzlePals.Puzzle;

namespace PuzzlePals.Systems
{
    /// <summary>
    /// 힌트 관리자 — 성인 피벗 v0.4.
    ///
    /// 정책:
    /// - 일 2회 무료 (자정 리셋 — 새 날짜 진입 시 +2 누적, 최대 14일치 stockpile)
    /// - 광고 시청 시 +1 (무제한 — 일일 cap 없음)
    /// - IAP 구매분은 별도 누적 (cap 무시)
    /// - 사용 시 1 차감 + 모든 미점유 SnapZone Flash
    ///
    /// 싱글톤 + DontDestroyOnLoad — 씬 전환과 무관하게 hintCount/lastResetDate 보존.
    /// 테스트에서는 SetTimeProvider 로 시계 주입 (DateTime.Now 비결정성 회피).
    /// </summary>
    public class HintManager : MonoBehaviour
    {
        // ── 상수 ─────────────────────────────────────────────
        public const int DailyFreeHints  = 2;
        public const int MaxStockpileDays = 14; // 며칠 안 들어왔어도 최대 28개까지만 누적

        // ── 상태 (public — 테스트/IAP 가 직접 설정) ──────────
        public int      hintCount;
        public DateTime lastResetDate; // Date 부분만 의미

        /// <summary>hintCount 변경 시 발화 — UI(툴바 배지) 갱신용.</summary>
        public event Action<int> OnHintCountChanged;

        /// <summary>외부에서 hintCount 직접 변경 시 호출 (테스트/IAP). 내부 변경은 자동 발화.</summary>
        public void NotifyHintCountChanged() => OnHintCountChanged?.Invoke(hintCount);

        // ── 의존성 주입 ───────────────────────────────────────
        // (성인 피벗 v0.4: IUIManager / ShopPopup 폐기. 광고 진입은 GameplayHeaderUI 가 직접 처리)

        // 시간 주입 (테스트)
        private Func<DateTime> _getNow = () => DateTime.Now;
        public void SetTimeProvider(Func<DateTime> getNow) => _getNow = getNow;

        // ── 싱글톤 ───────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────
        // Unity 생명주기
        // ─────────────────────────────────────────────────────

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            hintCount     = SaveManager.LoadHintCount();
            lastResetDate = SaveManager.LoadHintLastResetDate();

            // 첫 실행 (lastResetDate == MinValue) 이거나 새 날짜면 무료분 지급
            EnsureDailyReset(_getNow());
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void Update()
        {
            EnsureDailyReset(_getNow());
        }

        // ─────────────────────────────────────────────────────
        // 공개 API
        // ─────────────────────────────────────────────────────

        public void UseHint()
        {
            if (hintCount > 0)
            {
                hintCount--;
                SaveManager.SaveHintCount(hintCount);
                NotifyHintCountChanged();

                // 미스냅 조각 중 1개 랜덤 → AssignedSnapZone 위치로 자동 이동 + 스냅
                var pieces = FindObjectsByType<PuzzlePiece>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                var candidates = new List<PuzzlePiece>();
                for (int i = 0; i < pieces.Length; i++)
                {
                    var p = pieces[i];
                    if (p == null || p.IsSnapped) continue;
                    if (p.AssignedSnapZone == null || p.AssignedSnapZone.IsOccupied) continue;
                    candidates.Add(p);
                }

                if (candidates.Count > 0)
                {
                    var pick = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                    pick.AutoSnap();
                }
            }
            else
            {
                // 성인 피벗 v0.4: ShopPopup 폐기. 0 개 상태에서의 광고 진입은 GameplayHeaderUI.OnHint 가 처리.
                Debug.Log("[HintManager] hintCount == 0 — no-op (툴바 힌트 버튼이 광고로 +1 처리)");
            }
        }

        /// <summary>리워디드 광고 시청 보상 — 힌트 +1 (cap 없음).</summary>
        public void OnRewardedHintEarned()
        {
            hintCount++;
            SaveManager.SaveHintCount(hintCount);
            NotifyHintCountChanged();
        }

        /// <summary>IAP 구매분 누적.</summary>
        public void AddPurchasedHints(int amount)
        {
            hintCount += amount;
            SaveManager.SaveHintCount(hintCount);
            NotifyHintCountChanged();
        }

        /// <summary>무제한 IAP 상품 — 큰 수로 채워 사실상 무한 사용.</summary>
        public void SetUnlimited()
        {
            hintCount = 9999;
            SaveManager.SaveHintCount(hintCount);
            NotifyHintCountChanged();
        }

        // ─────────────────────────────────────────────────────
        // 일일 리셋 로직 — 자정 진입 시 무료 힌트 +DailyFreeHints
        // ─────────────────────────────────────────────────────

        private void EnsureDailyReset(DateTime now)
        {
            DateTime today = now.Date;

            // 첫 실행 (lastResetDate == MinValue) → 오늘 첫 무료분 1세트
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

            // 며칠 동안 쌓였는지 (최대 stockpile 일수까지만)
            int daysPassed = Mathf.Min(MaxStockpileDays, (int)(today - lastResetDate.Date).TotalDays);
            hintCount     += DailyFreeHints * daysPassed;
            lastResetDate = today;
            SaveManager.SaveHintCount(hintCount);
            SaveManager.SaveHintLastResetDate(lastResetDate);
            NotifyHintCountChanged();
        }
    }
}
