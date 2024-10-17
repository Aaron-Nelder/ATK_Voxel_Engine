using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelManager : MonoBehaviour
{
    const string WORLD_SETTINGS_PATH = "Data/Settings/World Settings";
    const string DEBUG_SETTINGS_PATH = "Data/Settings/Debug Settings";
    const string VOXEL_DATA_PATH = "Data/BlockData/";
    const string MATERIAL_DATA_PATH = "Data/MaterialData/";
    public static VoxelManager Instance { get; private set; }

    // Settings
    static WorldSettings_SO worldSettings;
    public static WorldSettings_SO WorldSettings
    {
        get
        {
            if (worldSettings is null)
                return worldSettings = Resources.Load<WorldSettings_SO>(WORLD_SETTINGS_PATH);
            return worldSettings;
        }
    }
    static DebugSettings_SO debugSettings;
    public static DebugSettings_SO DebugSettings
    {
        get
        {
            if (debugSettings is null)
                return debugSettings = Resources.Load<DebugSettings_SO>(DEBUG_SETTINGS_PATH);
            return debugSettings;
        }
    }

    //Atlas
    static Dictionary<uint, VoxelData_SO> voxelAtlas = null;
    public static Dictionary<uint, VoxelData_SO> VoxelAtlas
    {
        get
        {
            if (voxelAtlas is null)
                voxelAtlas = Resources.LoadAll<VoxelData_SO>(VOXEL_DATA_PATH).ToDictionary(x => x.Id);

            return voxelAtlas;
        }
    }

    static CombinedMaterial_SO[] materialAtlas = null;
    public static CombinedMaterial_SO[] MaterialAtlas
    {
        get
        {
            if (materialAtlas is null)
                materialAtlas = Resources.LoadAll<CombinedMaterial_SO>(MATERIAL_DATA_PATH);

            return materialAtlas;
        }
    }

    public static Action OnChunkTick;

    static bool GatherSO()
    {
        worldSettings = Resources.Load<WorldSettings_SO>(WORLD_SETTINGS_PATH);
        debugSettings = Resources.Load<DebugSettings_SO>(DEBUG_SETTINGS_PATH);
        voxelAtlas = Resources.LoadAll<VoxelData_SO>(VOXEL_DATA_PATH).ToDictionary(x => x.Id);
        materialAtlas = Resources.LoadAll<CombinedMaterial_SO>(MATERIAL_DATA_PATH);
        return WorldSettings is not null && DebugSettings is not null && VoxelAtlas is not null && MaterialAtlas is not null;
    }

    static float chunkTick = 0;

    [SerializeField] ScreenLogger screenLogger;

    void Awake()
    {
        Instance = this;
        screenLogger.Init();

        if (!GatherSO())
            return;

        ChunkManager.SpawnStartingChunks(transform, false);
        OnChunkTick?.Invoke();
        SetPlayerSpawnPoint();

        new Selector();
    }

    static void SetPlayerSpawnPoint()
    {
        int halfChunkSize = WorldSettings.chunkSize / 2;
        Vector3 newPos = new Vector3(halfChunkSize, 0, halfChunkSize);
        PlayerManager.Instance.transform.position = newPos;
        PlayerHelper.SnapPlayerToSurface();
    }

    // GlobalTickSystem
    void FixedUpdate()
    {
        ChunkTick();

        if (ScreenLogger.Instance == null) return;
    }

    void ChunkTick()
    {
        chunkTick += Time.deltaTime;

        if (chunkTick >= worldSettings.chunkTickRate)
        {
            OnChunkTick?.Invoke();
            chunkTick = 0;
        }
    }

    public static VoxelData_SO GetVoxelData(uint id)
    {
        if (id == 0) return null;

        return VoxelAtlas[id];
    }
}
