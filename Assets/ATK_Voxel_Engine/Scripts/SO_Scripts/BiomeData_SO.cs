using UnityEngine;
using System;

namespace ATKVoxelEngine
{
    public enum BiomeType { NULL, PLAINS, DESERT, MOUNTAINS, FOREST, SNOW, SWAMP, JUNGLE, OCEAN, RIVER, BEACH, VOLCANO, CANYON, RUINS, VILLAGE, MINE, SKY, VOID }
    public enum BiomeSize { NULL = 0, SMALL = 8, MEDIUM = 16, LARGE = 32 }
    public enum FolliageDensity { NULL = 0, LOW = 1, MEDIUM = 2, HIGH = 3 }
    public enum Habitability { NULL, LOW, MILD, FAIR, HABITABLE }
    public enum FolliageType { NULL, OAK_TREE, PINE_TREE, BIRCH_TREE, RED_FLOWER, BLUE_FLOWER }
    public enum Hostility { NULL, PASSIVE, NEUTRAL, AGGRESSIVE }

    [CreateAssetMenu(fileName = "Biome Data", menuName = EngineConstants.ENGINE_NAME + "/Biome Data")]
    public class BiomeData_SO : ScriptableObject
    {
        [SerializeField] BiomeType _biomeType;
        public BiomeType BiomeType => _biomeType;

        [SerializeField] BiomeSize _biomeSize;
        public BiomeSize BiomeSize => _biomeSize;

        [SerializeField] FolliageDensity _folliageDensity;
        public FolliageDensity FolliageDensity => _folliageDensity;

        [SerializeField] Habitability _habitability;
        public Habitability Habitability => _habitability;

        [SerializeField] Hostility _hostility;
        public Hostility Hostility => _hostility;

        [SerializeField] Folliage[] _folliage;
        public Folliage[] folliages => _folliage;

        #region Spawn Parameters
        [SerializeField] Vector2 _elevation;
        public Vector2 Elevation => _elevation;

        [SerializeField] Vector2 _temperature;
        public Vector2 Temperature => _temperature;

        [SerializeField] Vector2 _humidity;
        public Vector2 Humidity => _humidity;
        #endregion

        [SerializeField] NoiseProfile_SO _heightNoise;
        public NoiseProfile_SO HeightNoise => _heightNoise;

        [SerializeField] NoiseProfile_SO _caveNoise;
        public NoiseProfile_SO CaveNoise => _caveNoise;
    }

    [Serializable]
    public struct Folliage
    {
        public FolliageType type;
        public FolliageDensity density;

        public Folliage(FolliageType type, FolliageDensity density)
        {
            this.type = type;
            this.density = density;
        }
    }
}