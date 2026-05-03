using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace PuzzlePals.UI
{
    /// <summary>
    /// Legacy Text 컴포넌트에 Localization key 를 바인딩.
    /// Locale 변경 시 자동 갱신, table 미세팅이면 기존 text 유지(폴백).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Text))]
    public class LocalizedTextBinder : MonoBehaviour
    {
        [SerializeField] private string tableName = "UI";
        [SerializeField] private string entryKey;

        public string TableName
        {
            get => tableName;
            set { tableName = value; Refresh(); }
        }

        public string EntryKey
        {
            get => entryKey;
            set { entryKey = value; Refresh(); }
        }

        private Text _text;
        private LocalizedString _ls;

        private void Awake() => _text = GetComponent<Text>();

        private void OnEnable()
        {
            Refresh();
            // SelectedLocaleChanged 직접 listen — LocalizedString.StringChanged 만으론
            // popup overlay 같은 일부 케이스에서 갱신이 일관되지 않음 (Unity Localization 6.x 알려진 함정).
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            if (_ls != null)
            {
                _ls.StringChanged -= OnStringChanged;
                _ls = null;
            }
        }

        private void OnLocaleChanged(Locale newLocale)
        {
            // _ls 재생성 → 새 locale 로 string 가져와서 OnStringChanged 발화
            Refresh();
        }

        public void Refresh()
        {
            if (_ls != null)
            {
                _ls.StringChanged -= OnStringChanged;
                _ls = null;
            }

            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(entryKey)) return;
            if (!Application.isPlaying) return; // 에디트 모드에선 fallback text 유지

            _ls = new LocalizedString(tableName, entryKey);
            _ls.StringChanged += OnStringChanged;
        }

        private void OnStringChanged(string newValue)
        {
            if (_text == null) return;
            // entry 누락 시 — fallback (BuildText 시 set 한 한국어 원문) 유지.
            // Unity Localization 은 키 못 찾으면 빈 문자열 또는 "No translation found for '...'" placeholder 반환.
            if (string.IsNullOrEmpty(newValue)) return;
            if (newValue.StartsWith("No translation found")) return;
            _text.text = newValue;
        }
    }
}
