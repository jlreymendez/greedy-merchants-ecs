using System.Collections;
using System.Collections.Generic;
using GreedyMerchants.ECS.Grid;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.AI
{
    struct PathHeuristics : IEnumerator<GridDirection>
    {
        uint start;
        bool clockwise;
        int index;

        public PathHeuristics(GridDirection start, bool clockwise)
        {
            this.start = (uint)start;
            this.clockwise = clockwise;
            index = clockwise ? -1 : 1;
        }

        public bool MoveNext()
        {
            index += clockwise ? 1 : -1;
            return math.abs(index) != 4;
        }

        public void Reset()
        {
            index = clockwise ? -1 : 1;
        }

        public GridDirection Current
        {
            get => (GridDirection)(uint)((4 + start + index) % 4);
        }

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }
}