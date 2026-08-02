// LevelDataSO 유효성 검증 — 성인 피벗 v0.5
using NUnit.Framework;
using UnityEngine;
using PuzzlePals.Data;

namespace PuzzlePals.Tests.EditMode
{
    public class LevelDataTests
    {
        private LevelDataSO _levelData;

        [SetUp]
        public void SetUp()
        {
            _levelData = ScriptableObject.CreateInstance<LevelDataSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_levelData);
        }

        // TC-LD-001: levelNumber 는 1~16 범위 (v0.5)
        [Test]
        public void TC_LD_001_LevelNumber_ValidRange1To16()
        {
            Assert.IsTrue(LevelDataValidator.ValidateLevelNumber(1));
            Assert.IsTrue(LevelDataValidator.ValidateLevelNumber(10));
            Assert.IsTrue(LevelDataValidator.ValidateLevelNumber(16));

            Assert.IsFalse(LevelDataValidator.ValidateLevelNumber(0),  "0 invalid");
            Assert.IsFalse(LevelDataValidator.ValidateLevelNumber(17), "17 invalid");
            Assert.IsFalse(LevelDataValidator.ValidateLevelNumber(100),"100 invalid (구 v0.3 잔재)");
        }

        // TC-LD-002: characterSprite 가 null 이 아니어야 한다
        [Test]
        public void TC_LD_002_CharacterSprite_NotNull()
        {
            _levelData.characterSprite = null;
            Assert.IsFalse(LevelDataValidator.ValidateCharacterSprite(_levelData.characterSprite));

            var tex = new Texture2D(2, 2);
            _levelData.characterSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
            Assert.IsTrue(LevelDataValidator.ValidateCharacterSprite(_levelData.characterSprite));
        }

        // TC-LD-003: pieceCount 는 36 또는 64만 (v0.5 — 정사각 셀 보장 위해 제곱수만)
        [Test]
        public void TC_LD_003_PieceCount_Only36Or64()
        {
            Assert.IsTrue(LevelDataValidator.ValidatePieceCount(36));
            Assert.IsTrue(LevelDataValidator.ValidatePieceCount(64));

            Assert.IsFalse(LevelDataValidator.ValidatePieceCount(0));
            Assert.IsFalse(LevelDataValidator.ValidatePieceCount(16));
            Assert.IsFalse(LevelDataValidator.ValidatePieceCount(32), "32 는 4×8 직사각 셀 → v0.5에서 폐기");
            Assert.IsFalse(LevelDataValidator.ValidatePieceCount(48));
            Assert.IsFalse(LevelDataValidator.ValidatePieceCount(100));
        }

        // TC-LD-004: characterNameKey 가 비어있지 않아야 한다
        [Test]
        public void TC_LD_004_CharacterNameKey_NotEmpty()
        {
            Assert.IsFalse(LevelDataValidator.ValidateCharacterNameKey(""));
            Assert.IsFalse(LevelDataValidator.ValidateCharacterNameKey(null));

            Assert.IsTrue(LevelDataValidator.ValidateCharacterNameKey("art_irises"));
        }

        // TC-LD-005: imageBucket enum 5종 (VeryEasy ~ VeryHard)
        [Test]
        public void TC_LD_005_ImageBucket_AllFiveValuesValid()
        {
            _levelData.imageBucket = LevelDataSO.ImageBucket.VeryEasy;
            Assert.AreEqual(LevelDataSO.ImageBucket.VeryEasy, _levelData.imageBucket);

            _levelData.imageBucket = LevelDataSO.ImageBucket.Easy;
            _levelData.imageBucket = LevelDataSO.ImageBucket.Medium;
            _levelData.imageBucket = LevelDataSO.ImageBucket.Hard;
            _levelData.imageBucket = LevelDataSO.ImageBucket.VeryHard;
            Assert.AreEqual(LevelDataSO.ImageBucket.VeryHard, _levelData.imageBucket);
        }
    }
}
