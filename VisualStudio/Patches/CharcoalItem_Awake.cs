namespace MapManager
{
    [HarmonyPatch(typeof(CharcoalItem), nameof(CharcoalItem.Awake))]
    internal class CharcoalItem_Awake
    {
        private static void Postfix(CharcoalItem __instance)
        {
            float before = __instance.m_SurveyGameMinutes;
            __instance.m_SurveyGameMinutes *= Settings.Instance.MapSurveyMultTime;
            Main.Logger.Log($"[Diag] CharcoalItem survey time: {before:F1} min → {__instance.m_SurveyGameMinutes:F1} min (x{Settings.Instance.MapSurveyMultTime:F2})", FlaggedLoggingLevel.Debug);
        }
    }
}
