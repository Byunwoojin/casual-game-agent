using NUnit.Framework;
using SpotDiff.Data;

namespace SpotDiff.Tests.EditMode
{
    /// <summary>DiffRegion.IsHit — 실제 게임플레이 정답 판정 로직 (DiffGameBoard.TryFindAt 이 사용).</summary>
    public class DiffRegionHitTests
    {
        private static DiffRegion Region() => new DiffRegion
        {
            id = 1, x = 100, y = 100, width = 40, height = 30,
            centerX = 120, centerY = 115,
        };

        // TC-DR-001: 중심점 정확히 탭 → 항상 히트
        [Test]
        public void TC_DR_001_TapAtCenter_IsHit()
        {
            var r = Region();
            Assert.IsTrue(r.IsHit(120, 115, tolerancePx: 0));
        }

        // TC-DR-002: 영역 경계 바로 안쪽 → 히트 (tolerance 0)
        [Test]
        public void TC_DR_002_TapAtEdge_IsHit()
        {
            var r = Region(); // halfW=20, halfH=15 (tolerance 0)
            Assert.IsTrue(r.IsHit(120 + 20, 115, tolerancePx: 0));  // 우측 경계
            Assert.IsTrue(r.IsHit(120, 115 + 15, tolerancePx: 0));  // 하단 경계
        }

        // TC-DR-003: 영역 밖 (tolerance 0) → 미스
        [Test]
        public void TC_DR_003_TapOutside_IsMiss()
        {
            var r = Region();
            Assert.IsFalse(r.IsHit(120 + 21, 115, tolerancePx: 0));
            Assert.IsFalse(r.IsHit(120, 115 + 16, tolerancePx: 0));
        }

        // TC-DR-004: tolerance 를 늘리면 원래 영역 밖 탭도 히트로 잡힘 (모바일 탭 여유)
        [Test]
        public void TC_DR_004_ToleranceExpandsHitArea()
        {
            var r = Region();
            Assert.IsFalse(r.IsHit(150, 115, tolerancePx: 0),  "tolerance 0 이면 30px 떨어진 지점은 미스");
            Assert.IsTrue(r.IsHit(150, 115, tolerancePx: 40f), "tolerance 40 이면 히트로 잡힘 (halfW 20+40=60 >= 30)");
        }

        // TC-DR-005: x/y 축 중 하나라도 범위 밖이면 미스 (사각형 AND 조건 확인)
        [Test]
        public void TC_DR_005_OutsideOnEitherAxis_IsMiss()
        {
            var r = Region();
            // x 는 범위 안, y 만 범위 밖
            Assert.IsFalse(r.IsHit(120, 115 + 100, tolerancePx: 0));
            // y 는 범위 안, x 만 범위 밖
            Assert.IsFalse(r.IsHit(120 + 100, 115, tolerancePx: 0));
        }
    }
}
