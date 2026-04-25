using System.Collections.Generic;

namespace PuzzlePals.Data
{
    /// <summary>
    /// 명화 제목 로컬라이제이션 — 성인 피벗 v0.4 MVP.
    ///
    /// 정식 Unity Localization Tables 채우기 전 임시 한국어 매핑.
    /// LevelDataSO.characterNameKey ("art_irises" 등) → 한국어 작품 제목.
    /// 매핑에 없는 키는 그대로 반환 (raw key 노출 fallback).
    ///
    /// 출시 후 다국어 (en/ja/zh-Hans) 필요 시 Unity Localization Tables 로 마이그레이션.
    /// </summary>
    public static class ArtTitleLookup
    {
        private static readonly Dictionary<string, string> Korean = new Dictionary<string, string>
        {
            { "art_veteran_new_field",      "신대륙의 베테랑" },
            { "art_irises",                 "붓꽃" },
            { "art_great_wave",             "가나가와 해변의 큰 파도" },
            { "art_parthenon",              "파르테논 신전" },
            { "art_bridge_villeneuve",      "빌뇌브 다리" },
            { "art_teapot_fruit",           "주전자와 과일" },
            { "art_self_portrait_straw_hat","밀짚모자를 쓴 자화상" },
            { "art_autumn_oaks",            "가을의 참나무" },
            { "art_snap_the_whip",          "채찍 휘두르기" },
            { "art_two_girls_piano",        "피아노 앞의 두 소녀" },
            { "art_wheat_field_cypresses",  "사이프러스가 있는 밀밭" },
            { "art_card_players",           "카드놀이 하는 사람들" },
            { "art_by_the_seashore",        "바닷가에서" },
            { "art_jalais_hill",            "잘레 언덕, 퐁투아즈" },
            { "art_charpentier_children",   "샤르팡티에 부인의 아이들" },
            { "art_venice_turner",          "베니스" },
            { "art_boulevard_montmartre",   "몽마르트르 거리" },
            { "art_wyoming_valley",         "와이오밍 계곡" },
            { "art_the_harvesters",         "추수하는 사람들" },
            { "art_rocky_mountains",        "로키 산맥, 랜더 봉" },
        };

        /// <summary>art_* 키 → 한국어 제목. 매핑 없으면 raw key 그대로 반환.</summary>
        public static string GetKoreanTitle(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";
            return Korean.TryGetValue(key, out var title) ? title : key;
        }
    }
}
