using UnityEngine;

[CreateAssetMenu(fileName = "StaminaStats", menuName = DebugHelper.MENU_NAME + "/Noise Profile")]
public class NoiseProfile_SO : ScriptableObject
{
    [SerializeField] float scale = 200.0f;
    public float Scale => scale;

    [SerializeField] float amplitude = 1.0f;
    public float Amplitude => amplitude;

    [SerializeField] float frequency = 0.01f;
    public float Frequency => frequency;

    [SerializeField] int octaves = 4;
    public int Octaves => octaves;

    [SerializeField] float lacunarity = 2.0f;
    public float Lacunarity => lacunarity;

    [SerializeField] float persistence = 0.5f;
    public float Persistence => persistence;

    [SerializeField] int magClamp = 1;
    public int MagClamp => magClamp;
}
