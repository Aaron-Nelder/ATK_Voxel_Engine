using UnityEngine;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "Combined_Material", menuName = EngineConstants.ENGINE_NAME + "/Combined_Material")]
    public class CombinedMaterial_SO : ScriptableObject
    {
        [SerializeField] Material material;
        public Material Material => material;

        [SerializeField] int gridSize;
        public int GridSize => gridSize;
    }
}
