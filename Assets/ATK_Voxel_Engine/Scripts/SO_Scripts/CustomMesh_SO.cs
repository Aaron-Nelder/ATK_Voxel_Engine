using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "Custom_Mesh", menuName = EngineConstants.ENGINE_NAME + "/Custom_Mesh")]
    public class CustomMesh_SO : ScriptableObject
    {
        public string meshName;
        public Vector3[] vertices;
        public List<Vector3> Vertices
        {
            get
            {
                List<Vector3> verts = new List<Vector3>();
                foreach (var plane in Planes)
                    foreach (var index in plane.VertexIndexs)
                        verts.Add(vertices[index]);
                return verts;
            }
        }
        public List<int> Indices
        {
            get
            {
                List<int> allIndicies = new List<int>();
                int offset = 0;
                foreach (var plane in planes)
                {
                    foreach (var index in plane.Indices)
                        allIndicies.Add(index + offset);
                    offset += 4;
                }
                return allIndicies;
            }
        }
        public List<Vector3> Normals
        {
            get
            {
                List<Vector3> norms = new List<Vector3>();
                foreach (var plane in Planes)
                    for (int i = 0; i < 4; i++)
                        norms.Add(new(plane.Normal.x, plane.Normal.y, plane.Normal.z));
                return norms;
            }
        }
        public List<Vector2> UVS
        {
            get
            {
                List<Vector2> uvs = new List<Vector2>();
                foreach (var plane in Planes)
                    foreach (var uv in plane.UVS)
                        uvs.Add(uv);
                return uvs;
            }
        }

        public MeshTopology meshTopology;

        [SerializeField] Plane[] planes;
        public Plane[] Planes => planes;

        [SerializeField] Mesh mesh;
        public Mesh Mesh
        {
            get => Instantiate(mesh);
            set => mesh = value;
        }

        public Mesh SaveMesh()
        {
            mesh = new Mesh();
            mesh.name = meshName;
            mesh.SetVertices(Vertices);
            mesh.SetIndices(Indices, meshTopology, 0);
            mesh.SetNormals(Normals);
            mesh.SetUVs(0, UVS);
            mesh.RecalculateBounds();
            mesh.colors = _SortedColoring(mesh.triangles);
            return mesh;
        }

        public bool GetVisiblesPlanes(ChunkPosition chunkPos, int3 vPos, out List<Vector3> vertices, out List<int> indices, out List<Vector2> UVs, out List<int3> normals)
        {
            int topologyOffset = meshTopology == MeshTopology.Quads ? 4 : 3;
            vertices = new List<Vector3>();
            indices = new List<int>();
            UVs = new List<Vector2>();
            normals = new List<int3>();
            int offset = 0;
            bool hasVisible = false;

            //Loops through all planes in the mesh
            foreach (var plane in Planes)
            {
                // if the plane is visible
                if (!WorldHelper.IsOccupied(chunkPos, vPos + plane.Normal))
                {
                    // Vertices
                    foreach (var index in plane.VertexIndexs)
                        vertices.Add(this.vertices[index] + new Vector3Int(vPos.x, vPos.y, vPos.z));

                    // Indices
                    foreach (var index in plane.Indices)
                        indices.Add(index + offset);
                    offset += topologyOffset;

                    // UVs
                    foreach (var uv in plane.UVS)
                        UVs.Add(uv);

                    // Normals
                    for (int i = 0; i < topologyOffset; i++)
                        normals.Add(plane.Normal);

                    hasVisible = true;
                }
            }

            return hasVisible;
        }

        public void GetPlanesFromInt(int visibleSides, int3 vPos, out List<Vector3> vertices, out List<int> indices, out List<Vector2> UVs, out List<int3> normals)
        {
            int topologyOffset = meshTopology == MeshTopology.Quads ? 4 : 3;
            vertices = new List<Vector3>();
            indices = new List<int>();
            UVs = new List<Vector2>();
            normals = new List<int3>();
            int offset = 0;

            foreach (var plane in Planes)
            {
                // if the plane is visible
                if ((WorldHelper.IsBitSet(visibleSides, 0) && plane.Normal.Equals(new int3(0, 1, 0)))
                    || (WorldHelper.IsBitSet(visibleSides, 1) && plane.Normal.Equals(new int3(0, -1, 0)))
                    || (WorldHelper.IsBitSet(visibleSides, 2) && plane.Normal.Equals(new int3(-1, 0, 0)))
                    || (WorldHelper.IsBitSet(visibleSides, 3) && plane.Normal.Equals(new int3(1, 0, 0)))
                    || (WorldHelper.IsBitSet(visibleSides, 4) && plane.Normal.Equals(new int3(0, 0, 1)))
                    || (WorldHelper.IsBitSet(visibleSides, 5) && plane.Normal.Equals(new int3(0, 0, -1))))
                {
                    // Vertices
                    foreach (var index in plane.VertexIndexs)
                        vertices.Add(this.vertices[index] + new Vector3(vPos.x, vPos.y, vPos.z));

                    // Indices
                    foreach (var index in plane.Indices)
                        indices.Add(index + offset);
                    offset += topologyOffset;

                    // UVs
                    foreach (var uv in plane.UVS)
                        UVs.Add(uv);

                    // Normals
                    for (int i = 0; i < topologyOffset; i++)
                        normals.Add(plane.Normal);
                }
            }
        }

        public bool IsVisible(ChunkPosition chunkPos, int3 vPos)
        {
            foreach (var plane in Planes)
                if (!WorldHelper.IsOccupied(chunkPos, vPos + plane.Normal))
                    return true;

            return false;
        }

        public Plane GetPlane(int3 vPos, int3 dir, out List<Vector3> verticies, out List<int> Indicies, out List<Vector2> UVS, out List<int3> Normals)
        {
            verticies = new List<Vector3>() { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
            Indicies = new List<int>();
            UVS = new List<Vector2>();
            Normals = new List<int3>() { int3.zero, int3.zero, int3.zero, int3.zero };
            foreach (var plane in Planes)
            {
                if (plane.Normal.Equals(dir))
                {
                    for (int i = 0; i < 4; i++)
                        verticies[i] = vertices[plane.VertexIndexs[i]] + new Vector3(vPos.x, vPos.y, vPos.z);
                    Indicies = plane.Indices;
                    UVS = plane.UVS;
                    for (int i = 0; i < 4; i++)
                        Normals[i] = plane.Normal;
                    return plane;
                }
            }

            return new Plane();
        }

        public CustomMesh GetStruct()
        {
            return new CustomMesh(Vertices.ToArray(), Indices.ToArray(), Normals.ToArray(), UVS.ToArray());
        }

        static Color[] _COLORS = new Color[] { Color.red, Color.green, Color.blue, };
        Color[] _SortedColoring(int[] tris)
        {
            int length = Planes.Length * 4;

            int[] labels = new int[length];

            List<int[]> triangles = _GetSortedTriangles(tris);
            triangles.Sort((int[] t1, int[] t2) =>
            {
                int i = 0;
                while (i < t1.Length && i < t2.Length)
                {
                    if (t1[i] < t2[i]) return -1;
                    if (t1[i] > t2[i]) return 1;
                    i += 1;
                }
                if (t1.Length < t2.Length) return -1;
                if (t1.Length > t2.Length) return 1;
                return 0;
            });

            foreach (int[] triangle in triangles)
            {
                List<int> availableLabels = new List<int>() { 1, 2, 3 };
                foreach (int vertexIndex in triangle)
                {
                    if (availableLabels.Contains(labels[vertexIndex]))
                        availableLabels.Remove(labels[vertexIndex]);
                }
                foreach (int vertexIndex in triangle)
                {
                    if (labels[vertexIndex] == 0)
                    {
                        if (availableLabels.Count == 0)
                        {
                            Debug.LogError("Could not find color");
                            return null;
                        }
                        labels[vertexIndex] = availableLabels[0];
                        availableLabels.RemoveAt(0);
                    }
                }
            }

            Color[] colors = new Color[length];
            for (int i = 0; i < length; i++)
                colors[i] = labels[i] > 0 ? _COLORS[labels[i] - 1] : _COLORS[0];

            return colors;
        }

        List<int[]> _GetSortedTriangles(int[] triangles)
        {
            List<int[]> result = new List<int[]>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                List<int> t = new List<int> { triangles[i], triangles[i + 1], triangles[i + 2] };
                t.Sort();
                result.Add(t.ToArray());
            }
            return result;
        }

        #region Context Menu
        [ContextMenu("Top Plane Setup")]
        void TopPlaneSetup()
        {
            vertices = new Vector3[]
            {
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f)
            };

            planes = new Plane[]
            {
            new Plane(Face.TOP,new List<int> { 0, 1, 2, 3 })
            };
        }

        [ContextMenu("Create Default Block")]
        void DefaultBlock()
        {
            meshTopology = MeshTopology.Quads;

            vertices = new Vector3[]
            {         
            //top
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),

            //bottom
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f),
            };

            planes = new Plane[]
            {
            new Plane(Face.TOP,new List<int>{0,1,2,3 }),
            new Plane(Face.BOTTOM,new List<int> { 7, 6, 5, 4 }),
            new Plane(Face.LEFT,new List<int> { 5, 1, 0, 4 }),
            new Plane(Face.RIGHT,new List<int>{7,3,2,6 }),
            new Plane(Face.FRONT,new List<int>{6,2,1,5 }),
            new Plane(Face.BACK,new List<int> { 4, 0, 3, 7 })
            };
        }
        #endregion
    }

    public struct CustomMesh
    {
        public Vector3[] vertices;
        public int[] indices;
        public Vector3[] normals;
        public Vector2[] uvs;

        public CustomMesh(Vector3[] vertices, int[] indices, Vector3[] normals, Vector2[] uvs)
        {
            this.vertices = vertices;
            this.indices = indices;
            this.normals = normals;
            this.uvs = uvs;
        }
    }

    [System.Serializable]
    public struct Plane
    {
        [SerializeField] Face normal;
        public int3 Normal => WorldHelper.FaceToVec(normal);

        [SerializeField] List<int> vertexIndexs;
        public List<int> VertexIndexs => vertexIndexs;

        [SerializeField] List<int> indices;
        public List<int> Indices => indices;

        [SerializeField] List<Vector2> uvs;
        public List<Vector2> UVS => uvs;

        public Plane(Face normal, List<int> vertexIndexs)
        {
            this.normal = normal;
            this.vertexIndexs = vertexIndexs;
            indices = new List<int> { vertexIndexs[0], vertexIndexs[1], vertexIndexs[2], vertexIndexs[3] };
            uvs = new List<Vector2> { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
        }
    }
}