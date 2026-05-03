// AdManager 광고 트리거 — 성인 피벗 v0.4
// L1 무광고 / L2+ 시작·완료 인터스티셜 / 무제한 rewarded
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PuzzlePals.Systems;
using PuzzlePals.Interfaces;

namespace PuzzlePals.Tests.PlayMode
{
    public class AdTriggerTests
    {
        private GameObject     _adGO;
        private AdManager      _adManager;
        private MockAdProvider _mockAd;

        // ── 인라인 Mock ──────────────────────────────────────
        private class MockAdProvider : IAdProvider
        {
            public bool   ShowCalled         { get; private set; }
            public string LastPlacementId    { get; private set; }
            public int    ShowCallCount      { get; private set; }
            public int    RewardedCallCount  { get; private set; }
            public bool   RewardedCallbackInvoked { get; private set; }

            public void Show(string placementId)
            {
                ShowCalled      = true;
                LastPlacementId = placementId;
                ShowCallCount++;
            }

            public void ShowRewarded(string placementId, System.Action onRewarded)
            {
                RewardedCallCount++;
                LastPlacementId = placementId;
                onRewarded?.Invoke();
                RewardedCallbackInvoked = true;
            }

            public void ShowRewarded(string placementId, System.Action onRewarded, System.Action onFailed)
                => ShowRewarded(placementId, onRewarded);

            public int  BannerShowCount { get; private set; }
            public int  BannerHideCount { get; private set; }
            public void ShowBanner() => BannerShowCount++;
            public void HideBanner() => BannerHideCount++;
        }
        // ─────────────────────────────────────────────────────

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _adGO      = new GameObject("AdManager");
            _adManager = _adGO.AddComponent<AdManager>();
            _mockAd    = new MockAdProvider();
            _adManager.AdProvider = _mockAd;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(_adGO);
            yield return null;
        }

        // TC-AD-001: L1 시작 → 광고 X (무광고 정책)
        [UnityTest]
        public IEnumerator TC_AD_001_OnLevelStart_L1_SkipsAd()
        {
            _adManager.OnLevelStart(1);
            yield return null;

            Assert.IsFalse(_mockAd.ShowCalled, "L1 시작은 무광고");
        }

        // TC-AD-002: L2 시작 → 인터스티셜 표시
        [UnityTest]
        public IEnumerator TC_AD_002_OnLevelStart_L2_ShowsInterstitial()
        {
            _adManager.OnLevelStart(2);
            yield return null;

            Assert.IsTrue(_mockAd.ShowCalled, "L2 이상 시작 시 광고 표시");
            Assert.AreEqual("Interstitial_Android", _mockAd.LastPlacementId);
        }

        // TC-AD-003: L1 완료 → 광고 X
        [UnityTest]
        public IEnumerator TC_AD_003_OnLevelComplete_L1_SkipsAd()
        {
            _adManager.OnLevelComplete(1);
            yield return null;

            Assert.IsFalse(_mockAd.ShowCalled, "L1 완료는 무광고");
        }

        // TC-AD-004: L2 완료 → 인터스티셜 표시
        [UnityTest]
        public IEnumerator TC_AD_004_OnLevelComplete_L2_ShowsInterstitial()
        {
            _adManager.OnLevelComplete(2);
            yield return null;

            Assert.IsTrue(_mockAd.ShowCalled, "L2 이상 완료 시 광고 표시");
            Assert.AreEqual("Interstitial_Android", _mockAd.LastPlacementId);
        }

        // TC-AD-005: ShowRewarded → 콜백 invoke (힌트 +1 수여)
        [UnityTest]
        public IEnumerator TC_AD_005_ShowRewarded_InvokesCallback()
        {
            bool callbackFired = false;
            _adManager.AdProvider.ShowRewarded("Rewarded_Android", () => callbackFired = true);
            yield return null;

            Assert.IsTrue(callbackFired, "Rewarded 광고 보상 콜백이 invoke 되어야 함");
            Assert.AreEqual(1, _mockAd.RewardedCallCount);
            Assert.AreEqual("Rewarded_Android", _mockAd.LastPlacementId);
        }

        // TC-AD-006 [Legacy]: parameterless OnLevelComplete() 가 매 5라운드 광고 (구 v0.3 호환)
        [UnityTest]
        public IEnumerator TC_AD_006_LegacyOnLevelComplete_5thCallShowsAd()
        {
            _adManager.RoundsSinceAd = 4;
            _adManager.OnLevelComplete(); // 5번째 호출
            yield return null;

            Assert.IsTrue(_mockAd.ShowCalled, "구 5-rounds 카운터 분기 보존");
            Assert.AreEqual(0, _adManager.RoundsSinceAd, "광고 후 카운터 리셋");
        }
    }
}
