using UnityEngine;
using System.Collections.Generic;

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
                        norms.Add(plane.Normal);
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

        public bool GetVisiblesPlanes(ChunkPosition chunkPos, Vector3Int voxelPosition, out List<Vector3> vertices, out List<int> indices, out List<Vector2> UVs, out List<Vector3> normals)
        {
            int topologyOffset = meshTopology == MeshTopology.Quads ? 4 : 3;
            vertices = new List<Vector3>();
            indices = new List<int>();
            UVs = new List<Vector2>();
            normals = new List<Vector3>();
            int offset = 0;
            bool hasVisible = false;

            //Loops through all planes in the mesh
            foreach (var plane in Planes)
            {
                // if the plane is visible
                if (!WorldHelper.IsOccupied(chunkPos, voxelPosition + plane.Normal))
                {
                    // Vertices
                    foreach (var index in plane.VertexIndexs)
                        vertices.Add(this.vertices[index] + voxelPosition);

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

        public bool IsVisible(ChunkPosition chunkPos, Vector3Int voxelPosition)
        {
            foreach (var plane in Planes)
                if (!WorldHelper.IsOccupied(chunkPos, voxelPosition + plane.Normal))
                    return true;

            return false;
        }

        public Plane GetPlane(Vector3Int voxelPosition, Vector3Int direction, out List<Vector3> verticies, out List<int> Indicies, out List<Vector2> UVS, out List<Vector3> Normals)
        {
            verticies = new List<Vector3>() { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
            Indicies = new List<int>();
            UVS = new List<Vector2>();
            Normals = new List<Vector3>() { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
            foreach (var plane in Planes)
            {
                if (plane.Normal == direction)
                {
                    for (int i = 0; i < 4; i++)
                        verticies[i] = vertices[plane.VertexIndexs[i]] + voxelPosition;
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
            new Plane(Face.Top,new List<int> { 0, 1, 2, 3 })
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
            new Plane(Face.Top,new List<int>{0,1,2,3 }),
            new Plane(Face.Bottom,new List<int> { 7, 6, 5, 4 }),
            new Plane(Face.Left,new List<int> { 5, 1, 0, 4 }),
            new Plane(Face.Right,new List<int>{7,3,2,6 }),
            new Plane(Face.Front,new List<int>{6,2,1,5 }),
            new Plane(Face.Back,new List<int> { 4, 0, 3, 7 })
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
        public Vector3Int Normal => WorldHelper.DirectionEnumToVector(normal);

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