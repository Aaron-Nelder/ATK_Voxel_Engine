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
                    return _tickRateSettings = Resources.Load<TickRates_SO>(EngineConstants.ENGINE_SETTINGS_PATH + "Tick Rates");
                return _tickRateSettings;
            }
        }

        //Atlas
        static Dictionary<VoxelType, VoxelData_SO> _voxelAtlas = null;
        public static Dictionary<VoxelType, VoxelData_SO> VoxelAtlas
        {
            get
            {
                if (_voxelAtlas is null)
                    _voxelAtlas = Resources.LoadAll<VoxelData_SO>(EngineConstants.VOXEL_DATA_PATH).ToDictionary(x => x.Id);
                return _voxelAtlas;
            }
        }

        static Dictionary<BiomeType, BiomeData_SO> _biomeAtlas = null;
        public static Dictionary<BiomeType, BiomeData_SO> BiomeAtlas
        {
            get
            {
                if (_biomeAtlas is null)
                    _biomeAtlas = Resources.LoadAll<BiomeData_SO>(EngineConstants.BIOME_DATA_PATH).ToDictionary(x => x.BiomeType);
                return _biomeAtlas;
            }
        }

        static Dictionary<FolliageType, FolliageData_SO> _folliageAtlas = null;
        public static Dictionary<FolliageType, FolliageData_SO> FolliageAtlas
        {
            get
            {
                if (_folliageAtlas is null)
                    _folliageAtlas = Resources.LoadAll<FolliageData_SO>(EngineConstants.FOLLIAGE_DATA_PATH).ToDictionary(x => x.Type);
                return _folliageAtlas;
            }
        }

        public static bool GatherSO()
        {
            _worldSettings = Resources.Load<WorldSettings_SO>(EngineConstants.ENGINE_SETTINGS_PATH + "World Settings");
            _debugSettings = Resources.Load<DebugSettings_SO>(EngineConstants.ENGINE_SETTINGS_PATH + "Debug Settings");
            _tickRateSettings = Resources.Load<TickRates_SO>(EngineConstants.ENGINE_SETTINGS_PATH + "Tick Rates");
            _voxelAtlas = Resources.LoadAll<VoxelData_SO>(EngineConstants.VOXEL_DATA_PATH).ToDictionary(x => x.Id);
            _biomeAtlas = Resources.LoadAll<BiomeData_SO>(EngineConstants.BIOME_DATA_PATH).ToDictionary(x => x.BiomeType);
            _folliageAtlas = Resources.LoadAll<FolliageData_SO>(EngineConstants.FOLLIAGE_DATA_PATH).ToDictionary(x => x.Type);
            return WorldSettings is not null &&
                DebugSettings is not null &&
                VoxelAtlas is not null &&
                _tickRateSettings != null &&
                _biomeAtlas != null &&
                _folliageAtlas != null;
        }

        public static VoxelData_SO GetVoxelData(uint id) => id == 0 ? null : VoxelAtlas[(VoxelType)id];
        public static VoxelData_SO GetVoxelData(VoxelType id) => id == 0 ? null : VoxelAtlas[id];
        public static BiomeData_SO GetBiomeData(BiomeType type) => type == BiomeType.NULL ? null : BiomeAtlas[type];
        public static FolliageData_SO GetFolliageData(FolliageType type) => type == FolliageType.NULL ? null : FolliageAtlas[type];
    }
}
