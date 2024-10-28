using UnityEngine;

namespace ATKVoxelEngine
{
    public class CullingManager : MonoBehaviour
    {
        [SerializeField] int tickRate = 10;
        int tick = 0;

        public bool runOnce = false;
        public bool alwaysRun = false;

        void Update()
        {
            tick++;

            if (tick < tickRate) return;

            if (runOnce || alwaysRun)
            {
                CullChunksCPU();
            }
            runOnce = false;
            tick = 0;
        }

        void CullChunksCPU()
        {
            UnityEngine.Plane[] planes = GeometryUtility.CalculateFrustumPlanes(PlayerManager.Instance.PlayerCamera.Camera);

            foreach (var b in ChunkManager.Chunks)
            {
                bool render = GeometryUtility.TestPlanesAABB(planes, b.Value.Bounds);
                b.Value.Renderer.enabled = render;
            }
        }
    }
}