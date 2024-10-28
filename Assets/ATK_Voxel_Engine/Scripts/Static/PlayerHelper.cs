using UnityEngine;
using ATKVoxelEngine;

public static class PlayerHelper
{
    const string REF_WARINGING = "Player or PlayerCC is null Returning (0,0)";
    
    public static void SnapPlayerToSurface(float startY = 500)
    {
        if (PlayerManager.Instance is null) return;

        Vector3 playerPosition = PlayerManager.Instance.transform.position;
        playerPosition.y = startY;
        
        float playerHeight = PlayerManager.Instance.MotionHandler.Controller.height * 0.5f;
        PlayerManager.Instance.MotionHandler.Controller.enabled = false;

        if (Physics.Raycast(playerPosition, Vector3.down, out RaycastHit hit, Mathf.Infinity))
        {
            playerPosition = new Vector3(playerPosition.x, hit.point.y + playerHeight, playerPosition.z);
            PlayerManager.Instance.transform.position = playerPosition;
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

            ChunkPosition position = new ChunkPosition();
            position.x = Mathf.FloorToInt(PlayerManager.Instance.MotionHandler.transform.position.x / EngineSettings.WorldSettings.ChunkSize.x);
            position.z = Mathf.FloorToInt(PlayerManager.Instance.MotionHandler.transform.position.z / EngineSettings.WorldSettings.ChunkSize.z);
            return position;
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
