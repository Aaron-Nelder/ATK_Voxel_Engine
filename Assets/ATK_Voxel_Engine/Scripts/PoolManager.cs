using System;
using UnityEngine;
using UnityEngine.Pool;
using Unity.Mathematics;

namespace ATKVoxelEngine
{
    public class PoolManager : MonoBehaviour
    {
        [SerializeField] Poolable _chunkPoolable;
        public ObjectPool<GameObject> ChunkOBJPool;

        public void SetupPools()
        {
            ushort renderDistance = EngineSettings.WorldSettings.RenderDistance;
            int defaultCapacity = (renderDistance * renderDistance) + renderDistance;
            int maxCapacity = defaultCapacity * 2;

            ChunkOBJPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_chunkPoolable.prefab),
                actionOnGet: obj => obj.gameObject.SetActive(true),
                actionOnRelease: obj => obj.gameObject.SetActive(false),
                actionOnDestroy: obj => Destroy(obj.gameObject),
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxCapacity
            );
        }
    }

    [Serializable]
    public struct Poolable
    {
        public GameObject prefab;
        //TODO: THESE DON'T DO ANYTHING FOR CHUNKS
        public int initialCapacity;
        public int maxSize;
    }
}