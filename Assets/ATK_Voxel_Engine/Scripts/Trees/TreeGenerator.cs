using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;

namespace ATKVoxelEngine
{
    public class TreeGenerator : MonoBehaviour
    {
        [SerializeField] FolliageData_SO _folliageData;

        [ContextMenu("Read Data")]
        public void ReadData()
        {
            VoxelStructure[] newStructure;
            if (_folliageData.FolliageStructures.Length != 0)
            {
                newStructure = new VoxelStructure[_folliageData.FolliageStructures.Length + 1];
                _folliageData.FolliageStructures.CopyTo(newStructure, 0);
            }
            else
            {
                newStructure = new VoxelStructure[1];
            }

            List<int3> positions = new List<int3>();
            List<VoxelType> types = new List<VoxelType>();

            foreach (Transform t in transform)
            {
                try
                {
                    VoxelType type = VoxelType.Parse<VoxelType>(t.name);
                    positions.Add(new int3((int)t.localPosition.x, (int)t.localPosition.y, (int)t.localPosition.z));
                    types.Add(type);
                }
                catch
                {
                    Debug.LogError($"Invalid Voxel Type:{t.name}");
                    continue;
                }
            }

            newStructure[newStructure.Length - 1] = new VoxelStructure(positions.ToArray(), types.ToArray());

            _folliageData.UpdateStructures(newStructure);
        }
    }
}