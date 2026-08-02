using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace SlidingPuzzle.EditorTools
{
    /// <summary>
    /// Unity Localization 기본 세팅 자동화:
    ///   - Locale 4개 에셋 생성 (한국어/영어/일본어/중국어 간체)
    ///   - LocalizationSettings 에셋 생성 + Project Settings > Localization 에 Active 등록
    ///   - 시작 Locale = 한국어, 폴백 = 영어
    /// idempotent — 재실행 안전.
    ///
    /// 사용법: Unity 상단 메뉴 → [SlidingPuzzle] → [Setup Localization (Locales + Settings)]
    /// </summary>
    public static class LocalizationSetupTool
    {
        private const string LocRoot      = "Assets/Localization";
        private const string LocaleFolder = "Assets/Localization/Locales";
        private const string SettingsPath = "Assets/Localization/LocalizationSettings.asset";

        // BCP-47 코드 / 표시명 / 한국어 표시명
        private static readonly (string code, string english, string native)[] Locales =
        {
            ("ko",      "Korean",               "한국어"),
            ("en",      "English",              "English"),
            ("ja",      "Japanese",             "日本語"),
            ("zh-Hans", "Chinese (Simplified)", "简体中文"),
        };

        [MenuItem("SlidingPuzzle/Setup Localization (Locales + Settings)")]
        public static void SetupLocalization()
        {
            EnsureFolder(LocRoot);
            EnsureFolder(LocaleFolder);

            // 0) Addressables 초기화 — Localization 은 Addressables 기반으로 Locale 로드
            EnsureAddressablesInitialized();

            // 1) Locale 4개 에셋 생성 + LocalizationEditorSettings 등록 + Addressables 등록
            var created = new Locale[Locales.Length];
            for (int i = 0; i < Locales.Length; i++)
            {
                created[i] = LoadOrCreateLocale(Locales[i].code, Locales[i].native);
                if (LocalizationEditorSettings.GetLocale(new LocaleIdentifier(Locales[i].code)) == null)
                    LocalizationEditorSettings.AddLocale(created[i]);

                // Addressables 그룹("Localization-Locales") 에 명시적 등록 — runtime 로드 보장
                MarkAsAddressable(created[i]);
            }

            // 2) LocalizationSettings 에셋 생성 또는 로드
            var settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                settings.name = "LocalizationSettings";
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            // 3) Project Settings > Localization 의 Active Settings 로 지정
            LocalizationEditorSettings.ActiveLocalizationSettings = settings;

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[LocalizationSetupTool] Localization 세팅 완료 — Locale 4개 + Settings 등록");
            EditorUtility.DisplayDialog("SlidingPuzzle",
                "Localization 세팅 완료\n\n" +
                "Locales:\n" +
                "  • ko (한국어) — 기본\n" +
                "  • en (English) — 폴백\n" +
                "  • ja (日本語)\n" +
                "  • zh-Hans (简体中文)\n\n" +
                "다음 단계: SlidingPuzzle > Populate String Tables",
                "OK");
        }

        private static Locale LoadOrCreateLocale(string code, string displayName)
        {
            string path = $"{LocaleFolder}/Locale_{code}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Locale>(path);
            if (existing != null) return existing;

            var locale = Locale.CreateLocale(new LocaleIdentifier(code));
            locale.name = $"Locale_{code}";
            AssetDatabase.CreateAsset(locale, path);
            return locale;
        }

        // Addressables 기본 세팅 보장 — 없으면 새로 생성
        private static void EnsureAddressablesInitialized()
        {
            if (AddressableAssetSettingsDefaultObject.Settings == null)
            {
                AddressableAssetSettingsDefaultObject.Settings =
                    AddressableAssetSettings.Create(
                        AddressableAssetSettingsDefaultObject.kDefaultConfigFolder,
                        AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName,
                        true, true);
            }
        }

        // Locale 에셋을 "Localization-Locales" Addressables 그룹에 등록
        private static void MarkAsAddressable(Locale locale)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;

            string assetPath = AssetDatabase.GetAssetPath(locale);
            string guid      = AssetDatabase.AssetPathToGUID(assetPath);

            const string groupName = "Localization-Locales";
            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                group = settings.CreateGroup(groupName, false, false, false, null,
                    typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema),
                    typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.ContentUpdateGroupSchema));
            }

            // 이미 등록돼 있으면 스킵
            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                entry = settings.CreateOrMoveEntry(guid, group, false, false);
                entry.address  = locale.Identifier.Code;
                entry.SetLabel("Locale", true, true, false);
            }
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            string leaf   = Path.GetFileName(folder);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
