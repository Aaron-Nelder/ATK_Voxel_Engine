using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Block_Data", menuName = DebugHelper.MENU_NAME + "/Block_Data")]
public class VoxelData_SO : ScriptableObject
{
    [SerializeField] uint id;
    public uint Id => id;
    [SerializeField] new string name;
    public string Name => name;
    [SerializeField] bool isSolid;
    public bool IsSolid => isSolid;
    [SerializeField] uint materialIndex;
    public uint MaterialIndex => materialIndex;
    [SerializeField] Vector2Int textureIndex = new();
    public Vector2Int TextureIndex => textureIndex;
    [SerializeField] CustomMesh_SO meshData;
    public CustomMesh_SO MeshData => meshData;

    public List<Vector2> ScaledUVs(List<Vector2> uvs)
    {
        // scale the UVs to the correct texture       
        CombinedMaterial_SO mat = VoxelManager.MaterialAtlas[materialIndex];
        List<Vector2> scaledUVS = new List<Vector2>();
        for (int i = 0; i < uvs.Count; i++)
        {
            float x = 0;
            float y = 0;
            x = (uvs[i].x / mat.GridSize) + (textureIndex.x * (1f / mat.GridSize));
            y = (uvs[i].y / mat.GridSize) + (textureIndex.y * (1f / mat.GridSize));
            scaledUVS.Add(new Vector2(x, y));
        }
        return scaledUVS;
    }
}
