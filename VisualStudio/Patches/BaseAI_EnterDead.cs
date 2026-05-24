namespace MapManager.Patches
{
	[HarmonyPatch(typeof(BaseAi), nameof(BaseAi.EnterDead))]
	public class BaseAI_EnterDead
	{
		public static void Postfix(BaseAi __instance)
		{
			if (__instance == null || !Settings.Instance.AddCorpseToMap) return;
			MapDetail mapDetail = __instance.gameObject.GetComponent<MapDetail>();
			if (mapDetail == null)
			{
				Main.Logger.Log($"EnterDead: {__instance.name} has no MapDetail component, skipped", FlaggedLoggingLevel.Verbose);
				return;
			}
			mapDetail.ShowOnMap(true);
			Main.Logger.Log($"EnterDead: Added {__instance.name} to map", FlaggedLoggingLevel.Debug);
		}
	}
}
