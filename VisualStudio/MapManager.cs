global using ComplexLogger;

using MapManager.Patches;

namespace MapManager
{
    internal class Main : MelonMod
    {
        public static ComplexLogger<Main> Logger = new ComplexLogger<Main>();
        public override void OnInitializeMelon()
        {
            Settings.OnLoad();
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
			if (MapDetailManager.s_MapDetails == null)
			{
				Logger.Log($"[Diag] Scene '{sceneName}' — s_MapDetails is null, skipping DupeFix", FlaggedLoggingLevel.Debug);
				return;
			}

			int totalBefore = MapDetailManager.s_MapDetails.Count;
			Logger.Log($"[Diag] Scene '{sceneName}' loaded — MapDetails registered: {totalBefore}", FlaggedLoggingLevel.Debug);

			HashSet<string> seenGuids = new HashSet<string>();
			List<MapDetail> toRemove = new List<MapDetail>();

			foreach (MapDetail detail in MapDetailManager.s_MapDetails)
			{
				if (detail == null) continue;
				GameObject go = detail.gameObject;
				if (go == null) continue;
				ObjectGuid objectGuid = go.GetComponent<ObjectGuid>();
				if (objectGuid == null) continue;
				string guid = objectGuid.m_Guid;
				if (string.IsNullOrWhiteSpace(guid)) continue;

				if (!seenGuids.Add(guid))
				{
					toRemove.Add(detail);
				}
			}

			foreach (MapDetail detail in toRemove)
			{
				string locId = detail.m_LocID ?? "(null)";
				MapDetailManager.Unregister(detail);
				Logger.Log($"[Diag] DupeFix: Removed duplicate MapDetail LocID='{locId}' in scene '{sceneName}'", FlaggedLoggingLevel.Debug);
			}

			if (toRemove.Count > 0)
				Logger.Log($"[Diag] DupeFix summary for '{sceneName}': removed {toRemove.Count} duplicates (was {totalBefore}, now {MapDetailManager.s_MapDetails.Count})", FlaggedLoggingLevel.Debug);
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