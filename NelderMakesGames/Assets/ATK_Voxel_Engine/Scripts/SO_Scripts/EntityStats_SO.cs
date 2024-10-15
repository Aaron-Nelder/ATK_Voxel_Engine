using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = DebugHelper.MENU_NAME + "/Player_Stats", order = 1)]
public class EntityStats_SO : ScriptableObject
{
    [SerializeField] private StaminaStats_SO _staminaStats;
    public StaminaStats_SO StaminaStats => _staminaStats;

    [SerializeField] private MotionStats_SO _motion;
    public MotionStats_SO MotionStats => _motion;

    [SerializeField] float _breakSpeed = 1.0f;
    public float BreakSpeed => _breakSpeed;

    [SerializeField] float _placeSpeed = 1.0f;
    public float PlaceSpeed => _placeSpeed;

    [SerializeField] float _height = 2.0f;
    public float Height => _height;
}
