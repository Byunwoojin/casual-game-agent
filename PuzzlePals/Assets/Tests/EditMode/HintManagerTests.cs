// HintManager 정책 — 성인 피벗 v0.4
// 일 2회 자정 리셋 (최대 14일치 stockpile) + 무제한 rewarded 광고
using System;
using NUnit.Framework;
using UnityEngine;
using PuzzlePals.Systems;

namespace PuzzlePals.Tests.EditMode
{
    public class HintManagerTests
    {
        private GameObject  _go;
        private HintManager _hm;

        [SetUp]
        public void SetUp()
        {
            // PlayerPrefs 격리 — Awake 의 EnsureDailyReset 영향 차단
            PlayerPrefs.DeleteAll();

            _go = new GameObject("HintManager");
            _hm = _go.AddComponent<HintManager>();

            // 기준 시간 고정 — 2026-04-25 13:00
            var now = new DateTime(2026, 4, 25, 13, 0, 0);
            _hm.SetTimeProvider(() => now);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_go);
            PlayerPrefs.DeleteAll();
        }

        // TC-HM-001: 힌트 보유 시 UseHint() → 1 감소
        [Test]
        public void TC_HM_001_UseHint_WhenAvailable_DecrementsCount()
        {
            _hm.hintCount = 2;
            _hm.UseHint();
            Assert.AreEqual(1, _hm.hintCount);
        }

        // TC-HM-002: 0 일 때 UseHint() → no-op (광고 진입은 GameplayHeaderUI 가 별도 처리)
        [Test]
        public void TC_HM_002_UseHint_WhenZero_NoOp()
        {
            _hm.hintCount = 0;
            _hm.UseHint();
            Assert.AreEqual(0, _hm.hintCount, "0개 상태에서 UseHint 는 카운트 변동 없음");
        }

        // TC-HM-003: 새 날짜 진입 → +2 무료 힌트 누적
        [Test]
        public void TC_HM_003_NewDay_GrantsTwoFreeHints()
        {
            _hm.hintCount     = 0;
            _hm.lastResetDate = new DateTime(2026, 4, 24); // 어제
            _hm.SetTimeProvider(() => new DateTime(2026, 4, 25, 1, 0, 0)); // 오늘

            _hm.Update();
            Assert.AreEqual(2, _hm.hintCount, "1일 경과 → +2");
        }

        // TC-HM-004: 며칠 누적 (3일 경과 → +6)
        [Test]
        public void TC_HM_004_MultipleDays_AccumulatesHints()
        {
            _hm.hintCount     = 1;
            _hm.lastResetDate = new DateTime(2026, 4, 22); // 3일 전
            _hm.SetTimeProvider(() => new DateTime(2026, 4, 25, 1, 0, 0));

            _hm.Update();
            Assert.AreEqual(1 + 6, _hm.hintCount, "3일 누적 → +6 (기존 1 + 6)");
        }

        // TC-HM-005: 14일 stockpile cap (예: 100일 미접속 → 28개만)
        [Test]
        public void TC_HM_005_DailyReset_StockpileCap14Days()
        {
            _hm.hintCount     = 0;
            _hm.lastResetDate = new DateTime(2026, 1, 1); // ~115일 전
            _hm.SetTimeProvider(() => new DateTime(2026, 4, 25, 1, 0, 0));

            _hm.Update();
            Assert.AreEqual(HintManager.DailyFreeHints * HintManager.MaxStockpileDays, _hm.hintCount,
                $"최대 {HintManager.MaxStockpileDays}일치 cap (= {HintManager.DailyFreeHints * HintManager.MaxStockpileDays} 개)");
        }

        // TC-HM-006: 같은 날짜 → 변동 없음
        [Test]
        public void TC_HM_006_SameDay_NoChange()
        {
            _hm.hintCount     = 5;
            _hm.lastResetDate = new DateTime(2026, 4, 25);
            _hm.SetTimeProvider(() => new DateTime(2026, 4, 25, 23, 0, 0));

            _hm.Update();
            Assert.AreEqual(5, _hm.hintCount, "같은 날 → 무변동");
        }

        // TC-HM-007: OnRewardedHintEarned → +1 (cap 없음)
        [Test]
        public void TC_HM_007_OnRewardedHintEarned_IncrementsByOne()
        {
            _hm.hintCount = 0;
            _hm.OnRewardedHintEarned();
            Assert.AreEqual(1, _hm.hintCount);

            _hm.OnRewardedHintEarned();
            Assert.AreEqual(2, _hm.hintCount);
        }

        // TC-HM-008: rewarded 는 무제한 — 100 보유 상태에서도 +1
        [Test]
        public void TC_HM_008_OnRewardedHintEarned_NoCap()
        {
            _hm.hintCount = 100;
            _hm.OnRewardedHintEarned();
            Assert.AreEqual(101, _hm.hintCount, "rewarded 광고 cap 없음 (구 v0.3 의 일일 광고 cap 폐기)");
        }
    }
}
