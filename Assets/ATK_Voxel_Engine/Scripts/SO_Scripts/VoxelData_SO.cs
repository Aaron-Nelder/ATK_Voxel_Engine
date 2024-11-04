using System.Collections.Generic;
using UnityEngine;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "Block_Data", menuName = EngineConstants.ENGINE_NAME + "/Block_Data")]
    public class VoxelData_SO : ScriptableObject
    {
        [SerializeField] uint _id;
        public uint Id => _id;

        [SerializeField] string _name;
        public string Name => _name;

        [SerializeField] uint _materialIndex;
        public uint MaterialIndex => _materialIndex;

        [SerializeField] Vector2Int _textureIndex = new();
        public Vector2Int TextureIndex => _textureIndex;

        [SerializeField] CustomMesh_SO _oldMeshData;
        public CustomMesh_SO OldMeshData => _oldMeshData;

        [SerializeField] VoxelMeshData_SO _meshData;
        public VoxelMeshData_SO MeshData => _meshData;

        public List<Vector2> ScaledUVs(List<Vector2> uvs)
        {
            // scale the UVs to the correct texture       
            CombinedMaterial_SO mat = EngineSettings.MaterialAtlas[_materialIndex];
            List<Vector2> scaledUVS = new List<Vector2>();
            for (int i = 0; i < uvs.Count; i++)
            {
                float x = 0;
                float y = 0;
                x = (uvs[i].x / mat.GridSize) + (_textureIndex.x * (1f / mat.GridSize));
                y = (uvs[i].y / mat.GridSize) + (_textureIndex.y * (1f / mat.GridSize));
                scaledUVS.Add(new Vector2(x, y));
            }
            return scaledUVS;
        }
    }
}