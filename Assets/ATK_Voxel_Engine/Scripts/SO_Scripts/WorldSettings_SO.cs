using UnityEngine;

[CreateAssetMenu(fileName = "World_Settings", menuName = DebugHelper.MENU_NAME + "/World_Settings", order = 1)]
public class WorldSettings_SO : ScriptableObject
{
    public GameObject chunkPrefab;

    public int chunkSize = 16;
    public int renderDistanceInChunks = 4;
    public int worldHeight = 64;
    public int seed = 0;
    public float chunkTickRate = 0;

    [SerializeField] NoiseProfile_SO heightNoise;
    public NoiseProfile_SO HeightNoise => heightNoise;
}
