using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generators.Grass
{
    public class GrassChunk
    {
        private readonly Dictionary<GrassRenderKey, GrassRenderBatch> batchesByRender = new(16);
        private readonly List<GrassRenderBatch> batches = new(16);
        
        private ComputeBuffer matrixBuffer;
        private ComputeBuffer argsBuffer;
        private Material material;
        private Mesh mesh;
        private Bounds bounds;
        private bool hasBounds;
        private int instanceCount;
        
        private bool buffersBuilt;

        public void AddInstance(Mesh mesh, Material material, GrassInstance instance)
        {
            if (!mesh || !material)
                return;

            GrassRenderKey key = new(mesh, material);
            if (!batchesByRender.TryGetValue(key, out GrassRenderBatch batch))
            {
                batch = new GrassRenderBatch(mesh, material);
                batchesByRender.Add(key, batch);
                batches.Add(batch);
            }
            
            Matrix4x4 matrix = Matrix4x4.TRS(
                instance.position,
                instance.rotation,
                Vector3.one * instance.scale
            );

            batch.matrices.Add(matrix);
            if (!hasBounds)
            {
                bounds = new Bounds(instance.position, Vector3.zero);
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(instance.position);
            }
        }

        public void BuildBuffers()
        {
            if (buffersBuilt)
                return;

            foreach (GrassRenderBatch batch in batches)
                batch.BuildBuffers();

            buffersBuilt = true;
        }

        public void Draw()
        {
            foreach (GrassRenderBatch batch in batches)
                batch.Draw(bounds);
        }

        public void Release()
        {
            if (!buffersBuilt)
                return;

            foreach (GrassRenderBatch batch in batches)
                batch.Release();

            buffersBuilt = false;
        }
        
        private readonly struct GrassRenderKey : IEquatable<GrassRenderKey>
        {
            private readonly Mesh mesh;
            private readonly Material material;

            public GrassRenderKey(Mesh mesh, Material material)
            {
                this.mesh = mesh;
                this.material = material;
            }

            public bool Equals(GrassRenderKey other)
            {
                return ReferenceEquals(mesh, other.mesh) && ReferenceEquals(material, other.material);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (mesh ? mesh.GetHashCode() : 0) * 397 ^ (material ? material.GetHashCode() : 0);
                }
            }
        }
        
        ~GrassChunk()
        {
            Release();
        }
    }
}
