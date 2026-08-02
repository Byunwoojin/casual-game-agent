// LevelPlay (IronSource) 8.x 인터스티셜 광고 프로바이더
// 패키지: com.unity.services.levelplay (>= 8.0)
//
// 8.x API namespace 구조:
//   - com.unity3d.mediation: LevelPlay (Init/이벤트), LevelPlayAdFormat, LevelPlayConfiguration, LevelPlayInitError
//   - (root namespace): IronSource (Agent.loadInterstitial 등), IronSourceInterstitialEvents (정적 이벤트),
//                       IronSourceAdInfo, IronSourceError
//
// LEVELPLAY_AVAILABLE 미정의 시(패키지 미설치) 로그 스텁으로 동작.
// versionDefines 는 SlidingPuzzle.asmdef 에서 com.unity.services.levelplay >= 8.0.0 일 때 자동 정의.
//
// App Key 발급:
//   1) https://platform.ironsrc.com 로그인 (Unity 계정 연동 가능)
//   2) New App → Android / iOS 앱 등록
//   3) App Key 복사 → AdManager 인스펙터 "Level Play App Key" 필드에 붙여넣기
//   4) Networks 탭 → Unity Ads 추가 (Game ID: Android=6092073, iOS=6092072)

using System;
using UnityEngine;
using SlidingPuzzle.Interfaces;

#if LEVELPLAY_AVAILABLE
using com.unity3d.mediation;
#endif

namespace SlidingPuzzle.Systems
{
    /// <summary>
    /// LevelPlay SDK 8.x 기반 인터스티셜 광고 프로바이더.
    /// Lifecycle:
    ///   1) 생성자 → LevelPlay.Init(appKey, …) 호출, OnInitSuccess 대기
    ///   2) Init 성공 → IronSource.Agent.loadInterstitial() (프리로드)
    ///   3) Show() 호출 시 ready 면 즉시, 아니면 ready 후 자동 표시
    ///   4) 광고 닫힘/표시 실패 → 다음 광고 자동 프리로드
    /// </summary>
    public class LevelPlayAdProvider : IAdProvider
    {
#if LEVELPLAY_AVAILABLE
        private bool   _initialized;
        private bool   _interstitialReady;
        private bool   _pendingShow;
        private string _pendingPlacement;
        private bool   _eventsHooked;

        // Rewarded 광고 상태 (Step 5)
        private bool   _rewardedReady;
        private bool   _rewardedShowing; // 표시 중 flag — onAvailable 재발화로 인한 중복 표시 방지
        private string _pendingRewardedPlacement;
        private Action _pendingRewardedCallback;
        private Action _pendingRewardedFailCallback;

        // Banner 광고 상태 (LevelPlay 8.x 새 API — LevelPlayBannerAd)
        private LevelPlayBannerAd _bannerAd;
        private readonly string   _bannerAdUnitId;
        private bool _bannerLoaded;  // SDK 에 로드된 상태
        private bool _bannerShown;   // 사용자 의도 — 표시되어야 함

        public LevelPlayAdProvider(string appKey, bool testMode, string bannerAdUnitId = null)
        {
            _bannerAdUnitId = bannerAdUnitId;

            if (string.IsNullOrEmpty(appKey) || appKey == "YOUR_LEVELPLAY_APP_KEY")
            {
                Debug.LogError("[LevelPlay] App Key 가 설정되지 않았습니다.\n" +
                               "platform.ironsrc.com 에서 앱 등록 후 App Key 를 AdManager 인스펙터에 입력하세요.");
                return;
            }

            // 테스트 모드: IronSource 메타데이터로 테스트 광고 활성화
            if (testMode)
                IronSource.Agent.setMetaData("is_test_suite", "enable");

            // SDK 초기화 이벤트 구독
            LevelPlay.OnInitSuccess += HandleInitSuccess;
            LevelPlay.OnInitFailed  += HandleInitFailed;

            // 인터스티셜 + 리워디드 + 배너 포맷 초기화
            LevelPlay.Init(appKey, userId: null, adFormats: new[]
            {
                LevelPlayAdFormat.INTERSTITIAL,
                LevelPlayAdFormat.REWARDED,
                LevelPlayAdFormat.BANNER,
            });

            string preview = appKey.Length >= 4 ? appKey.Substring(0, 4) : appKey;
            Debug.Log($"[LevelPlay] Init 호출 — appKey={preview}**** testMode={testMode}");
        }

        private void HandleInitSuccess(LevelPlayConfiguration config)
        {
            _initialized = true;
            Debug.Log("[LevelPlay] Init 성공 — 인터스티셜 + 리워디드 프리로드 시작");

            HookInterstitialEvents();
            HookRewardedEvents();
            IronSource.Agent.loadInterstitial();
            IronSource.Agent.loadRewardedVideo();

            // Banner 광고 — LevelPlay 8.x 새 API. ad unit ID 가 있으면 즉시 인스턴스 생성.
            // Init 전에 ShowBanner() 호출됐으면 LoadAd → displayOnLoad=true 로 자동 표시.
            CreateBannerAd();
            if (_bannerShown && _bannerAd != null && !_bannerLoaded)
            {
                Debug.Log("[LevelPlay] Init 후 pending 배너 로드");
                _bannerAd.LoadAd();
            }
        }

        private void HandleInitFailed(LevelPlayInitError error)
        {
            Debug.LogError($"[LevelPlay] Init 실패: code={error.ErrorCode} — {error.ErrorMessage}");
        }

        private void HookInterstitialEvents()
        {
            if (_eventsHooked) return;
            _eventsHooked = true;

            IronSourceInterstitialEvents.onAdReadyEvent      += HandleAdReady;
            IronSourceInterstitialEvents.onAdLoadFailedEvent += HandleAdLoadFailed;
            IronSourceInterstitialEvents.onAdShowFailedEvent += HandleAdShowFailed;
            IronSourceInterstitialEvents.onAdClosedEvent     += HandleAdClosed;
        }

        private void HandleAdReady(IronSourceAdInfo info)
        {
            _interstitialReady = true;
            Debug.Log("[LevelPlay] 인터스티셜 로드 완료");
            TryShowPending();
        }

        private void HandleAdLoadFailed(IronSourceError err)
        {
            _interstitialReady = false;
            Debug.LogWarning($"[LevelPlay] 인터스티셜 로드 실패: code={err.getErrorCode()} — {err.getDescription()}");
        }

        private void HandleAdShowFailed(IronSourceError err, IronSourceAdInfo info)
        {
            Debug.LogWarning($"[LevelPlay] 인터스티셜 표시 실패: code={err.getErrorCode()} — {err.getDescription()} → 다음 광고 프리로드");
            _interstitialReady = false;
            IronSource.Agent.loadInterstitial();
        }

        private void HandleAdClosed(IronSourceAdInfo info)
        {
            Debug.Log("[LevelPlay] 인터스티셜 닫힘 — 다음 광고 프리로드");
            _interstitialReady = false;
            IronSource.Agent.loadInterstitial();
        }

        private void TryShowPending()
        {
            if (!_pendingShow || !_interstitialReady) return;
            _pendingShow = false;
            DoShow(_pendingPlacement);
        }

        private void DoShow(string placement)
        {
            if (IronSource.Agent.isInterstitialReady())
            {
                if (string.IsNullOrEmpty(placement)) IronSource.Agent.showInterstitial();
                else                                  IronSource.Agent.showInterstitial(placement);
            }
            else
            {
                Debug.Log("[LevelPlay] 광고 준비 안 됨 — 재로드 트리거 (다음 기회에 표시)");
                IronSource.Agent.loadInterstitial();
            }
        }

        /// <summary>AdManager.OnLevelStart/Complete 에서 호출</summary>
        public void Show(string placementId)
        {
            if (!_initialized)
            {
                _pendingShow      = true;
                _pendingPlacement = placementId;
                Debug.Log($"[LevelPlay] 초기화 대기 중 — 완료 후 광고 예약 (placement={placementId})");
                return;
            }
            DoShow(placementId);
        }

        // ─────────────────────────────────────────────────────
        // Rewarded 광고 (Step 5 — 힌트용)
        // ─────────────────────────────────────────────────────

        private void HookRewardedEvents()
        {
            IronSourceRewardedVideoEvents.onAdAvailableEvent   += HandleRewardedAvailable;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += HandleRewardedUnavailable;
            IronSourceRewardedVideoEvents.onAdLoadFailedEvent  += HandleRewardedLoadFailed;
            IronSourceRewardedVideoEvents.onAdRewardedEvent    += HandleRewardedReward;
            IronSourceRewardedVideoEvents.onAdClosedEvent      += HandleRewardedClosed;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent  += HandleRewardedShowFailed;
        }

        private void HandleRewardedAvailable(IronSourceAdInfo info)
        {
            _rewardedReady = true;
            Debug.Log("[LevelPlay] 리워디드 사용 가능");
            TryShowPendingRewarded();
        }

        private void HandleRewardedUnavailable()
        {
            _rewardedReady = false;
            Debug.Log("[LevelPlay] 리워디드 사용 불가 — 재로드 트리거");
            IronSource.Agent.loadRewardedVideo();
        }

        private void HandleRewardedLoadFailed(IronSourceError err)
        {
            _rewardedReady = false;
            Debug.LogWarning($"[LevelPlay] 리워디드 로드 실패: code={err.getErrorCode()} — {err.getDescription()}");
            InvokePendingRewardedFail();
        }

        private void InvokePendingRewardedFail()
        {
            var fail = _pendingRewardedFailCallback;
            _pendingRewardedCallback     = null;
            _pendingRewardedFailCallback = null;
            fail?.Invoke();
        }

        private void HandleRewardedReward(IronSourcePlacement placement, IronSourceAdInfo info)
        {
            // 보상 지급 시점 — 사용자 콜백 호출
            Debug.Log($"[LevelPlay] 리워디드 보상 지급 — placement={placement?.getPlacementName()}, " +
                      $"reward={placement?.getRewardName()} ×{placement?.getRewardAmount()}");
            var cb = _pendingRewardedCallback;
            _pendingRewardedCallback     = null;
            _pendingRewardedFailCallback = null; // 보상 받았으니 fail 콜백 폐기
            cb?.Invoke();
        }

        private void HandleRewardedClosed(IronSourceAdInfo info)
        {
            Debug.Log("[LevelPlay] 리워디드 닫힘 — 다음 광고 프리로드");
            _rewardedReady   = false;
            _rewardedShowing = false;
            // 보상 안 받고 닫혔어도 사용자 의도적 스킵으로 간주 — fail 토스트 띄우지 않음.
            _pendingRewardedCallback     = null;
            _pendingRewardedFailCallback = null;
            IronSource.Agent.loadRewardedVideo();
        }

        private void HandleRewardedShowFailed(IronSourceError err, IronSourceAdInfo info)
        {
            Debug.LogWarning($"[LevelPlay] 리워디드 표시 실패: code={err.getErrorCode()} — {err.getDescription()}");
            _rewardedReady   = false;
            _rewardedShowing = false;
            InvokePendingRewardedFail();
            IronSource.Agent.loadRewardedVideo();
        }

        private void TryShowPendingRewarded()
        {
            if (_pendingRewardedCallback == null) return;
            if (!_rewardedReady) return;
            if (_rewardedShowing) return; // 이미 표시 중이면 onAvailable 재발화에 의한 자동 재호출 무시
            DoShowRewarded(_pendingRewardedPlacement);
        }

        private void DoShowRewarded(string placement)
        {
            if (_rewardedShowing)
            {
                Debug.Log("[LevelPlay] 리워디드 표시 중 — 중복 호출 무시");
                return;
            }
            if (IronSource.Agent.isRewardedVideoAvailable())
            {
                _rewardedShowing = true;
                if (string.IsNullOrEmpty(placement)) IronSource.Agent.showRewardedVideo();
                else                                 IronSource.Agent.showRewardedVideo(placement);
            }
            else
            {
                Debug.Log("[LevelPlay] 리워디드 준비 안 됨 — 재로드");
                IronSource.Agent.loadRewardedVideo();
            }
        }

        public void ShowRewarded(string placementId, Action onRewarded)
            => ShowRewarded(placementId, onRewarded, onFailed: null);

        public void ShowRewarded(string placementId, Action onRewarded, Action onFailed)
        {
            if (_rewardedShowing)
            {
                Debug.Log("[LevelPlay] 리워디드 표시 중 — ShowRewarded 중복 호출 무시");
                return;
            }

            // 새 요청 — 이전 pending 콜백 폐기
            _pendingRewardedPlacement    = placementId;
            _pendingRewardedCallback     = onRewarded;
            _pendingRewardedFailCallback = onFailed;

            if (!_initialized)
            {
                Debug.Log($"[LevelPlay] 초기화 대기 중 — 리워디드 예약 (placement={placementId})");
                return;
            }
            if (_rewardedReady) DoShowRewarded(placementId);
            else                IronSource.Agent.loadRewardedVideo(); // 로드되면 TryShowPendingRewarded 가 자동 표시
        }

        // ─────────────────────────────────────────────────────
        // Banner 광고 (Gameplay 씬 하단 — LevelPlay 8.x 새 API)
        // ─────────────────────────────────────────────────────

        private void CreateBannerAd()
        {
            if (_bannerAd != null) return; // 이미 생성됨
            if (string.IsNullOrEmpty(_bannerAdUnitId))
            {
                Debug.LogWarning("[LevelPlay] Banner Ad Unit ID 미설정 — 배너 비활성");
                return;
            }

            _bannerAd = new LevelPlayBannerAd(
                adUnitId:      _bannerAdUnitId,
                size:          LevelPlayAdSize.BANNER,
                position:      LevelPlayBannerPosition.BottomCenter,
                placementName: null,
                displayOnLoad: true);

            _bannerAd.OnAdLoaded     += HandleBannerLoaded;
            _bannerAd.OnAdLoadFailed += HandleBannerLoadFailed;
            _bannerAd.OnAdDisplayed  += HandleBannerDisplayed;
            Debug.Log($"[LevelPlay] Banner 인스턴스 생성 — adUnit={_bannerAdUnitId}");
        }

        private void HandleBannerLoaded(LevelPlayAdInfo info)
        {
            _bannerLoaded = true;
            Debug.Log("[LevelPlay] 배너 로드 성공 — 자동 표시됨 (displayOnLoad=true)");

            // 그 사이 HideBanner 호출됐으면 즉시 숨김
            if (!_bannerShown) _bannerAd?.HideAd();
        }

        private void HandleBannerLoadFailed(LevelPlayAdError err)
        {
            _bannerLoaded = false;
            Debug.LogWarning($"[LevelPlay] 배너 로드 실패: code={err.ErrorCode} — {err.ErrorMessage}");
        }

        private void HandleBannerDisplayed(LevelPlayAdInfo info)
        {
            Debug.Log("[LevelPlay] 배너 표시됨 (Displayed)");
        }

        public void ShowBanner()
        {
            _bannerShown = true;

            if (!_initialized)
            {
                Debug.Log("[LevelPlay] 초기화 대기 중 — 배너 예약 (Init 후 자동 로드)");
                return;
            }
            if (_bannerAd == null) CreateBannerAd();
            if (_bannerAd == null) return; // ad unit ID 없음

            if (_bannerLoaded) _bannerAd.ShowAd();    // 이미 로드됐으면 표시만
            else               _bannerAd.LoadAd();    // 아직 안 됐으면 로드 (displayOnLoad=true → 자동 표시)
        }

        public void HideBanner()
        {
            _bannerShown = false;
            _bannerAd?.HideAd();
            Debug.Log("[LevelPlay] 배너 숨김");
        }
#else
        public LevelPlayAdProvider(string appKey, bool testMode, string bannerAdUnitId = null)
        {
            Debug.LogWarning("[LevelPlay] com.unity.services.levelplay 패키지가 설치되지 않았습니다.\n" +
                             "Package Manager 에서 패키지를 추가하세요. 광고 미송출 상태입니다.");
        }

        public void Show(string placementId)
        {
            Debug.Log($"[LevelPlay] (stub) Show: {placementId} — 패키지 미설치");
        }

        public void ShowRewarded(string placementId, Action onRewarded)
        {
            Debug.Log($"[LevelPlay] (stub) ShowRewarded: {placementId} — 패키지 미설치, 즉시 보상 지급");
            onRewarded?.Invoke();
        }

        public void ShowRewarded(string placementId, Action onRewarded, Action onFailed)
            => ShowRewarded(placementId, onRewarded);

        public void ShowBanner()
        {
            Debug.Log("[LevelPlay] (stub) ShowBanner — 패키지 미설치");
        }

        public void HideBanner()
        {
            Debug.Log("[LevelPlay] (stub) HideBanner — 패키지 미설치");
        }
#endif
    }
}
