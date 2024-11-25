using System;

namespace ATKVoxelEngine
{
    public enum TickType { UPDATE, FIXED_UPDATE, LATE_UPDATE, ENGINE, CHUNK, PLAYER, ENTITY, CHUNK_LOAD }

    public enum NoiseDimension : byte { TWO_D, THREE_D }
    public enum NoiseType { CLASSIC, PERLIN, PERLIN_ROTATION, SIMPLEX, SIMPLEX_ROTATION, CELLUAR }

    public enum BiomeType : uint { NULL, PLAINS, DESERT, MOUNTAINS, FOREST, SNOW, SWAMP, JUNGLE, OCEAN, RIVER, BEACH, VOLCANO, CANYON, RUINS, VILLAGE, MINE, SKY, VOID }
    public enum BiomeSize : uint { NULL = 0, SMALL = 8, MEDIUM = 16, LARGE = 32 }
    public enum FolliageDensity : uint { NULL = 0, LOW = 1, MEDIUM = 3, HIGH = 5 }
    public enum Habitability : uint { NULL = 0, LOW = 1, MILD = 2, FAIR = 3, HABITABLE = 4 }
    public enum FolliageType : UInt16 { NULL, OAK_TREE, PINE_TREE, BIRCH_TREE, RED_FLOWER, BLUE_FLOWER }
    public enum Hostility { NULL, PASSIVE, NEUTRAL, AGGRESSIVE }

    public enum PreviewType { WireCube, WireSphere, Line, DashLine, }

    // Just a simple struct to quickly represent a 3D Int Direction
    public enum Face : byte { NULL = 0x000, TOP = 0x001, BOTTOM = 0x002, LEFT = 0x003, RIGHT = 0x004, FRONT = 0x005, BACK = 0x006 }

    public enum VoxelType : uint { AIR, GRASS, DIRT, STONE, LEAF_OAK, LOG_OAK, SAND, SNOW }

    public enum ChunkState : ushort { NULL, IDLE, UPDATING, LOADED, UNLOADING, WAITING_FOR_NEIGHBOURS, DETERMINING_BIOME, GENERATING_SURFACE, GENERATING_CAVES, ASSIGNING_VOXELS, GENERATING_FOLLIAGE, LOADING_MESH }

    public static class EngineConstants
    {
        public const string ENGINE_NAME = "ATK Voxel Engine";
        public const string ENGINE_SETTINGS_PATH = "Data/Settings/";
        public const string VOXEL_DATA_PATH = "Data/VoxelData/";
        public const string MATERIAL_DATA_PATH = "Data/MaterialData/";
        public const string ENGINE_EDITOR_WINDOW_PATH = "Window/" + ENGINE_NAME + "/" + "Engine Editor";
        public const string MESHES_FOLDER_PATH = "Assets/ATK_Voxel_Engine/Meshes/";
        public const string BIOME_DATA_PATH = "Data/BiomeData/";
        public const string FOLLIAGE_DATA_PATH = "Data/FolliageData/";

        //Tags
        public const string CUSTOM_MESH_TAG = "CustomMesh";

        public const float DEBUG_LINE_OFFSET = 0.5f;

    }
}
