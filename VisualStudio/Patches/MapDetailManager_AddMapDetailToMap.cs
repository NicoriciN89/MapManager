using HarmonyLib;
using MelonLoader;
using System;

namespace MapManager.Patches
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
                var objectGuid = mapDetail.gameObject?.GetComponent<ObjectGuid>();
                if (objectGuid == null) return true;
                string guid = objectGuid.m_Guid;
                if (string.IsNullOrEmpty(guid)) return true;

                var existing = MapDetailManager.s_MapDetails;
                if (existing == null) return true;

                foreach (MapDetail detail in existing)
                {
                    if (detail == null) continue;
                    if (detail.Pointer == mapDetail.Pointer) continue;
                    var detailGuid = detail.gameObject?.GetComponent<ObjectGuid>();
                    if (detailGuid != null && detailGuid.m_Guid == guid)
                    {
                        MelonLogger.Warning($"[MapManager] DupeFix: Blocked duplicate MapDetail GUID={guid} LocID={mapDetail.m_LocID}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[MapManager] DupeFix error: {ex.Message}");
            }
            return true;
        }
    }
}
