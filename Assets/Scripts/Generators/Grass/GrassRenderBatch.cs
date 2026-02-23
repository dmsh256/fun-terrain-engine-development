using System.Collections.Generic;
using UnityEngine;

namespace Generators.Grass
{
    public class GrassRenderBatch
    {
        private readonly Mesh mesh;
        private Material material;
        public readonly List<Matrix4x4> matrices = new();

        private ComputeBuffer matrixBuffer;
        private ComputeBuffer argsBuffer;
        private int instanceCount;
        
        public GrassRenderBatch(Mesh mesh, Material material)
        {
            this.mesh = mesh;
            this.material = material;
        }

        public void BuildBuffers()
        {
            instanceCount = matrices.Count;
            matrixBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 16);
            matrixBuffer.SetData(matrices);

            argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

            uint[] args = new uint[5];
            args[0] = mesh.GetIndexCount(0);
            args[1] = (uint)instanceCount;
            args[2] = mesh.GetIndexStart(0);
            args[3] = mesh.GetBaseVertex(0);
            args[4] = 0;

            argsBuffer.SetData(args);

            material = new Material(material)
            {
                enableInstancing = true
            };
            material.SetBuffer("_InstanceMatrices", matrixBuffer);
        }

        public void Draw(Bounds bounds)
        {
            if (instanceCount == 0)
                return;

            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
        }

        public void Release()
        {
            if (matrixBuffer != null)
            {
                matrixBuffer.Release();
                matrixBuffer = null;
            }

            if (argsBuffer != null)
            {
                argsBuffer.Release();
                argsBuffer = null;
            }

            if (material)
            {
                Object.Destroy(material);
                material = null;
            }
        }
    }
}
