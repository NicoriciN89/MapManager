namespace MapManager
{
    [HarmonyPatch(typeof(Panel_Map), nameof(Panel_Map.Enable), new Type[] { typeof(bool), typeof(bool) })]
    internal class Panel_Map_Initialize
    {
        private static void Postfix(Panel_Map __instance, bool enable, bool cameFromDetailSurvey)
        {
            if (!enable) return;

            Panel_Map.ResetOpts opts = Panel_Map.ResetOpts.Zoomed;
            if (Settings.Instance.EnableArrow)
            {
                opts |= Panel_Map.ResetOpts.ShowPlayer;
            }
            if (Settings.Instance.CenterOnPlayer)
            {
                opts |= Panel_Map.ResetOpts.CenterOnPlayer;
            }
            Main.Logger.Log($"[Diag] Map opened: opts={opts}, fromSurvey={cameFromDetailSurvey}", FlaggedLoggingLevel.Debug);
            __instance.ResetToNormal(opts);
        }
    }
}
