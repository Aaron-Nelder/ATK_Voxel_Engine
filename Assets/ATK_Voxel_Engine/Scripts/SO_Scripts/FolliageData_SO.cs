using Unity.Mathematics;
using UnityEngine;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "Voxel Data", menuName = EngineConstants.ENGINE_NAME + "/Folliage Data")]
    public class FolliageData_SO : ScriptableObject
    {
        [SerializeField] FolliageType _type;
        public FolliageType Type => _type;

        [SerializeField] VoxelType[] _placeableVoxels;
        public VoxelType[] PlaceableVoxels => _placeableVoxels;

        [SerializeField] VoxelStructure[] _folliageStructures;
        public VoxelStructure[] FolliageStructures => _folliageStructures;

        public void UpdateStructures(VoxelStructure[] newStructures)
        {
            _folliageStructures = newStructures;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        public VoxelStructure GetRandomFolliage(ref Unity.Mathematics.Random rng)
        {
            int randomIndex = rng.NextInt(_folliageStructures.Length);
            return _folliageStructures[randomIndex];
        }
    }
}
