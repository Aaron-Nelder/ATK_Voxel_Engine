using UnityEngine;

[CreateAssetMenu(fileName = "StaminaStats", menuName = DebugHelper.MENU_NAME + "/Motion/Stamina Stats")]
public class StaminaStats_SO : ScriptableObject
{
    [SerializeField] uint _max = 100;
    public uint MaxStamina => _max;

    [SerializeField] uint _starting = 100;
    public uint Starting => _starting;

    [SerializeField] float _tickRate = 1;
    public float TickRate => _tickRate;

    [SerializeField] float _regenPerTick = 10;
    public float RegenPerTick => _regenPerTick;

    [SerializeField] float _regenDelay = 5;
    public float RegenDelay => _regenDelay;
}