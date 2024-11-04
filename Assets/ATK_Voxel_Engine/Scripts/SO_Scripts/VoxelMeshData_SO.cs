using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System.Runtime.InteropServices;
using System.Linq;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "New_CustomMesh", menuName = EngineConstants.ENGINE_NAME + "/ New_CustomMesh")]
    public class VoxelMeshData_SO : ScriptableObject
    {
        public MeshPlane[] MeshPlanes;
        public bool usesCollisions;
        public bool isTransparent;
        public Material material;
        public Mesh Mesh;

        [SerializeField] Vertex[] _vertexData;
        [SerializeField] uint[] _indexData;

        public Vertex[] GetVertices()
        {
            Vertex[] vertices = new Vertex[MeshPlanes.Length * MeshPlane.VERTEX_COUNT];
            for (int i = 0; i < MeshPlanes.Length; i++)
                for (int j = 0; j < MeshPlane.VERTEX_COUNT; j++)
                    vertices[i * MeshPlane.VERTEX_COUNT + j] = MeshPlanes[i].Vertices[j];

            return vertices;
        }

        public uint[] GetIndices()
        {
            uint[] indices = new uint[MeshPlanes.Length * MeshPlane.INDEX_COUNT];
            uint vOffset = 0;
            for (int i = 0; i < MeshPlanes.Length; i++)
            {
                for (int j = 0; j < MeshPlane.INDEX_COUNT; j++)
                    indices[i * MeshPlane.INDEX_COUNT + j] = MeshPlane.Indices[j] + vOffset;

                vOffset += MeshPlane.VERTEX_COUNT;
            }

            return indices;
        }

        [ContextMenu("Save Mesh")]
        public void SaveMesh()
        {
            NativeArray<float3> Vertices = new NativeArray<float3>(MeshPlanes.Length * 4, Allocator.Temp);
            NativeArray<uint> Indices = new NativeArray<uint>(MeshPlanes.Length * 6, Allocator.Temp);
            NativeArray<float3> Normals = new NativeArray<float3>(MeshPlanes.Length * 4, Allocator.Temp);
            NativeArray<float2> UVS = new NativeArray<float2>(MeshPlanes.Length * 4, Allocator.Temp);

            uint vOffset = 0;
            for (int i = 0; i < MeshPlanes.Length; i++)
            {
                for (int j = 0; j < MeshPlane.INDEX_COUNT; j++)
                    Indices[i * MeshPlane.INDEX_COUNT + j] = MeshPlane.Indices[j] + vOffset;

                vOffset += MeshPlane.VERTEX_COUNT;

                for (int j = 0; j < MeshPlane.VERTEX_COUNT; j++)
                {
                    Vertices[i * MeshPlane.VERTEX_COUNT + j] = MeshPlanes[i].Vertices[j].position;
                    Normals[i * MeshPlane.VERTEX_COUNT + j] = MeshPlanes[i].Vertices[j].normal;
                    UVS[i * MeshPlane.VERTEX_COUNT + j] = MeshPlanes[i].texture.uv[j];
                }
            }

            Mesh = new Mesh();
            Mesh.name = "New Mesh";
            Mesh.SetVertices(Vertices);
            Mesh.SetIndices(Indices, MeshTopology.Triangles, 0);
            Mesh.SetNormals(Normals);
            Mesh.SetUVs(0, UVS);
            Mesh.RecalculateBounds();
            Mesh.RecalculateTangents();

            Vertices.Dispose();
            Indices.Dispose();
            Normals.Dispose();
            UVS.Dispose();

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(Mesh, "Assets/ATK_Voxel_Engine/Meshes/NewMesh.asset");
#endif
        }

        [ContextMenu("Bake Data")]
        public void BakeData()
        {
            foreach (var plane in MeshPlanes)
            {
                Vector2[] uvs = plane.texture.uv;
                for (int i = 0; i < uvs.Length; i++)
                    plane.Vertices[i].texCoord0 = uvs[i];
            }

            _vertexData = GetVertices();
            _indexData = GetIndices();
        }

        public Vertex[] GetPlanes(float3 normal)
        {
            int vertexCount = 0;

            // Gets the number of verticies that have the same normal
            foreach (var plane in MeshPlanes)
                for (int i = 0; i < MeshPlane.VERTEX_COUNT; i++)
                    if (plane.Vertices[i].normal.Equals(normal))
                        vertexCount++;

            Vertex[] vertices = new Vertex[vertexCount * MeshPlane.VERTEX_COUNT];

            int vIndex = 0;
            for (int i = 0; i < MeshPlanes.Length; i++)
                if (MeshPlanes[i].Vertices[0].normal.Equals(normal))
                    for (int j = 0; j < MeshPlane.INDEX_COUNT; j++)
                        if (j < MeshPlane.VERTEX_COUNT)
                            vertices[vIndex++] = MeshPlanes[i].Vertices[j];
            return vertices;
        }

        // Get the vertices and indices from the mesh planes that are visible and applies the position offset
        public void GetVisiblePlanes(int visibleSides, int3 vPos, out Vertex[] vertices)
        {
            int[] visiblePlanes = GetVisiblePlaneIndecies(visibleSides);

            vertices = new Vertex[visiblePlanes.Length * MeshPlane.VERTEX_COUNT];

            int vIndex = 0;
            for (int i = 0; i < visiblePlanes.Length; i++)
            {
                if (visiblePlanes[i] == -1) // continue if the planes isn't visible
                    continue;

                for (int j = 0; j < MeshPlane.INDEX_COUNT; j++)
                {
                    if (j < MeshPlane.VERTEX_COUNT)
                    {
                        vertices[vIndex] = MeshPlanes[i].Vertices[j];
                        vertices[vIndex++].position += (float3)vPos;
                    }
                }
            }
        }

        bool IsPlaneVisible(int visibleSides, int planeIndex, float3 direction)
        {
            return WorldHelper.IsBitSet(visibleSides, WorldHelper.DirectionToInt(direction)) && MeshPlanes[planeIndex].Vertices[0].normal.Equals(direction);
        }

        // returns and array of plane indecies that are visible, -1 if the plane is not visible
        int[] GetVisiblePlaneIndecies(int visibleSides)
        {
            int[] indecies = new int[MeshPlanes.Length];

            for (int i = 0; i < MeshPlanes.Length; i++)
            {
                if (IsPlaneVisible(visibleSides, i, WorldHelper.FaceToVec(Face.TOP)))
                    indecies[i] = 0;
                else if (IsPlaneVisible(visibleSides, i, WorldHelper.FaceToVec(Face.BOTTOM)))
                    indecies[i] = 1;
                else if (IsPlaneVisible(visibleSides, i, WorldHelper.FaceToVec(Face.LEFT)))
                    indecies[i] = 2;
                else if (IsPlaneVisible(visibleSides, i, WorldHelper.FaceToVec(Face.RIGHT)))
                    indecies[i] = 3;
                else if (IsPlaneVisible(visibleSides, i, WorldHelper.FaceToVec(Face.FRONT)))
                    indecies[i] = 4;
                else if (IsPlaneVisible(visibleSides, i, WorldHelper.FaceToVec(Face.BACK)))
                    indecies[i] = 5;
                else
                    indecies[i] = -1;
            }

            return indecies;
        }
    }

    [System.Serializable]
    public struct MeshPlane
    {
        public const byte VERTEX_COUNT = 4;
        public const byte INDEX_COUNT = 6;

        public Vertex[] Vertices;
        public static readonly uint[] Indices = new uint[] { 2, 0, 1, 2, 1, 3 };
        public Sprite texture;

        public MeshPlane(Vertex[] Vertices, Sprite texture)
        {
            this.Vertices = Vertices;
            this.texture = texture;
        }
    }

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public float3 position, normal;
        public float2 texCoord0;

        public Vertex(float3 position, float3 normal, float2 texCoord0)
        {
            this.position = position;
            this.normal = normal;
            this.texCoord0 = texCoord0;
        }
    }
}
