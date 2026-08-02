using NUnit.Framework;
using UnityEngine;
using SpotDiff.Data;

namespace SpotDiff.Tests.EditMode
{
    public class LevelDataValidatorTests
    {
        // TC-LD-001: levelNumber 는 1 이상
        [Test]
        public void TC_LD_001_LevelNumber_ValidWhenPositive()
        {
            Assert.IsTrue(LevelDataValidator.ValidateLevelNumber(1));
            Assert.IsTrue(LevelDataValidator.ValidateLevelNumber(16));
            Assert.IsFalse(LevelDataValidator.ValidateLevelNumber(0));
            Assert.IsFalse(LevelDataValidator.ValidateLevelNumber(-1));
        }

        // TC-LD-002: 원본/변형 스프라이트 둘 다 있어야 유효
        [Test]
        public void TC_LD_002_ValidateSprites_RequiresBoth()
        {
            var tex = new Texture2D(2, 2);
            var sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);

            Assert.IsFalse(LevelDataValidator.ValidateSprites(null, null));
            Assert.IsFalse(LevelDataValidator.ValidateSprites(sprite, null));
            Assert.IsFalse(LevelDataValidator.ValidateSprites(null, sprite));
            Assert.IsTrue(LevelDataValidator.ValidateSprites(sprite, sprite));
        }

        // TC-LD-003: diffRegions 는 최소 1개 이상 있어야 유효
        [Test]
        public void TC_LD_003_ValidateDiffRegions_RequiresAtLeastOne()
        {
            Assert.IsFalse(LevelDataValidator.ValidateDiffRegions(null));
            Assert.IsFalse(LevelDataValidator.ValidateDiffRegions(new DiffRegion[0]));
            Assert.IsTrue(LevelDataValidator.ValidateDiffRegions(new[] { new DiffRegion { id = 1 } }));
        }
    }
}
