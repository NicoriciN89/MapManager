namespace MapManager.Patches
{
    /// <summary>
    /// Intercept Localization.Get for all MAPMANAGER_ keys and return the correct language
    /// directly from our embedded dictionary — avoids StringTable timing issues entirely.
    /// Uses a flat per-language cache so each lookup is a single dictionary read.
    /// </summary>
    [HarmonyPatch(typeof(Localization), nameof(Localization.Get))]
    internal static class Patch_Localization_Get
    {
        private static string s_CachedLang = null;
        private static Dictionary<string, string> s_LangCache = null;

        [HarmonyPrefix]
        private static bool Prefix(string key, ref string __result)
        {
            if (key == null || !key.StartsWith("MAPMANAGER_")) return true;

            string lang = Localization.Language ?? "English";
            if (lang != s_CachedLang)
            {
                s_CachedLang = lang;
                var cache = new Dictionary<string, string>(Loc.s_Translations.Count);
                foreach (var kv in Loc.s_Translations)
                {
                    string val;
                    if (!kv.Value.TryGetValue(lang, out val) || string.IsNullOrWhiteSpace(val))
                        kv.Value.TryGetValue("English", out val);
                    cache[kv.Key] = val ?? kv.Key;
                }
                s_LangCache = cache;
            }

            if (s_LangCache != null && s_LangCache.TryGetValue(key, out __result))
                return false;

            return true;
        }
    }

    /// <summary>
    /// For MAPMANAGER_ description keys: store the raw key instead of a one-time translated
    /// snapshot, so get_Text can live-look it up each time (correct after language switches).
    /// Applied manually because ModSettings.DescriptionHolder is internal.
    /// </summary>
    internal static class Patch_DescriptionHolder_SetDescription
    {
        internal static void Prefix(string description, ref bool localize)
        {
            // localize=true path stores raw key; localize=false would call Localization.Get
            // immediately and cache the result — bad if called before the language is loaded.
            if (!localize && description != null && description.StartsWith("MAPMANAGER_"))
                localize = true;
        }
    }

    /// <summary>
    /// When Text holds a MAPMANAGER_ key (stored by the SetDescription patch above),
    /// return a live translation instead of the cached key string.
    /// Applied manually because ModSettings.DescriptionHolder is internal.
    /// </summary>
    internal static class Patch_DescriptionHolder_get_Text
    {
        internal static void Postfix(ref string __result)
        {
            if (__result != null && __result.StartsWith("MAPMANAGER_"))
                __result = Localization.Get(__result);
        }
    }
}
