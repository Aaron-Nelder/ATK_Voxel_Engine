using UnityEngine;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "Motion Stats", menuName = EngineConstants.ENGINE_NAME + "/Motion/Motion Stats")]
    public class MotionStats_SO : ScriptableObject
    {
        [SerializeField] float _mass = 62;
        public float Mass => _mass;

        [SerializeField] float _drag = 1;
        public float Drag => _drag;

        [SerializeField] float _angularDrag = 0.05f;
        public float AngularDrag => _angularDrag;

        [SerializeField] LayerMask _groundedLayers;
        public LayerMask GroundedLayers => _groundedLayers;

        [SerializeField] float _skinWidth = 0.1f;
        public float SkinWidth => _skinWidth;

        [SerializeField] float _moveSpeedMul = 1;
        public float MoveSpeedMul => _moveSpeedMul;

        [SerializeField] Vector3 _size = new(1.0f,2.0f,1.0f);
        public Vector3 Size => _size;
    }
}