using UnityEngine;

namespace Settings
{
	[CreateAssetMenu]
	public class MeshSettings : UpdatableData
	{
		public const int numSupportedLODs = 5;
		private const int numSupportedChunkSizes = 9;
		private const int numSupportedFlatshadedChunkSizes = 3;
		private static readonly int[] chunkVertexSizes = {48, 72, 96, 120, 144, 168, 192, 216, 240};
		private static int numOfExtraVertices => 3;
		
		[HideInInspector]
		public int numOfOutOfMeshVertices = 2;

		[Header("How large a chunk is")]
		public int meshScale = 1;
		
		[Header("How many vertices per chunk")]
		[Range(0,numSupportedChunkSizes-1)]
		public int vertexSizeIndex;
		
		public bool useFlatShading;
		[Range(0,numSupportedFlatshadedChunkSizes-1)]
		public int flatShadedChunkSizeIndex;
	
		public int numVerticesPerLine => chunkVertexSizes [useFlatShading ? flatShadedChunkSizeIndex : vertexSizeIndex] + numOfExtraVertices + numOfOutOfMeshVertices;

		public int meshWorldSize => (numVerticesPerLine - numOfExtraVertices) * meshScale;
	}
}
