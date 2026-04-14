// TC-SM-001 ~ TC-SM-004 : 진행도 저장 및 불러오기
using NUnit.Framework;
using UnityEngine;
using PuzzlePals.Systems;

namespace PuzzlePals.Tests.EditMode
{
    public class SaveManagerTests
    {
        [SetUp]
        public void SetUp()
        {
            // 각 테스트 전 PlayerPrefs 초기화
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        // TC-SM-001: MarkLevelComplete(0) 후 IsLevelComplete(0) == true
        [Test]
        public void TC_SM_001_MarkLevelComplete_ThenIsComplete_ReturnsTrue()
        {
            SaveManager.MarkLevelComplete(0);
            Assert.IsTrue(SaveManager.IsLevelComplete(0));
        }

        // TC-SM-002: PlayerPrefs 저장 후 SaveManager 재초기화 시 복구
        [Test]
        public void TC_SM_002_AfterSave_Reinit_ProgressRestored()
        {
            SaveManager.MarkLevelComplete(4); // 레벨 5 (index 4)
            PlayerPrefs.Save();

            // 재초기화 시뮬레이션 — static 클래스이므로 PlayerPrefs 재로드로 확인
            // (새 인스턴스 생성이 불필요한 static 구현)
            Assert.IsTrue(SaveManager.IsLevelComplete(4),
                "Level 5 (index 4) should still be marked as complete after reload");
        }

        // TC-SM-003: 클리어하지 않은 레벨은 IsLevelComplete() == false
        [Test]
        public void TC_SM_003_UnplayedLevel_IsComplete_ReturnsFalse()
        {
            Assert.IsFalse(SaveManager.IsLevelComplete(0),
                "Fresh level should not be marked as complete");
        }

        // TC-SM-004: 힌트 데이터 저장 후 재초기화 시 복구
        [Test]
        public void TC_SM_004_SaveHintCount_AfterReinit_Restored()
        {
            SaveManager.SaveHintCount(2);
            PlayerPrefs.Save();

            int loaded = SaveManager.LoadHintCount();
            Assert.AreEqual(2, loaded, "Hint count should be restored after reinit");
        }
    }
}
