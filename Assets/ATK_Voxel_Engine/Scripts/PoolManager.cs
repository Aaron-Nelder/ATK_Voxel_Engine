using System;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    [SerializeField] Poolable _chunkPoolable;
    public ObjectPool<GameObject> ChunkOBJPool;

    public void SetupPools()
    {
        ChunkOBJPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(_chunkPoolable.prefab),
            actionOnGet: obj => obj.gameObject.SetActive(true),
            actionOnRelease: obj => obj.gameObject.SetActive(false),
            actionOnDestroy: obj => Destroy(obj.gameObject),
            collectionCheck: false,
            defaultCapacity: _chunkPoolable.initialCapacity,
            maxSize: _chunkPoolable.maxSize
        );
    }
}

[Serializable]
public struct Poolable
{
    public GameObject prefab;
    public int initialCapacity;
    public int maxSize;
}