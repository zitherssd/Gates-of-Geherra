using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts.Utility
{
    public class MeshExtruder : MonoBehaviour
    {
        public Texture2D texture;
        public Texture2D depth;
        public Material material;

        // public bool contentScale = false;
        public bool normExtrusion = false;
        public float extrudeScale = 1.0f;
        public float3 scale = new(1.0f);

        public bool pixelPivotXY = true;
        [Tooltip("Middle of 64 pixels is (64+1)/2 - 0.5f = 31.5f")]
        public float3 _pivot = new float3(0.5f, 0.5f, 0.0f);
        private float3 pivot = new float3(0.5f, 0.5f, 0.0f);

        public float threshold = 0.00f;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private const int empty = -1;

        public void BuildGeometry()
        {
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshFilter = gameObject.GetComponent<MeshFilter>();
            meshRenderer.sharedMaterial = material;
            meshRenderer.sharedMaterial.SetTexture("_MainText", texture);

            Assert.IsTrue(depth.width == depth.height, "Depth texture is not square.");

            var width = depth.width;
            var height = depth.height;
            int2 sizes = new int2(width, height);

            var invWidth = 1.0f / (width - 1);
            var invHeight = 1.0f / (height - 1);

            SetPivot(width, height, invWidth, invHeight);

            List<Vector2> UVs = new List<Vector2>();
            List<Vector3> vertices = new List<Vector3>();
            int[] vertexTable = new int[width * height * 2];
            int[] vertexUnderTable = new int[width * height * 2];
            byte[] vertexStateTable = new byte[width * height * 2];

            Array.Fill(vertexTable, empty);
            Array.Fill(vertexUnderTable, empty);

            var minValue = Mathf.Infinity;
            var maxValue = Mathf.NegativeInfinity;
            float3 minBound = new(float.PositiveInfinity);
            float3 maxBound = new(float.NegativeInfinity);

            // Calculate vertices
            for (int face = 0; face < 2; face++)
            {
                var sign = (face == 1 ? -1 : 1);
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                    {
                        var pixel = depth.GetPixel(i, j);
                        var valid = pixel.a > 0;
                        if (valid)
                        {
                            var value = pixel.grayscale; // Depth
                            if (minValue > value) minValue = value;
                            if (maxValue < value) maxValue = value;

                            var tableIndex = CalcIndex(face, height, width, i, j);
                            var vertexIndex = vertices.Count;

                            var vertex = new Vector3(i * invWidth, j * invHeight, value * sign);
                            vertices.Add(vertex);
                            vertexTable[tableIndex] = vertexIndex;
                        }
                    }
            }

            var inverseDiffMaxMin = 1.0f / (maxValue - minValue);
            for (int face = 0; face < 2; face++)
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                    {
                        var tableIndex = CalcIndex(face, height, width, i, j);
                        var vertexIndex = vertexTable[tableIndex];

                        if (vertexIndex != empty)
                        {
                            float3 vertex = vertices[vertexIndex];

                            // Optionally normalize extrusion depth
                            if (normExtrusion)
                                vertex.z = (vertex.z - minValue) * inverseDiffMaxMin;

                            // Apply extrusion scale
                            vertex.z *= extrudeScale;

                            // Copy UVs before translation and scale
                            UVs.Add(new Vector2(vertex.x, vertex.y));

                            // Transform and scale vertex
                            vertices[vertexIndex] = (vertex - pivot) * scale;
                        }
                    }

            for (int face = 0; face < 2; face++)
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                    {
                        var tableIndex = CalcIndex(face, height, width, i, j);

                        // Quad state
                        int state = Convert.ToInt32(!isEmpty(vertexTable, width, height, i + 0, j + 0)) << 0 |
                                Convert.ToInt32(!isEmpty(vertexTable, width, height, i + 0, j + 1)) << 1 |
                                Convert.ToInt32(!isEmpty(vertexTable, width, height, i + 1, j + 0)) << 2 |
                                Convert.ToInt32(!isEmpty(vertexTable, width, height, i + 1, j + 1)) << 3;
                        state = CornerFixState(vertexTable, state, sizes, face, i, j);
                        vertexStateTable[tableIndex] = (byte)state;

                        // Add under edges
                        state = state |
                                Convert.ToInt32(!isEmpty(vertexTable, width, height, i - 1, j - 1)) << 4 |
                                Convert.ToInt32(!isEmpty(vertexTable, width, height, i - 1, j + 0)) << 5 |
                                Convert.ToInt32(!isEmpty(vertexTable, width, height, i - 1, j + 1)) << 6 |
                                Convert.ToInt32(!isEmpty(vertexTable, width, height, i + 0, j - 1)) << 7 |
                                Convert.ToInt32(!isEmpty(vertexTable, width, height, i + 1, j - 1)) << 8;

                        int bits = math.countbits(state);
                        if (bits != 0 && bits != 9)
                        {
                            int vertexIndex = vertices.Count;
                            var vertex = new float3(i * invWidth, j * invHeight, 0);

                            // Copy UVs before translation and scale
                            UVs.Add(new Vector2(vertex.x, vertex.y));

                            vertices.Add((vertex - pivot) * scale);
                            vertexUnderTable[tableIndex] = vertexIndex;
                        }
                    }


            // Calculate indices
            var indices = new List<int>();
            for (int i = 0; i < height - 1; i++)
                for (int j = 0; j < width - 1; j++)
                {
                    // Table indices
                    const int face = 0;
                    var bl = CalcIndex(face, height, width, i + 0, j + 0);
                    var br = CalcIndex(face, height, width, i + 0, j + 1);
                    var tl = CalcIndex(face, height, width, i + 1, j + 0);
                    var tr = CalcIndex(face, height, width, i + 1, j + 1);

                    byte state = vertexStateTable[bl];

                    #pragma warning disable format
                    switch (state)
                    {
                        case 0b0011: case_0b0011(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b1010: case_0b1010(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b0101: case_0b0101(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b1100: case_0b1100(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b0111: case_0b0101(indices, vertexTable, vertexUnderTable, bl, br, tl, tr);
                                     case_0b0011(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b1011: case_0b1010(indices, vertexTable, vertexUnderTable, bl, br, tl, tr);
                                     case_0b0011(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b1101: case_0b0101(indices, vertexTable, vertexUnderTable, bl, br, tl, tr);
                                     case_0b1100(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b1110: case_0b1010(indices, vertexTable, vertexUnderTable, bl, br, tl, tr);
                                     case_0b1100(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b0100: case_0b0100(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b0010: case_0b0010(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b1111: case_0b1111(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                    }
                    #pragma warning restore format
                }

            for (int i = 0; i < height - 1; i++)
                for (int j = 0; j < width - 1; j++)
                {
                    // Table indices
                    const int face = 1;
                    var bl = CalcIndex(face, height, width, i + 0, j + 0);
                    var br = CalcIndex(face, height, width, i + 0, j + 1);
                    var tl = CalcIndex(face, height, width, i + 1, j + 0);
                    var tr = CalcIndex(face, height, width, i + 1, j + 1);

                    byte state = vertexStateTable[bl];

                    #pragma warning disable format
                    switch (state)
                    {
                        case 0b0011: case_0b0011_rev(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b1010: case_0b1010_rev(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b0101: case_0b0101_rev(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b1100: case_0b1100_rev(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b0111: case_0b0101_rev(indices, vertexTable, vertexUnderTable, bl, br, tl, tr);
                                     case_0b0011_rev(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b1011: case_0b1010_rev(indices, vertexTable, vertexUnderTable, bl, br, tl, tr);
                                     case_0b0011_rev(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b1101: case_0b0101_rev(indices, vertexTable, vertexUnderTable, bl, br, tl, tr);
                                     case_0b1100_rev(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b1110: case_0b1010_rev(indices, vertexTable, vertexUnderTable, bl, br, tl, tr);
                                     case_0b1100_rev(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b1000: case_0b1000_rev(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b0001: case_0b0001_rev(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                        case 0b1111: case_0b1111_rev(indices, vertexTable, vertexUnderTable, bl, br, tl, tr); break;
                    }
                    #pragma warning restore format
                }

            if (!meshFilter.sharedMesh)
                meshFilter.sharedMesh = new Mesh();

            var mesh = meshFilter.sharedMesh;
            mesh.Clear();

            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, UVs);
            mesh.SetIndices(indices, MeshTopology.Quads, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }

        private int CornerFixState(int[] vertexTable, int state, int2 sizes, int face, int i, int j)
        {
            switch (state)
            {
                // Erase actual corner states
                case 0b0100: return 0;
                case 0b0010: return 0;
                case 0b1000: return 0;
                case 0b0001: return 0;

                // Replace with corner states
                case 0b1111:
                    if (face == 0)
                    {
                        // Indices for checking 0b0010 corner
                        int2 bl = new int2(j - 1, i + 1);
                        int2 br = new int2(j + 0, i + 1);
                        int2 tl = new int2(j - 1, i + 2);
                        int2 tr = new int2(j + 0, i + 2);

                        // State of potential corner
                        int corner = CalcState(vertexTable, sizes, bl, br, tl, tr);
                        if (corner == 0b0010)
                            return corner;

                        // Indices for checking 0b0100 corner
                        bl = new int2(j + 1, i - 1);
                        br = new int2(j + 2, i - 1);
                        tl = new int2(j + 1, i + 0);
                        tr = new int2(j + 2, i + 0);
                        // State of potential corner
                        corner = CalcState(vertexTable, sizes, bl, br, tl, tr);
                        if (corner == 0b0100)
                            return corner;
                    }
                    else if (face == 1)
                    {
                        // Indices for checking 0b0001 corner
                        int2 bl = new int2(j + 1, i + 1);
                        int2 br = new int2(j + 2, i + 1);
                        int2 tl = new int2(j + 1, i + 2);
                        int2 tr = new int2(j + 2, i + 2);
                        // State of potential corner
                        int corner = CalcState(vertexTable, sizes, bl, br, tl, tr);
                        if (corner == 0b0001)
                            return corner;

                        // Indices for checking 0b1000 corner
                        bl = new int2(j - 1, i - 1);
                        br = new int2(j + 0, i - 1);
                        tl = new int2(j - 1, i + 0);
                        tr = new int2(j + 0, i + 0);
                        // State of potential corner
                        corner = CalcState(vertexTable, sizes, bl, br, tl, tr);
                        if (corner == 0b1000)
                            return corner;
                    }
                    else
                        Assert.IsTrue(false);
                    break;
            }
            return state;
        }

        public void SetPivot(int width, int height, float invWidth, float invHeight)
        {
            pivot = _pivot;
            if (pixelPivotXY)
            {
                bool oob = !(_pivot.x >= 0 && _pivot.x < width && _pivot.y >= 0 && _pivot.y < height);
                Assert.IsTrue(!oob, "Pixel pivot out of bounds.");
                if (!oob)
                {
                    pivot.x = _pivot.x * invWidth;
                    pivot.y = _pivot.y * invHeight;
                }
            }
        }

        void case_0b0011(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            //Assert.IsTrue(vertexTable[bl] != empty);
            //Assert.IsTrue(vertexUnderTable[bl] != empty);
            //Assert.IsTrue(vertexUnderTable[br] != empty);
            //Assert.IsTrue(vertexTable[br] != empty);

            indices.Add(vertexTable[bl]);
            indices.Add(vertexUnderTable[bl]);
            indices.Add(vertexUnderTable[br]);
            indices.Add(vertexTable[br]);
        }
        void case_0b0011_rev(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            indices.Add(vertexTable[br]);
            indices.Add(vertexUnderTable[br]);
            indices.Add(vertexUnderTable[bl]);
            indices.Add(vertexTable[bl]);
        }


        void case_0b1010(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            //Assert.IsTrue(vertexUnderTable[br] != empty);
            //Assert.IsTrue(vertexUnderTable[tr] != empty);
            //Assert.IsTrue(vertexTable[tr] != empty);
            //Assert.IsTrue(vertexTable[br] != empty);

            indices.Add(vertexUnderTable[br]);
            indices.Add(vertexUnderTable[tr]);
            indices.Add(vertexTable[tr]);
            indices.Add(vertexTable[br]);
        }
        void case_0b1010_rev(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            indices.Add(vertexTable[br]);
            indices.Add(vertexTable[tr]);
            indices.Add(vertexUnderTable[tr]);
            indices.Add(vertexUnderTable[br]);
        }


        void case_0b1100(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            //Assert.IsTrue(vertexUnderTable[tl] != empty);
            //Assert.IsTrue(vertexTable[tl] != empty);
            //Assert.IsTrue(vertexTable[tr] != empty);
            //Assert.IsTrue(vertexUnderTable[tr] != empty);

            indices.Add(vertexUnderTable[tl]);
            indices.Add(vertexTable[tl]);
            indices.Add(vertexTable[tr]);
            indices.Add(vertexUnderTable[tr]);
        }
        void case_0b1100_rev(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            indices.Add(vertexUnderTable[tr]);
            indices.Add(vertexTable[tr]);
            indices.Add(vertexTable[tl]);
            indices.Add(vertexUnderTable[tl]);
        }


        void case_0b0101(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            //Assert.IsTrue(vertexTable[bl] != empty);
            //Assert.IsTrue(vertexTable[tl] != empty);
            //Assert.IsTrue(vertexUnderTable[tl] != empty);
            //Assert.IsTrue(vertexUnderTable[bl] != empty);

            indices.Add(vertexTable[bl]);
            indices.Add(vertexTable[tl]);
            indices.Add(vertexUnderTable[tl]);
            indices.Add(vertexUnderTable[bl]);
        }
        void case_0b0101_rev(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            indices.Add(vertexUnderTable[bl]);
            indices.Add(vertexUnderTable[tl]);
            indices.Add(vertexTable[tl]);
            indices.Add(vertexTable[bl]);
        }


        void case_0b0100(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            case_0b1111_mir(indices, vertexTable, vertexUnderTable, bl, br, tl, tr);
        }
        void case_0b0010(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            case_0b1111_mir(indices, vertexTable, vertexUnderTable, bl, br, tl, tr);
        }


        void case_0b1000_rev(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            case_0b1111_rev_mir(indices, vertexTable, vertexUnderTable, bl, br, tl, tr);
        }
        void case_0b0001_rev(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            case_0b1111_rev_mir(indices, vertexTable, vertexUnderTable, bl, br, tl, tr);
        }


        void case_0b1111_mir(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            //Assert.IsTrue(vertexTable[bl] != empty);
            //Assert.IsTrue(vertexTable[tl] != empty);
            //Assert.IsTrue(vertexTable[tr] != empty);
            //Assert.IsTrue(vertexTable[br] != empty);

            indices.Add(vertexTable[tl]);
            indices.Add(vertexTable[tr]);
            indices.Add(vertexTable[br]);
            indices.Add(vertexTable[bl]);
        }
        void case_0b1111(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            //Assert.IsTrue(vertexTable[bl] != empty);
            //Assert.IsTrue(vertexTable[tl] != empty);
            //Assert.IsTrue(vertexTable[tr] != empty);
            //Assert.IsTrue(vertexTable[br] != empty);

            indices.Add(vertexTable[bl]);
            indices.Add(vertexTable[tl]);
            indices.Add(vertexTable[tr]);
            indices.Add(vertexTable[br]);
        }
        void case_0b1111_rev(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            indices.Add(vertexTable[br]);
            indices.Add(vertexTable[tr]);
            indices.Add(vertexTable[tl]);
            indices.Add(vertexTable[bl]);
        }
        void case_0b1111_rev_mir(List<int> indices, int[] vertexTable, int[] vertexUnderTable, int bl, int br, int tl, int tr)
        {
            indices.Add(vertexTable[tr]);
            indices.Add(vertexTable[tl]);
            indices.Add(vertexTable[bl]);
            indices.Add(vertexTable[br]);
        }

        bool inBounds(int width, int height, int i, int j)
        {
            return j >= 0 && j < width && i >= 0 && i < height;
        }

        bool inBounds(int2 sizes, int2 indices)
        {
            return indices.x >= 0 && indices.x < sizes.x && indices.y >= 0 && indices.y < sizes.y;
        }

        bool isEmpty(int[] table, int width, int height, int i, int j)
        {
            if (inBounds(width, height, i, j))
                return table[i * width + j] == empty;
            else
                return true; // Out of bounds
        }

        bool isEmpty(int[] table, int2 sizes, int2 indices)
        {
            if (inBounds(sizes, indices))
                return table[CalcIndex(sizes, indices)] == empty;
            else
                return true; // Out of bounds
        }

        int CalcIndex(int width, int height, int i, int j)
        {
            return i * width + j;
        }

        int CalcIndex(int face, int width, int height, int i, int j)
        {
            return face * width * height + i * width + j;
        }

        int CalcIndex(int2 sizes, int2 indices)
        {
            return indices.y * sizes.x + indices.x;
        }

        int CalcIndex(int face, int2 sizes, int2 indices)
        {
            return face * sizes.x * sizes.y + indices.y * sizes.x + indices.x;
        }

        int CalcState(int[] vertexTable, int2 sizes, int2 bl, int2 br, int2 tl, int2 tr)
        {
            return Convert.ToInt32(!isEmpty(vertexTable, sizes, bl)) << 0 |
                   Convert.ToInt32(!isEmpty(vertexTable, sizes, br)) << 1 |
                   Convert.ToInt32(!isEmpty(vertexTable, sizes, tl)) << 2 |
                   Convert.ToInt32(!isEmpty(vertexTable, sizes, tr)) << 3;
        }
    }
}