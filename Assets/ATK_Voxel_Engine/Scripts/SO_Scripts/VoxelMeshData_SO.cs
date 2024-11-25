using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System.Runtime.InteropServices;
using System;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "New_CustomMesh", menuName = EngineConstants.ENGINE_NAME + "/Custom Mesh")]
    public class VoxelMeshData_SO : ScriptableObject
    {
        [SerializeField] MeshPlane[] _meshPlanes;
        public MeshPlane[] MeshPlanes => _meshPlanes;

        [SerializeField] bool _hasCollision = true;
        public bool HasCollision => _hasCollision;

        [SerializeField] bool _isSolid = true;
        public bool IsSolid => _isSolid;

        [SerializeField] Material _material;
        public Material Material => _material;

        [SerializeField] Mesh _mesh;
        public Mesh Mesh => _mesh;

        public Mesh MeshInstance => Instantiate(Mesh);

        public static void SpawnMeshForEdit(VoxelMeshData_SO meshData)
        {
            GameObject go = new GameObject(meshData.name);
            go.tag = EngineConstants.CUSTOM_MESH_TAG;

            go.AddComponent<MeshFilter>().mesh = meshData.Mesh;
            go.AddComponent<MeshRenderer>().material = meshData.Material;
            go.AddComponent<CustomMeshReference>().MeshData = meshData;
        }

        [ContextMenu("Save Mesh")]
        public Mesh SaveMesh()
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

            if (_mesh == null)
            {
                _mesh = new Mesh();
                _mesh.name = "New Mesh";
            }
            _mesh.SetVertices(Vertices);
            _mesh.SetIndices(Indices, MeshTopology.Triangles, 0);
            _mesh.SetNormals(Normals);
            _mesh.SetUVs(0, UVS);
            _mesh.RecalculateBounds();
            _mesh.RecalculateTangents();

            Vertices.Dispose();
            Indices.Dispose();
            Normals.Dispose();
            UVS.Dispose();

#if UNITY_EDITOR

            string path = UnityEditor.AssetDatabase.GetAssetPath(_mesh);

            if (String.IsNullOrEmpty(path))
            {
                path = UnityEditor.EditorUtility.SaveFilePanelInProject("Save Mesh", "New Mesh", "asset", "Save Mesh");
                if (path.Length == 0)
                    return null;
                UnityEditor.AssetDatabase.CreateAsset(_mesh, path);
            }

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(_mesh);
            UnityEditor.AssetDatabase.SaveAssets();
#endif

            return _mesh;
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
        }

        // returns the verticies that have the same normal
        public Vertex[] GetPlanesFromNormal(float3 normal)
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

        // returns and array of plane indecies that are visible, -1 if the plane is not visible
        int[] GetVisiblePlaneIndecies(int visibleSides)
        {
            int[] indecies = new int[MeshPlanes.Length];

            for (int i = 0; i < MeshPlanes.Length; i++)
            {
                if (IsPlaneVisible(visibleSides, i, EngineUtilities.FaceToVec(Face.TOP)))
                    indecies[i] = 0;
                else if (IsPlaneVisible(visibleSides, i, EngineUtilities.FaceToVec(Face.BOTTOM)))
                    indecies[i] = 1;
                else if (IsPlaneVisible(visibleSides, i, EngineUtilities.FaceToVec(Face.LEFT)))
                    indecies[i] = 2;
                else if (IsPlaneVisible(visibleSides, i, EngineUtilities.FaceToVec(Face.RIGHT)))
                    indecies[i] = 3;
                else if (IsPlaneVisible(visibleSides, i, EngineUtilities.FaceToVec(Face.FRONT)))
                    indecies[i] = 4;
                else if (IsPlaneVisible(visibleSides, i, EngineUtilities.FaceToVec(Face.BACK)))
                    indecies[i] = 5;
                else
                    indecies[i] = -1;
            }

            return indecies;
        }

        bool IsPlaneVisible(int visibleSides, int planeIndex, float3 direction) => EngineUtilities.IsBitSet(visibleSides, EngineUtilities.DirectionToInt(direction)) && MeshPlanes[planeIndex].Vertices[0].normal.Equals(direction);
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

        public override string ToString() =>
            $"\n    Position: ({position.x},{position.y},{position.z})" +
            $"\n    Normal: ({normal.x},{normal.y},{normal.z})" +
            $"\n    UV: ({texCoord0.x},{texCoord0.y})";
    }
}
