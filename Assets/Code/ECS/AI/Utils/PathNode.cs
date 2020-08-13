namespace GreedyMerchants.ECS.AI
{
    struct PathNode
    {
        public uint from;
        public uint length;

        public PathNode(uint from, uint length)
        {
            this.from = from;
            this.length = length;
        }
    }
}