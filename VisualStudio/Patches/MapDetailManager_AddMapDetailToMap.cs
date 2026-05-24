namespace MapManager
{
	// Patch MapDetailManager.Register() to prevent duplicate LocID registrations.
	// Root cause: MountainTownRegion has 3 copies of STR_HouseExteriorBMiltonBurnt_Prefab
	// all sharing the same m_LocID. ObjectGuid is on the PARENT object, not on MapDetail's
	// GameObject — so GetComponent<ObjectGuid>() was always returning null and skipping.
	// Fix: use m_LocID directly, which is the same field the game itself checks in Serialize().
	[HarmonyPatch(typeof(MapDetailManager), nameof(MapDetailManager.Register))]
	internal class MapDetailManager_Register
	{
		private static bool Prefix(MapDetail mapDetail)
		{
			if (mapDetail == null) return true;
			try
			{
				string locId = mapDetail.m_LocID;
				if (string.IsNullOrWhiteSpace(locId)) return true;
				// GAMEPLAY_ LocIDs are shared type-names (e.g. GAMEPLAY_BranchBigHard).
				// Dozens of valid separate objects share them — not real duplicates.
				if (locId.StartsWith("GAMEPLAY_", StringComparison.Ordinal)) return true;

				if (MapDetailManager.s_MapDetails == null) return true;
				foreach (MapDetail detail in MapDetailManager.s_MapDetails)
				{
					if (detail == null) continue;
					if (detail.Pointer == mapDetail.Pointer) continue;
					if (detail.m_LocID == locId)
					{
						Main.Logger.Log($"DupeFix [Register]: Blocked duplicate LocID={locId}", FlaggedLoggingLevel.Debug);
						return false;
					}
				}
			}
			catch (Exception ex)
			{
				Main.Logger.Log($"DupeFix [Register]: error: {ex.Message}\n{ex.StackTrace}", FlaggedLoggingLevel.Error);
			}
			return true;
		}
	}

	// Patch MapDetailManager.Serialize() to deduplicate s_MapDetails before every save.
	// Last line of defense: catches duplicates that slipped through Register() or were loaded
	// from old save data, preventing the Unity 6000 crash during asset unloading.
	[HarmonyPatch(typeof(MapDetailManager), nameof(MapDetailManager.Serialize))]
	internal class MapDetailManager_Serialize
	{
		private static void Prefix()
		{
			if (MapDetailManager.s_MapDetails == null) return;
			try
			{
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
					Main.Logger.Log($"DupeFix [Serialize]: Removed duplicate LocID='{locId}'", FlaggedLoggingLevel.Debug);
				}
			}
			catch (Exception ex)
			{
				Main.Logger.Log($"DupeFix [Serialize]: error: {ex.Message}", FlaggedLoggingLevel.Error);
			}
		}
	}
}
