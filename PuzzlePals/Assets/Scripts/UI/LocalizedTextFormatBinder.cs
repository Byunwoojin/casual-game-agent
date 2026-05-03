using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace PuzzlePals.UI
{
    /// <summary>
    /// Format placeholder ({0}, {1}, ...) 가 있는 다국어 텍스트용 binder.
    /// LocalizedTextBinder 가 단순 string 만 지원해서 별도 컴포넌트.
    ///
    /// LocalizedString + Arguments 를 컴포넌트 필드로 보관 — inline closure 방식의 GC 위험 회피.
    /// SelectedLocaleChanged 도 직접 listen — popup overlay 등 일부 케이스에서 StringChanged 가
    /// 일관되지 않은 Unity Localization 6.x 알려진 함정 회피.
    ///
    /// 사용: DesignTokens.BuildLocalizedTextFormat 이 자동 부착. 호출 측에서 직접 추가할 일은 거의 없음.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Text))]
    public class LocalizedTextFormatBinder : MonoBehaviour
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

        public object[] Arguments { get; set; }

        private Text _text;
        private LocalizedString _ls;

        private void Awake() => _text = GetComponent<Text>();

        private void OnEnable()
        {
            Refresh();
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

        private void OnLocaleChanged(Locale newLocale) => Refresh();

        public void Refresh()
        {
            if (_ls != null)
            {
                _ls.StringChanged -= OnStringChanged;
                _ls = null;
            }

            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(entryKey)) return;
            if (!Application.isPlaying) return;

            _ls = new LocalizedString(tableName, entryKey) { Arguments = Arguments };
            _ls.StringChanged += OnStringChanged;
        }

        private void OnStringChanged(string newValue)
        {
            if (_text == null) return;
            if (string.IsNullOrEmpty(newValue)) return;
            if (newValue.StartsWith("No translation found")) return;
            _text.text = newValue;
        }
    }
}
