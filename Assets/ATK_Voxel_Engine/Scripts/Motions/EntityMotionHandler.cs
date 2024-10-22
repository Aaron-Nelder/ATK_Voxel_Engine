using UnityEngine;
using UnityEngine.InputSystem;
using ATKVoxelEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(EntityMotionManager))]
[RequireComponent(typeof(PlayerInput))]
public class EntityMotionHandler : MonoBehaviour
{
    [field: SerializeField] public CharacterController Controller { get; private set; }
    [field: SerializeField] public EntityMotionManager Manager { get; private set; }
    [field: SerializeField] public StaminaController StamController { get; private set; }
    [field: SerializeField] public PlayerInput Input { get; private set; }
    [field: SerializeField] public MotionStats_SO Stats { get; private set; }
    public EntityStats_SO EntityStats { get; private set; }

    public Vector3 Velocity { get; set; }

    public void Init(EntityStats_SO stats)
    {
        EntityStats = stats;
        Controller.height = stats.Height;
        StamController = new StaminaController(stats.StaminaStats);
        Manager.Init(this, true);
    }

    void Update() => Move(Velocity);

    public void Move(Vector3 movePoint) => Controller.Move(movePoint * Time.deltaTime);

    public bool IsGrounded(out RaycastHit[] hits)
    {
        float halfHeight = Controller.height * 0.5f;
        Vector3 start = -transform.up * Mathf.Abs(halfHeight - Controller.radius);
        hits = Physics.SphereCastAll(transform.position + start, Controller.radius, -transform.up, Stats.SkinWidth, Stats.GroundedLayers, QueryTriggerInteraction.Ignore);
        return hits.Length > 0;
    }

    public bool IsGrounded()
    {
        float halfHeight = Controller.height * 0.5f;
        Vector3 start = -transform.up * Mathf.Abs(halfHeight - Controller.radius);
        return Physics.SphereCastAll(transform.position + start, Controller.radius, -transform.up, Stats.SkinWidth, Stats.GroundedLayers, QueryTriggerInteraction.Ignore).Length > 0;
    }

    public bool HitHead(out RaycastHit[] hits)
    {
        float halfHeight = Controller.height * 0.5f;
        Vector3 start = transform.up * Mathf.Abs(halfHeight - Controller.radius);
        hits = Physics.SphereCastAll(transform.position + start, Controller.radius, transform.up, Stats.SkinWidth, Stats.GroundedLayers, QueryTriggerInteraction.Ignore);
        return hits.Length > 0;
    }

    public bool HitHead()
    {
        float halfHeight = Controller.height * 0.5f;
        Vector3 start = transform.up * Mathf.Abs(halfHeight - Controller.radius);
        return Physics.SphereCastAll(transform.position + start, Controller.radius, transform.up, Stats.SkinWidth, Stats.GroundedLayers, QueryTriggerInteraction.Ignore).Length > 0;
    }

    void OnDrawGizmos()
    {
        if (Stats is null) return;

        float halfHeight = Controller.height * 0.5f;

        Vector3 groundStart = -transform.up * Mathf.Abs(halfHeight - Controller.radius);
        RaycastHit[] groundHits = Physics.SphereCastAll(transform.position + groundStart, Controller.radius, -transform.up, Stats.SkinWidth, Stats.GroundedLayers, QueryTriggerInteraction.Ignore);
        Gizmos.color = groundHits.Length > 0 ? Color.red : Color.green;
        Gizmos.DrawWireSphere(groundStart + transform.position - (transform.up * Stats.SkinWidth), Controller.radius);

        Vector3 headStart = transform.up * Mathf.Abs(halfHeight - Controller.radius);
        RaycastHit[] headHits = Physics.SphereCastAll(transform.position + headStart, Controller.radius, transform.up, Stats.SkinWidth, Stats.GroundedLayers, QueryTriggerInteraction.Ignore);
        Gizmos.color = headHits.Length > 0 ? Color.red : Color.green;
        Gizmos.DrawWireSphere(headStart + transform.position + (transform.up * Stats.SkinWidth), Controller.radius);
    }
}
