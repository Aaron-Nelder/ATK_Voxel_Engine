using UnityEngine;
using ATKVoxelEngine;
using Unity.Mathematics;

public static class PlayerHelper
{
    const string REF_WARINGING = "PlayerManager.Instance is NULL";

    public static void SnapPlayerToVoxel(Chunk chunk, int voxelX, int voxelZ)
    {
        if (PlayerManager.Instance is null) return;

        Vector3 playerPos = chunk.transform.position + new Vector3(voxelX, EngineSettings.WorldSettings.ChunkSize.y - 1, voxelZ);

        PlayerManager.Instance.MotionHandler.Rigidbody.Sleep();

        for (int y = (int)playerPos.y; y > 0; y--)
        {
            if (chunk.Data.GetVoxel(voxelX, y, voxelZ) != 0)
            {
                PlayerManager.Instance.MotionHandler.Rigidbody.position = new Vector3(playerPos.x, y + 0.5f, playerPos.z);
                break;
            }
        }

        PlayerManager.Instance.MotionHandler.Rigidbody.WakeUp();
    }

    public static ChunkPosition PlayerChunk
    {
        get
        {
            if (PlayerManager.Instance is null)
            {
                Debug.LogWarning(REF_WARINGING);
                return new ChunkPosition(0, 0);
            }

            return EngineUtilities.WorldToChunkPos(PlayerManager.Instance.transform.position);
        }
    }

    public static int3 PlayerVoxelPosition
    {
        get
        {
            if (PlayerManager.Instance is null)
            {
                Debug.LogWarning(REF_WARINGING);
                return int3.zero;
            }
            float3 floatVal = math.round(PlayerManager.Instance.transform.position);
            return new int3((int)floatVal.x, (int)floatVal.y+1, (int)floatVal.z);
        }
    }
}
