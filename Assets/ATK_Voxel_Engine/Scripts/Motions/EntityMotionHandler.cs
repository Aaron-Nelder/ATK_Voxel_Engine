using UnityEngine;
using UnityEngine.InputSystem;
using ATKVoxelEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(EntityMotionManager))]
[RequireComponent(typeof(PlayerInput))]
public class EntityMotionHandler : MonoBehaviour
{
    [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
    [field: SerializeField] public Transform Body { get; private set; }
    [field: SerializeField] public Transform Head { get; private set; }
    [field: SerializeField] public EntityMotionManager Manager { get; private set; }
    [field: SerializeField] public StaminaController StamController { get; private set; }
    [field: SerializeField] public PlayerInput Input { get; private set; }
    [field: SerializeField] public MotionStats_SO Stats { get; private set; }
    public EntityStats_SO EntityStats { get; private set; }

    [field: SerializeField] public BoxCollider Collider { get; private set; }

    public void Init(EntityStats_SO stats)
    {
        EntityStats = stats;
        SetupCollider();
        SetupRigidBody();
        StamController = new StaminaController(stats.StaminaStats);
        Manager.Init(this, true);
    }

    void SetupCollider()
    {
        Collider.size = Stats.Size;
        Collider.center = new Vector3(0, Stats.Size.y * 0.5f, 0);
    }

    void SetupRigidBody()
    {
        Rigidbody.mass = Stats.Mass;
        Rigidbody.linearDamping = Stats.Drag;
        Rigidbody.angularDamping = Stats.AngularDrag;
    }

    public bool IsGrounded
    {
        get
        {
            Vector3 center = (Vector3.down * (Collider.size.y * 0.5f)) + Collider.center + Rigidbody.position;
            Vector3 halfExtents = new((Stats.Size.x * 0.5f) - Stats.SkinWidth, Stats.SkinWidth, (Stats.Size.z * 0.5f) - Stats.SkinWidth);
            return Physics.CheckBox(center, halfExtents, Quaternion.identity, Stats.GroundedLayers, QueryTriggerInteraction.Ignore);
        }
    }

    public bool HitHead
    {
        get
        {
            Vector3 center = (Vector3.up * (Collider.size.y * 0.5f)) + Collider.center + Rigidbody.position;
            Vector3 halfExtents = new((Stats.Size.x * 0.5f) - Stats.SkinWidth, Stats.SkinWidth, (Stats.Size.z * 0.5f) - Stats.SkinWidth);
            return Physics.CheckBox(center, halfExtents, Quaternion.identity, Stats.GroundedLayers, QueryTriggerInteraction.Ignore);
        }
    }

    void OnDrawGizmos()
    {
        if (Stats is null) return;
        if (!DebugHelper.Debugging) return;

        // Draw the box cast to check if the player is grounded
        Vector3 center = (Vector3.down * (Collider.size.y * 0.5f)) + Collider.center + Rigidbody.position;
        Vector3 size = new(Stats.Size.x - (Stats.SkinWidth * 2), Stats.SkinWidth * 2, Stats.Size.z - (Stats.SkinWidth * 2));

        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(center, size);

        center = (Vector3.up * (Collider.size.y * 0.5f)) + Collider.center + Rigidbody.position;
        size = new(Stats.Size.x - (Stats.SkinWidth * 2), Stats.SkinWidth * 2, Stats.Size.z - (Stats.SkinWidth * 2));

        Gizmos.color = HitHead ? Color.green : Color.red;
        Gizmos.DrawWireCube(center, size);
    }
}
