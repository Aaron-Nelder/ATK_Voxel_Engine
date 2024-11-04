using UnityEngine;
using ATKVoxelEngine;

public static class PlayerHelper
{
    const string REF_WARINGING = "Player or PlayerCC is null Returning (0,0)";
    
    public static void SnapPlayerToVoxel(Chunk chunk,int voxelX, int voxelZ)
    {
        if (PlayerManager.Instance is null) return;

        Vector3 playerPos = chunk.transform.position + new Vector3(voxelX, EngineSettings.WorldSettings.ChunkSize.y, voxelZ);
        PlayerManager.Instance.transform.position = playerPos;
        
        float playerHeight = PlayerManager.Instance.MotionHandler.Controller.height * 0.5f;
        PlayerManager.Instance.MotionHandler.Controller.enabled = false;

        for(int y = EngineSettings.WorldSettings.ChunkSize.y -1; y >0; y--)
        {
            if(chunk.GetVoxel(voxelX,y, voxelZ) != 0)
            {
                playerPos = new Vector3(playerPos.x, y + playerHeight + 0.5f, playerPos.z);
                PlayerManager.Instance.transform.position = playerPos;
                break;
            }
        }

        PlayerManager.Instance.MotionHandler.Controller.enabled = true;       
    }

    public static ChunkPosition PlayerChunk
    {
        get
        {
            if (PlayerManager.Instance is null || PlayerManager.Instance.MotionHandler is null)
            {
                Debug.LogWarning(REF_WARINGING);
                return new ChunkPosition(0, 0);
            }

            return WorldHelper.WorldToChunkPos(PlayerManager.Instance.transform.position);
        }
    }

    public static Vector3Int PlayerVoxelPosition
    {
        get
        {
            if (PlayerManager.Instance is null || PlayerManager.Instance.MotionHandler is null)
            {
                Debug.LogWarning(REF_WARINGING);
                return new Vector3Int(0, 0, 0);
            }

            return Vector3Int.FloorToInt(PlayerManager.Instance.MotionHandler.transform.position);
        }
    }
}
