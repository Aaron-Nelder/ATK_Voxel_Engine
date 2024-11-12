using UnityEngine;
using UnityEngine.InputSystem;

namespace ATKVoxelEngine
{
    public class RightHand : MonoBehaviour
    {
        [SerializeField] GameObject _heldVoxelPrefab;
        [SerializeField] Transform _heldAnchor;
        [SerializeField] Animator _animator;
        HeldVoxel _heldVoxel = null;

        // animator hashes
        int _breakingHash = Animator.StringToHash("IsLeftClick");

        bool _isLeftClick = false;
        bool _isRightClick = false;

        float _breakSpeed;
        float _breakTimer = 0.0f;
        float _placeSpeed;
        float _placeTimer = 0.0f;

        void Start()
        {
            //TODO:: Change this to a proper voxel / Inventory system
            _heldVoxel = Instantiate(_heldVoxelPrefab, _heldAnchor).GetComponent<HeldVoxel>().Init(_heldAnchor, EngineSettings.VoxelAtlas[2]);

            _breakSpeed = PlayerManager.Instance.Stats.BreakSpeed;
            _placeSpeed = PlayerManager.Instance.Stats.PlaceSpeed;

            new VoxelHighlight();
        }

        void Update()
        {
            if (_isLeftClick && Selector.HasSelection)
            {
                if (_breakTimer >= _breakSpeed)
                {
                    EngineUtilities.DestroyVoxel(EngineUtilities.LocalPosToWorldPos(Selector.SelectedVoxel.Chunk.Position, Selector.SelectedVoxel.LocalPosition));
                    _breakTimer = 0;
                }
                else
                    _breakTimer += Time.deltaTime;
            }
            else if (_isRightClick && Selector.HasSelection)
            {
                if (_placeTimer >= _placeSpeed)
                {
                    _animator.SetBool(_breakingHash, true);
                    EngineUtilities.PlaceVoxel(Selector.SelectedVoxel.NormalWorldPos, _heldVoxel.Id);
                    _placeTimer = 0;
                }
                else
                    _placeTimer += Time.deltaTime;
            }
            else if (_isRightClick && !Selector.HasSelection)
            {
                _animator.SetBool(_breakingHash, false);
                _placeTimer = _placeSpeed;
            }
        }

        void OnLeftClick(InputValue value)
        {
            _isLeftClick = value.isPressed;
            _breakTimer = _breakSpeed;

            if (_isLeftClick)
                _isRightClick = false;

            _animator.SetBool(_breakingHash, _isLeftClick);
        }

        void OnRightClick(InputValue value)
        {
            _isRightClick = value.isPressed;

            _placeTimer = _placeSpeed;

            if (_isRightClick)
                _isLeftClick = false;

            if (!_isRightClick)
                _animator.SetBool(_breakingHash, false);
        }
    }
}