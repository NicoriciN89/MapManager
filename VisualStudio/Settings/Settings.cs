using ModSettings;

namespace MapManager
{
	internal class Settings : JsonModSettings
	{
		internal static Settings Instance { get; } = new();

		[Section("MAPMANAGER_SECTION_MAIN", Localize = true)]
		[Name("MAPMANAGER_ENABLE_ARROW", Localize = true)]
		[Description("MAPMANAGER_ENABLE_ARROW_DESC")]
		public bool EnableArrow                 = false;

		[Name("MAPMANAGER_CENTER_PLAYER", Localize = true)]
		[Description("MAPMANAGER_CENTER_PLAYER_DESC")]
		public bool CenterOnPlayer              = false;

		[Name("MAPMANAGER_REVEAL_MAP", Localize = true)]
		[Description("MAPMANAGER_REVEAL_MAP_DESC")]
		public KeyCode RevealMap                = KeyCode.KeypadDivide;

		[Section("MAPMANAGER_SECTION_SURVEY", Localize = true)]
		[Name("MAPMANAGER_RANGE_MULT", Localize = true)]
		[Slider(0f, 25f, NumberFormat = "{0:F2}")]
		public float MapSurveyMult              = 1f;

		[Name("MAPMANAGER_SURVEY_TIME", Localize = true)]
		[Description("MAPMANAGER_SURVEY_TIME_DESC")]
		[Slider(0.1f, 5f, NumberFormat = "{0:F2}")]
		public float MapSurveyMultTime          = 1f;

		[Name("MAPMANAGER_POLAROID", Localize = true)]
		[Description("MAPMANAGER_POLAROID_DESC")]
		public bool MapWithPolariods            = false;

		[Name("MAPMANAGER_REVEAL_VISTA", Localize = true)]
		[Description("MAPMANAGER_REVEAL_VISTA_DESC")]
		public bool RevealVista                 = false;

		[Name("MAPMANAGER_UNLOCK_SURVEY", Localize = true)]
		[Description("MAPMANAGER_UNLOCK_SURVEY_DESC")]
		public bool UnlockSurvey                = false;

		[Name("MAPMANAGER_ADD_CORPSE", Localize = true)]
		[Description("MAPMANAGER_ADD_CORPSE_DESC")]
		public bool AddCorpseToMap = false;


		internal static void OnLoad()
		{
			Instance.AddToModSettings($"{BuildInfo.GUIName}");
			Instance.RefreshGUI();
		}
	}
}
