using UnityEngine;
using System;
using Unity.Mathematics;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "Biome Data", menuName = EngineConstants.ENGINE_NAME + "/Biome Data")]
    public class BiomeData_SO : ScriptableObject
    {
        [SerializeField] BiomeType _biomeType;
        public BiomeType BiomeType => _biomeType;

        [SerializeField] BiomeSize _biomeSize;
        public BiomeSize BiomeSize => _biomeSize;

        [SerializeField] Habitability _habitability;
        public Habitability Habitability => _habitability;

        [SerializeField] Hostility _hostility;
        public Hostility Hostility => _hostility;

        [SerializeField] Folliage[] _folliage;
        public Folliage[] folliages => _folliage;

        [SerializeField] VoxelType _surfaceVoxel;
        public VoxelType SurfaceVoxel => _surfaceVoxel;

        [SerializeField] VoxelType _subSurfaceVoxel;
        public VoxelType SubSurfaceVoxel => _subSurfaceVoxel;

        #region Spawn Parameters
        [SerializeField] float2 _elevation;
        public Vector2 Elevation => _elevation;

        [SerializeField] float2 _temperature;
        public Vector2 Temperature => _temperature;

        [SerializeField] float2 _humidity;
        public Vector2 Humidity => _humidity;
        #endregion

        [SerializeField] NoiseProfile_SO _heightNoise;
        public NoiseProfile_SO HeightNoise => _heightNoise;

        [SerializeField] NoiseProfile_SO _caveNoise;
        public NoiseProfile_SO CaveNoise => _caveNoise;

        [SerializeField] NoiseProfile_SO _folliageNoise;
        public NoiseProfile_SO FolliageNoise => _folliageNoise;
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