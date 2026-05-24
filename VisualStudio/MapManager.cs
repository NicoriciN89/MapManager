global using ComplexLogger;

using MapManager.Patches;

namespace MapManager
{
    internal class Main : MelonMod
    {
        public static ComplexLogger<Main> Logger = new ComplexLogger<Main>();
        public override void OnInitializeMelon()
        {
            ApplyDescriptionHolderPatches();
            Settings.OnLoad();
        }

        private void ApplyDescriptionHolderPatches()
        {
            try
            {
                var descType = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "ModSettings")
                    ?.GetType("ModSettings.DescriptionHolder");

                if (descType == null)
                {
                    Logger.Log("[MapManager] DescriptionHolder type not found — description patches skipped", FlaggedLoggingLevel.Debug);
                    return;
                }

                var setDescMethod = descType.GetMethod("SetDescription",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var getTextMethod = descType.GetProperty("Text",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?.GetGetMethod(nonPublic: true);

                if (setDescMethod != null)
                {
                    var prefix = typeof(Patch_DescriptionHolder_SetDescription)
                        .GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                    if (prefix != null)
                        HarmonyInstance.Patch(setDescMethod, new HarmonyMethod(prefix));
                }

                if (getTextMethod != null)
                {
                    var postfix = typeof(Patch_DescriptionHolder_get_Text)
                        .GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                    if (postfix != null)
                        HarmonyInstance.Patch(getTextMethod, postfix: new HarmonyMethod(postfix));
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[MapManager] DescriptionHolder patch failed: {ex.Message}");
            }
        }

		public override void OnUpdate()
		{
			if (!GameManager.IsMainMenuActive() && InputManager.instance != null && InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.Instance.RevealMap))
			{
				InterfaceManager.GetPanel<Panel_Map>().RevealCurrentScene();
			}
		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			if (MapDetailManager.s_MapDetails == null) return;

			int totalBefore = MapDetailManager.s_MapDetails.Count;

			HashSet<string> seenLocIds = new HashSet<string>();
			List<MapDetail> toRemove = new List<MapDetail>();

			foreach (MapDetail detail in MapDetailManager.s_MapDetails)
			{
				if (detail == null) continue;
				string locId = detail.m_LocID;
				if (string.IsNullOrWhiteSpace(locId)) continue;
				// GAMEPLAY_ LocIDs are type-names shared by many legitimate instances.
				if (locId.StartsWith("GAMEPLAY_", StringComparison.Ordinal)) continue;

				if (!seenLocIds.Add(locId))
				{
					toRemove.Add(detail);
				}
			}

			foreach (MapDetail detail in toRemove)
			{
				string locId = detail.m_LocID ?? "(null)";
				MapDetailManager.Unregister(detail);
				Logger.Log($"DupeFix [Scene]: Removed duplicate LocID='{locId}' in '{sceneName}'", FlaggedLoggingLevel.Debug);
			}

			if (toRemove.Count > 0)
				Logger.Log($"DupeFix [Scene]: Removed {toRemove.Count} duplicate(s) in '{sceneName}' (was {totalBefore}, now {MapDetailManager.s_MapDetails.Count})", FlaggedLoggingLevel.Debug);
		}

		public static AssetBundle LoadAssetBundle(string name)
		{
			using (Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
			{
				MemoryStream? memory = new((int)stream.Length);
				stream!.CopyTo(memory);
				return AssetBundle.LoadFromMemory(memory.ToArray());
			};
		}
	}

	public static class Extensions
	{
		//https://github.dev/NuclearPowered/Reactor/blob/6eb0bf19c30733b78532dada41db068b2b247742/Reactor/Utilities/DefaultBundle.cs#L17#L40
		/// <summary>
		/// Stops <paramref name="obj"/> from being unloaded.
		/// </summary>
		/// <param name="obj">The object to stop from being unloaded.</param>
		/// <typeparam name="T">The type of the object.</typeparam>
		/// <returns>Passed <paramref name="obj"/>.</returns>
		public static T DontUnload<T>(this T obj) where T : UnityEngine.Object
		{
			obj.hideFlags |= HideFlags.DontUnloadUnusedAsset;

			return obj;
		}
	}
}