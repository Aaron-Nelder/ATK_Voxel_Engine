using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;
using System;

namespace ATKVoxelEngine
{
    public class VoxelVisTest : MonoBehaviour
    {
        [SerializeField] ComputeShader _comShader;
        [SerializeField] int3 chunkSize;
        uint[] voxelsFlat;
        static bool IsBitSet(int num, int bitPos) => (num & (1 << bitPos)) != 0;

        void Start()
        {
            AssignVoxels();
            GetVisibleVoxels(voxelsFlat, chunkSize);
        }

        void DebugResult(int[] result)
        {
            for (int i = 0; i < result.Length; i++)
            {
                int visibleSides = result[i];
                if (visibleSides == 0) continue;        // if not visible, skip

                float3 voxelPosition = new float3(i % chunkSize.x, (i / chunkSize.x) % chunkSize.y, i / (chunkSize.x * chunkSize.y));
                GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = voxelPosition;

                string log = $"Voxel ID: ({voxelsFlat[i]}), Position: ({voxelPosition.x}, {voxelPosition.y}, {voxelPosition.z}) (";

                if (IsBitSet(visibleSides, 0))
                    log += "Top, ";
                if (IsBitSet(visibleSides, 1))
                    log += "Bottom, ";
                if (IsBitSet(visibleSides, 2))
                    log += "Left, ";
                if (IsBitSet(visibleSides, 3))
                    log += "Right, ";
                if (IsBitSet(visibleSides, 4))
                    log += "Front, ";
                if (IsBitSet(visibleSides, 5))
                    log += "Back, ";

                Debug.Log($"{log})");
            }

        }

        // TODO:: fills the voxelsFlat array dummy data
        void AssignVoxels()
        {
            int arraySize = chunkSize.x * chunkSize.y * chunkSize.z;
            voxelsFlat = new uint[arraySize];

            for (int x = 0; x < chunkSize.x; x++)
            {
                for (int y = 0; y < chunkSize.y; y++)
                {
                    for (int z = 0; z < chunkSize.z; z++)
                    {
                        int index = x + (y * chunkSize.x) + (z * chunkSize.y * chunkSize.z);
                        voxelsFlat[index] = 1;
                    }
                }
            }
        }

        /// <summary>
        /// Computes the visibility of voxels in a chunk using a compute shader and returns the results as an array of integers 
        /// where each integer represents the visibility of a voxel. The visibility is represented as a 6-bit integer where each bit
        /// if set to 1 represents a visible side of the voxel. The order of the bits is as follows: Top, Bottom, Left, Right, Front, Back.
        /// </summary>
        /// <param name="voxelsFlat"> The array of voxels flattened to a 1D array </param>
        /// <param name="chunkSize">  The dimensions of the chunk  </param>
        /// <param name="result">  A 1D array of integers with the first 6-Bits represending the visible faces: Top, Bottom, Left, Right, Front, Back. 1 is visible, 0 is not</param>
        void GetVisibleVoxels(uint[] voxelsFlat, int3 chunkSize, Action<int[]> result = null)
        {
            int kernel = _comShader.FindKernel("CSMain");

            int[] visibilityResults = new int[voxelsFlat.Length];                       // Initialize visibility results

            ComputeBuffer voxelIdBuffer = new ComputeBuffer(voxelsFlat.Length, sizeof(uint));         // Create compute buffers
            ComputeBuffer visibilityBuffer = new ComputeBuffer(voxelsFlat.Length, sizeof(int));

            voxelIdBuffer.SetData(voxelsFlat);                                          // Set data to buffers
            visibilityBuffer.SetData(visibilityResults);

            _comShader.SetBuffer(0, "VoxelIds", voxelIdBuffer);                         // Set buffers and parameters to compute shader
            _comShader.SetBuffer(0, "VisResults", visibilityBuffer);
            _comShader.SetInts("ChunkSize", chunkSize.x, chunkSize.y, chunkSize.z);

            int threadGroupX = Mathf.CeilToInt((float)chunkSize.x / 8);                 // Calculate thread group sizes
            int threadGroupY = Mathf.CeilToInt((float)chunkSize.y / 8);
            int threadGroupZ = Mathf.CeilToInt((float)chunkSize.z / 8);

            CommandBuffer cmd = new CommandBuffer();                                    // Create a command buffer
            cmd.name = "Voxel Visibility Compute";

            cmd.DispatchCompute(_comShader, kernel, threadGroupX, threadGroupY, threadGroupZ); // Dispatch the compute shader

            Graphics.ExecuteCommandBuffer(cmd);                                         // Execute the command buffer

            cmd.Release();                                                              // Release the command buffer

            // Wait for the compute shader to finish
            AsyncGPUReadback.Request(visibilityBuffer, request =>
            {
                if (request.hasError)
                {
                    Debug.LogError("Error reading back visibility buffer.");
                    return;
                }

                visibilityResults = request.GetData<int>().ToArray();

                // Release buffers
                voxelIdBuffer.Release();
                visibilityBuffer.Release();

                result?.Invoke(visibilityResults);
            });
        }
    }
}
