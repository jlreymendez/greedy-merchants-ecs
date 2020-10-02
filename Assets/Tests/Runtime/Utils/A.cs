namespace GreedyMerchants.Tests.Runtime.Utils
{
    public static class A
    {
        public static PlayerBuilder Player => new PlayerBuilder();
        public static CoinBuilder Coin => new CoinBuilder();
        public static GridCellBuilder GridCell => new GridCellBuilder();
    }
}