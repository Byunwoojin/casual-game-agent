// HintManager 정책 — 일 2회 자정 리셋 (최대 14일치 stockpile) + 무제한 rewarded
using System;
using NUnit.Framework;
using UnityEngine;
using MemoryMatch.Systems;

namespace MemoryMatch.Tests.EditMode
{
    public class HintManagerTests
    {
        private GameObject  _go;
        private HintManager _hm;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteAll();

            _go = new GameObject("HintManager");
            _hm = _go.AddComponent<HintManager>();

            var now = new DateTime(2026, 4, 25, 13, 0, 0);
            _hm.SetTimeProvider(() => now);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_go);
            PlayerPrefs.DeleteAll();
        }

        // TC-HM-001: 힌트 보유 시 UseHint() → OnHintRequested 발행 (재화는 ConsumeHint 가 확정)
        [Test]
        public void TC_HM_001_UseHint_WhenAvailable_RaisesRequestEvent()
        {
            _hm.hintCount = 2;
            bool requested = false;
            _hm.OnHintRequested += () => requested = true;

            _hm.UseHint();

            Assert.IsTrue(requested, "hintCount > 0 이면 OnHintRequested 가 발행돼야 함");
            Assert.AreEqual(2, _hm.hintCount, "UseHint 자체는 차감하지 않음 — ConsumeHint 가 확정");
        }

        // TC-HM-002: 0 일 때 UseHint() → 이벤트 발행 안 함
        [Test]
        public void TC_HM_002_UseHint_WhenZero_NoRequestEvent()
        {
            _hm.hintCount = 0;
            bool requested = false;
            _hm.OnHintRequested += () => requested = true;

            _hm.UseHint();

            Assert.IsFalse(requested, "0개 상태에서는 OnHintRequested 가 발행되면 안 됨 (UI 가 광고로 처리)");
        }

        // TC-HM-003: ConsumeHint() → 1 감소
        [Test]
        public void TC_HM_003_ConsumeHint_Decrements()
        {
            _hm.hintCount = 2;
            _hm.ConsumeHint();
            Assert.AreEqual(1, _hm.hintCount);
        }

        // TC-HM-004: 새 날짜 진입 → +2 무료 힌트 누적
        [Test]
        public void TC_HM_004_NewDay_GrantsTwoFreeHints()
        {
            _hm.hintCount     = 0;
            _hm.lastResetDate = new DateTime(2026, 4, 24); // 어제
            _hm.SetTimeProvider(() => new DateTime(2026, 4, 25, 1, 0, 0)); // 오늘

            _hm.Update();
            Assert.AreEqual(2, _hm.hintCount, "1일 경과 → +2");
        }

        // TC-HM-005: 며칠 누적 (3일 경과 → +6)
        [Test]
        public void TC_HM_005_MultipleDays_AccumulatesHints()
        {
            _hm.hintCount     = 1;
            _hm.lastResetDate = new DateTime(2026, 4, 22); // 3일 전
            _hm.SetTimeProvider(() => new DateTime(2026, 4, 25, 1, 0, 0));

            _hm.Update();
            Assert.AreEqual(1 + 6, _hm.hintCount, "3일 누적 → +6 (기존 1 + 6)");
        }

        // TC-HM-006: 14일 stockpile cap
        [Test]
        public void TC_HM_006_DailyReset_StockpileCap14Days()
        {
            _hm.hintCount     = 0;
            _hm.lastResetDate = new DateTime(2026, 1, 1); // ~115일 전
            _hm.SetTimeProvider(() => new DateTime(2026, 4, 25, 1, 0, 0));

            _hm.Update();
            Assert.AreEqual(HintManager.DailyFreeHints * HintManager.MaxStockpileDays, _hm.hintCount,
                $"최대 {HintManager.MaxStockpileDays}일치 cap (= {HintManager.DailyFreeHints * HintManager.MaxStockpileDays} 개)");
        }

        // TC-HM-007: 같은 날짜 → 변동 없음
        [Test]
        public void TC_HM_007_SameDay_NoChange()
        {
            _hm.hintCount     = 5;
            _hm.lastResetDate = new DateTime(2026, 4, 25);
            _hm.SetTimeProvider(() => new DateTime(2026, 4, 25, 23, 0, 0));

            _hm.Update();
            Assert.AreEqual(5, _hm.hintCount, "같은 날 → 무변동");
        }

        // TC-HM-008: OnRewardedHintEarned → +1 (cap 없음)
        [Test]
        public void TC_HM_008_OnRewardedHintEarned_NoCap()
        {
            _hm.hintCount = 100;
            _hm.OnRewardedHintEarned();
            Assert.AreEqual(101, _hm.hintCount, "rewarded 광고 cap 없음");
        }
    }
}
