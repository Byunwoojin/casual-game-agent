using NUnit.Framework;
using UnityEngine;
using SlidingPuzzle.Data;

namespace SlidingPuzzle.Tests.EditMode
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

        // TC-LD-002: 원본 스프라이트가 있어야 유효
        [Test]
        public void TC_LD_002_ValidateSprite_RequiresNonNull()
        {
            var tex = new Texture2D(2, 2);
            var sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);

            Assert.IsFalse(LevelDataValidator.ValidateSprite(null));
            Assert.IsTrue(LevelDataValidator.ValidateSprite(sprite));
        }

        // TC-LD-003: gridSize 는 2 이상 (2×2 미만은 슬라이딩 퍼즐이 성립 안 함)
        [Test]
        public void TC_LD_003_ValidateGridSize_RequiresAtLeastTwo()
        {
            Assert.IsFalse(LevelDataValidator.ValidateGridSize(0));
            Assert.IsFalse(LevelDataValidator.ValidateGridSize(1));
            Assert.IsTrue(LevelDataValidator.ValidateGridSize(2));
            Assert.IsTrue(LevelDataValidator.ValidateGridSize(5));
        }
    }
}
