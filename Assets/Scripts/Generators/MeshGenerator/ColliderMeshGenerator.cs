using Settings;
using UnityEngine;

namespace Generators.MeshGenerator
{
    public static class ColliderMeshGenerator
    {
        public static ColliderMeshData GenerateColliderMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail)
        {
            int skipIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
            int numVertsPerLine = meshSettings.numVerticesPerLine;

            Vector2 topLeft = new(0, 0);

            ColliderMeshData colliderMeshData = new(numVertsPerLine, skipIncrement);

            int[] vertexIndicesMap = new int[numVertsPerLine * numVertsPerLine];
            int meshVertexIndex = 0;
            int outOfMeshVertexIndex = -1;

            int numVertsPerLine1 = numVertsPerLine - 1;
            int numVertsPerLine2 = numVertsPerLine - 2;
            int numVertsPerLine3 = numVertsPerLine - 3;
            float invSize = 1f / (numVertsPerLine - 3);

            for (int y = 0; y < numVertsPerLine; y++)
            {
                for (int x = 0; x < numVertsPerLine; x++)
                {
                    bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine1 || x == 0 || x == numVertsPerLine1;
                    bool isSkippedVertex = x > 2 && x < numVertsPerLine3 && y > 2 && y < numVertsPerLine3 &&
                                           ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);
                    if (isOutOfMeshVertex)
                    {
                        vertexIndicesMap[y * numVertsPerLine + x] = outOfMeshVertexIndex;
                        outOfMeshVertexIndex--;
                    }
                    else if (!isSkippedVertex)
                    {
                        vertexIndicesMap[y * numVertsPerLine + x] = meshVertexIndex;
                        meshVertexIndex++;
                    }
                }
            }

            for (int y = 0; y < numVertsPerLine; y++)
            {
                for (int x = 0; x < numVertsPerLine; x++)
                {
                    bool isSkippedVertex = x > 2 && x < numVertsPerLine3 && y > 2 && y < numVertsPerLine3 &&
                                           ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);

                    if (isSkippedVertex)
                        continue;

                    bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine1 || x == 0 || x == numVertsPerLine1;
                    bool isMeshEdgeVertex =
                        (y == 1 || y == numVertsPerLine2 || x == 1 || x == numVertsPerLine2) &&
                        !isOutOfMeshVertex;
                    bool isMainVertex = (x - 2) % skipIncrement == 0 && (y - 2) % skipIncrement == 0 &&
                                        !isOutOfMeshVertex && !isMeshEdgeVertex;
                    bool isEdgeConnectionVertex =
                        (y == 2 || y == numVertsPerLine3 || x == 2 || x == numVertsPerLine3) &&
                        !isOutOfMeshVertex && !isMeshEdgeVertex && !isMainVertex;

                    int vertexIndex = vertexIndicesMap[y * numVertsPerLine + x];
                    Vector2 percent = new((x - 1) * invSize, (y - 1) * invSize);
                    Vector2 vertexPosition2D =
                        topLeft + new Vector2(percent.x, percent.y) * meshSettings.meshWorldSize;

                    float height = heightMap[x, y];
                    if (isEdgeConnectionVertex)
                    {
                        bool isVertical = x == 2 || x == numVertsPerLine3;
                        int dstToMainVertexA = (isVertical ? y - 2 : x - 2) % skipIncrement;
                        int dstToMainVertexB = skipIncrement - dstToMainVertexA;
                        float dstPercentFromAToB = dstToMainVertexA / (float)skipIncrement;

                        float heightMainVertexA = heightMap[isVertical ? x : x - dstToMainVertexA,
                            isVertical ? y - dstToMainVertexA : y];
                        float heightMainVertexB = heightMap[isVertical ? x : x + dstToMainVertexB,
                            isVertical ? y + dstToMainVertexB : y];

                        height = heightMainVertexA * (1 - dstPercentFromAToB) + heightMainVertexB * dstPercentFromAToB;
                    }

                    colliderMeshData.AddVertex(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), vertexIndex);
                    bool createTriangle = x < numVertsPerLine1 && y < numVertsPerLine1 &&
                                          (!isEdgeConnectionVertex || (x != 2 && y != 2));

                    if (createTriangle)
                    {
                        int currentIncrement = isMainVertex && x != numVertsPerLine3 && y != numVertsPerLine3
                            ? skipIncrement
                            : 1;

                        int row = y * numVertsPerLine;
                        int nextRow = (y + currentIncrement) * numVertsPerLine;
                        int a = vertexIndicesMap[row + x];
                        int b = vertexIndicesMap[row + x + currentIncrement];
                        int c = vertexIndicesMap[nextRow + x];
                        int d = vertexIndicesMap[nextRow + x + currentIncrement];

                        colliderMeshData.AddTriangle(a, c, d);
                        colliderMeshData.AddTriangle(d, b, a);
                    }
                }
            }
            
            return colliderMeshData;
        }
    }

    public class ColliderMeshData
    {
        private readonly Vector3[] vertices;
        private readonly int[] triangles;

        private int triangleIndex;
        
        public ColliderMeshData(int numVertsPerLine, int skipIncrement)
        {
            int numMeshEdgeVertices = (numVertsPerLine - 2) * 4 - 4;
            int numEdgeConnectionVertices = (skipIncrement - 1) * (numVertsPerLine - 5) / skipIncrement * 4;
            int numMainVerticesPerLine = (numVertsPerLine - 5) / skipIncrement + 1;
            int numMainVertices = numMainVerticesPerLine * numMainVerticesPerLine;

            vertices = new Vector3[numMeshEdgeVertices + numEdgeConnectionVertices + numMainVertices];

            int numMeshEdgeTriangles = 8 * (numVertsPerLine - 4);
            int numMainTriangles = (numMainVerticesPerLine - 1) * (numMainVerticesPerLine - 1) * 2;
            triangles = new int[(numMeshEdgeTriangles + numMainTriangles) * 3];
        }

        public void AddVertex(Vector3 vertexPosition, int vertexIndex)
        {
            if (vertexIndex < 0)
                return;

            vertices[vertexIndex] = vertexPosition;
        }

        public void AddTriangle(int a, int b, int c)
        {
            if (a < 0 || b < 0 || c < 0)
                return;
            
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }

        public Mesh CreateColliderMesh()
        {
            Mesh mesh = new()
            {
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}