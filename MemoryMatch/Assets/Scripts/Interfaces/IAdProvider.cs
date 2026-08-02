using System;

namespace MemoryMatch.Interfaces
{
    /// <summary>광고 표시 추상화 — 테스트에서 Mock 주입 가능</summary>
    public interface IAdProvider
    {
        /// <summary>인터스티셜 광고 (전면) — 콜백 없음.</summary>
        void Show(string placementId);

        /// <summary>리워디드 광고 (보상형). 보상 지급 시점에 onRewarded 콜백 호출.
        /// 광고 미준비/스킵/실패 시엔 콜백 호출 안 됨.</summary>
        void ShowRewarded(string placementId, Action onRewarded);

        /// <summary>리워디드 광고 (보상형) — 보상 / 실패 콜백 분리 버전.
        /// 보상 지급 시 onRewarded, 로드/표시 실패 시 onFailed 호출 (둘 중 하나만).</summary>
        void ShowRewarded(string placementId, Action onRewarded, Action onFailed);

        /// <summary>배너 광고 표시 — 화면 하단 320×50 (Smart banner). idempotent: 이미 표시 중이면 no-op.</summary>
        void ShowBanner();

        /// <summary>배너 광고 숨김 + 메모리 해제. 다른 씬으로 이동 시 호출.</summary>
        void HideBanner();
    }
}
