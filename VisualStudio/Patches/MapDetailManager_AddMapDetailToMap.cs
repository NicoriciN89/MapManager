namespace MapManager
{
	// Patch MapDetailManager.Register() to prevent duplicate GUID registrations.
	// Root cause: MountainTownRegion has 3 copies of STR_HouseExteriorBMiltonBurnt_Prefab
	// all sharing the same ObjectGuid. With MapManager large survey radius, all 3 get
	// surveyed and registered -> Unity 6000 asset unloader crashes on scene exit.
	[HarmonyPatch(typeof(MapDetailManager), nameof(MapDetailManager.Register))]
	internal class MapDetailManager_Register
	{
		private static bool Prefix(MapDetail mapDetail)
		{
			if (mapDetail == null) return true;
			try
			{
				GameObject go = mapDetail.gameObject;
				if (go == null) return true;
				ObjectGuid objectGuid = go.GetComponent<ObjectGuid>();
				if (objectGuid == null) return true;
				string guid = objectGuid.m_Guid;
				if (string.IsNullOrWhiteSpace(guid)) return true;

				if (MapDetailManager.s_MapDetails == null) return true;
				foreach (MapDetail detail in MapDetailManager.s_MapDetails)
				{
					if (detail == null) continue;
					if (detail.Pointer == mapDetail.Pointer) continue;
					GameObject detailGo = detail.gameObject;
					if (detailGo == null) continue;
					ObjectGuid detailGuid = detailGo.GetComponent<ObjectGuid>();
					if (detailGuid == null) continue;
					if (detailGuid.m_Guid == guid)
					{
						Main.Logger.Log($"DupeFix [Register]: Blocked duplicate GUID={guid} LocID={mapDetail.m_LocID}", FlaggedLoggingLevel.Debug);
						return false;
					}
				}
			}
			catch (Exception ex)
			{
				Main.Logger.Log($"[Diag] DupeFix error: {ex.Message}\n{ex.StackTrace}", FlaggedLoggingLevel.Error);
			}
			return true;
		}
	}

	// Patch MapDetailManager.Serialize() to deduplicate s_MapDetails before every save.
	// This is the last line of defense: catches any duplicates that slipped through Register()
	// or were loaded from old save data via Deserialize(), preventing the Unity 6000 crash
	// during asset unloading that follows scene serialization.
	[HarmonyPatch(typeof(MapDetailManager), nameof(MapDetailManager.Serialize))]
	internal class MapDetailManager_Serialize
	{
		private static void Prefix()
		{
			if (MapDetailManager.s_MapDetails == null) return;
			try
			{
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
					Main.Logger.Log($"DupeFix [Serialize]: Removed duplicate LocID='{locId}'", FlaggedLoggingLevel.Debug);
				}
			}
			catch (Exception ex)
			{
				Main.Logger.Log($"[Diag] DupeFix Serialize error: {ex.Message}", FlaggedLoggingLevel.Error);
			}
		}
	}
}
