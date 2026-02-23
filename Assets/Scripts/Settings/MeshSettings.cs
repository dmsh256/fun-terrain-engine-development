using UnityEngine;

namespace Settings
{
	[CreateAssetMenu]
	public class MeshSettings : UpdatableData
	{
		public const int numSupportedLODs = 5;
		private const int numSupportedChunkSizes = 9;
		private const int numSupportedFlatshadedChunkSizes = 3;
		private static readonly int[] supportedChunkSizes = {48, 72, 96, 120, 144, 168, 192, 216, 240};
		private static int numOfExtraVertices => 3;
		
		[HideInInspector]
		public int numOfOutOfMeshVertices = 2;

		public int meshScale = 1;
		public bool useFlatShading;

		[Range(0,numSupportedChunkSizes-1)]
		public int chunkSizeIndex;
		
		[Range(0,numSupportedFlatshadedChunkSizes-1)]
		public int flatShadedChunkSizeIndex;
	
		// num verts per line of mesh rendered at LOD = 0. Includes the 2 extra verts that are excluded from final mesh, but used for calculating normals
		public int numVertsPerLine => supportedChunkSizes [useFlatShading ? flatShadedChunkSizeIndex : chunkSizeIndex] + numOfExtraVertices + numOfOutOfMeshVertices;

		public int meshWorldSize => (numVertsPerLine - numOfExtraVertices) * meshScale;
	}
}
