using NUnit.Framework;
using UnityEngine;
using MemoryMatch.Data;

namespace MemoryMatch.Tests.EditMode
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

        // TC-LD-002: 카드 스프라이트 배열이 pairCount 이상은 있어야 유효
        [Test]
        public void TC_LD_002_ValidateCardSprites_RequiresEnoughSprites()
        {
            var tex = new Texture2D(2, 2);
            var sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
            var fourSprites = new[] { sprite, sprite, sprite, sprite };

            Assert.IsFalse(LevelDataValidator.ValidateCardSprites(null, 4));
            Assert.IsFalse(LevelDataValidator.ValidateCardSprites(new Sprite[2], 4), "쌍 개수보다 스프라이트가 적으면 무효");
            Assert.IsTrue(LevelDataValidator.ValidateCardSprites(fourSprites, 4));
        }

        // TC-LD-003: pairCount 는 2 이상 (1쌍짜리는 매칭 게임이 성립 안 함)
        [Test]
        public void TC_LD_003_ValidateCardSprites_RequiresAtLeastTwoPairs()
        {
            var tex = new Texture2D(2, 2);
            var sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
            var oneSprite = new[] { sprite };

            Assert.IsFalse(LevelDataValidator.ValidateCardSprites(oneSprite, 1));
            Assert.IsFalse(LevelDataValidator.ValidateCardSprites(oneSprite, 0));
        }
    }
}
