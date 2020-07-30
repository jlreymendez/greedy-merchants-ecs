using Unity.Mathematics;
using UnityEngine;

namespace GreedyMerchants.Data.Grid
{
    [CreateAssetMenu(fileName = "Grid", menuName = "Game/Grid", order = 0)]
    public class GridDefinition : ScriptableObject
    {
        public uint2 Size;
        public float2 CellSize;
    }
}