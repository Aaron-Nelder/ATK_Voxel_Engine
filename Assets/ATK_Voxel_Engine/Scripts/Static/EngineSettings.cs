using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ATKVoxelEngine
{
    public static class EngineSettings
    {
        // Settings
        static WorldSettings_SO _worldSettings;
        public static WorldSettings_SO WorldSettings
        {
            get
            {
                if (_worldSettings is null)
                    return _worldSettings = Resources.Load<WorldSettings_SO>(EngineConstants.ENGINE_SETTINGS_PATH + "World Settings");
                return _worldSettings;
            }
        }
        static DebugSettings_SO _debugSettings;
        public static DebugSettings_SO DebugSettings
        {
            get
            {
                if (_debugSettings is null)
                    return _debugSettings = Resources.Load<DebugSettings_SO>(EngineConstants.ENGINE_SETTINGS_PATH + "Debug Settings");
                return _debugSettings;
            }
        }
        static TickRates_SO _tickRateSettings;
        public static TickRates_SO TickRateSettings
        {
            get
            {
                if (_tickRateSettings is null)
                    return _tickRateSettings = Resources.Load<TickRates_SO>(EngineConstants.ENGINE_SETTINGS_PATH + "Tick Rate Settings");
                return _tickRateSettings;
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

        public static bool GatherSO()
        {
            _worldSettings = Resources.Load<WorldSettings_SO>(EngineConstants.ENGINE_SETTINGS_PATH + "World Settings");
            _debugSettings = Resources.Load<DebugSettings_SO>(EngineConstants.ENGINE_SETTINGS_PATH + "Debug Settings");
            _tickRateSettings = Resources.Load<TickRates_SO>(EngineConstants.ENGINE_SETTINGS_PATH + "Tick Rate Settings");
            voxelAtlas = Resources.LoadAll<VoxelData_SO>(EngineConstants.VOXEL_DATA_PATH).ToDictionary(x => x.Id);
            return WorldSettings is not null && DebugSettings is not null && VoxelAtlas is not null && _tickRateSettings != null;
        }

        public static VoxelData_SO GetVoxelData(uint id) => id == 0 ? null : VoxelAtlas[id];
    }
}
