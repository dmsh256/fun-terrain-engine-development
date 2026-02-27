using System;
using System.Collections.Generic;
using Generators.Noise;
using Generators.Noise.NoiseSource;
using Settings;
using UnityEngine;

namespace Generators.HeightMap
{
    /**
     * This is the base (raw) terrain generator. The fundament of the world.
     */
    public class StructuralHeightMapGenerator
    {
        private readonly List<(INoiseSource source, NoiseLayer layer)> noiseSources = new();

        public float[,] GenerateStructuralHeightMap(int width, int length, HeightMapSettings heightMapSettings, Vector2 sampleCentre)
        {
            if (heightMapSettings.layers == null || heightMapSettings.layers.Count == 0)
                throw new Exception("No noise layers defined.");
            
            INoiseSource maskSource = null;
            NoiseLayer maskLayer = null;
            foreach (NoiseLayer layer in heightMapSettings.layers)
            {
                if (!layer.enabled) continue;

                INoiseSource source = NoiseGeneratorFactory.Create(layer.settings);
                if (layer.useAsMask)
                {
                    maskSource = source;
                    maskLayer = layer;
                }
                else
                {
                    noiseSources.Add((source, layer));
                }
            }

            if (noiseSources.Count == 0)
                throw new Exception("No active height noise layers found.");
            
            if (maskLayer == null)
                throw new Exception("No active mask noise layer found.");

            float[,] result = new float[width, length];
            float[,] maskValues = null;

            if (maskSource != null)
                maskValues = maskSource.Generate(width, length, sampleCentre);

            bool useCurve = heightMapSettings.useHeightCurve;
            AnimationCurve curve = null;
            if (useCurve)
                curve = new AnimationCurve(heightMapSettings.heightCurve.keys);
            
            foreach ((INoiseSource noiseSource, NoiseLayer noiseLayer) in noiseSources)
            {
                float[,] heightValues = noiseSource.Generate(width, length, sampleCentre);
                float maskMin = maskLayer.maskSmoothRange.x;
                float maskMax = maskLayer.maskSmoothRange.y;
                for (int y = 0; y < length; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float heightValue = heightValues[x, y] * noiseLayer.weight;
                        if (maskValues != null)
                        {
                            float maskValue = maskValues[x, y];
                            float smoothedMask = Mathf.SmoothStep(maskMin, maskMax, maskValue);

                            heightValue *= smoothedMask;
                        }

                        if (useCurve)
                            heightValue = curve.Evaluate(heightValue);
                        
                        result[x, y] = ApplyBlend(noiseLayer, result[x, y], heightValue);
                    }
                }
            }

            return result;
        }

        /**
         * teases to replace it with layer->ApplyBlend(), but it'll require a custom PropertyDrawer
         */
        private float ApplyBlend(NoiseLayer noiseLayer, float currentHeight, float layerHeight)
        {
            if (noiseLayer.invertInput)
                layerHeight = 1f - layerHeight;

            switch (noiseLayer.blendMode)
            {
                case NoiseBlendMode.Add:
                    currentHeight += layerHeight;
                    break;

                case NoiseBlendMode.Subtract:
                    currentHeight -= layerHeight;
                    break;

                case NoiseBlendMode.Multiply:
                    currentHeight *= layerHeight;
                    break;

                case NoiseBlendMode.Divide:
                    if (layerHeight != 0f)
                        currentHeight /= layerHeight;
                    break;

                case NoiseBlendMode.Replace:
                    currentHeight = layerHeight;
                    break;

                case NoiseBlendMode.Max:
                    currentHeight = Mathf.Max(currentHeight, layerHeight);
                    break;

                case NoiseBlendMode.Min:
                    currentHeight = Mathf.Min(currentHeight, layerHeight);
                    break;
            }

            return currentHeight; 
        }
    }
}