using ATKVoxelEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ATKVoxelEngine
{
    public static class EngineSettings
    {
        // Settings
        static WorldSettings_SO worldSettings;
        public static WorldSettings_SO WorldSettings
        {
            get
            {
                if (worldSettings is null)
                    return worldSettings = Resources.Load<WorldSettings_SO>(EngineConstants.WORLD_SETTINGS_PATH);
                return worldSettings;
            }
        }
        static DebugSettings_SO debugSettings;
        public static DebugSettings_SO DebugSettings
        {
            get
            {
                if (debugSettings is null)
                    return debugSettings = Resources.Load<DebugSettings_SO>(EngineConstants.DEBUG_SETTINGS_PATH);
                return debugSettings;
            }
        }

        //Atlas
        static Dictionary<uint, VoxelData_SO> voxelAtlas = null;
        public static Dictionary<uint, VoxelData_SO> VoxelAtlas
        {
            get
            {
                if (voxelAtlas is null)
                    voxelAtlas = Resources.LoadAll<VoxelData_SO>(EngineConstants.VOXEL_DATA_PATH).ToDictionary(x => x.Id);
                return voxelAtlas;
            }
        }
        static CombinedMaterial_SO[] materialAtlas = null;
        public static CombinedMaterial_SO[] MaterialAtlas
        {
            get
            {
                if (materialAtlas is null)
                    materialAtlas = Resources.LoadAll<CombinedMaterial_SO>(EngineConstants.MATERIAL_DATA_PATH);
                return materialAtlas;
            }
        }

        public static bool GatherSO()
        {
            worldSettings = Resources.Load<WorldSettings_SO>(EngineConstants.WORLD_SETTINGS_PATH);
            debugSettings = Resources.Load<DebugSettings_SO>(EngineConstants.DEBUG_SETTINGS_PATH);
            voxelAtlas = Resources.LoadAll<VoxelData_SO>(EngineConstants.VOXEL_DATA_PATH).ToDictionary(x => x.Id);
            materialAtlas = Resources.LoadAll<CombinedMaterial_SO>(EngineConstants.MATERIAL_DATA_PATH);
            return WorldSettings is not null && DebugSettings is not null && VoxelAtlas is not null && MaterialAtlas is not null;
        }

        public static VoxelData_SO GetVoxelData(uint id) => id == 0 ? null : VoxelAtlas[id];
    }
}
