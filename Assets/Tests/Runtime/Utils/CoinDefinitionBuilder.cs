using Code.Data.Coin;

namespace GreedyMerchants.Tests.Runtime.Utils
{
    public class CoinDefinitionBuilder
    {
        string _key = "CoinDefinition";

        public AssetLoader<CoinDefinition> Build()
        {
            return new AssetLoader<CoinDefinition>(_key);
        }

        public CoinDefinitionBuilder WithKey(string key)
        {
            _key = key;
            return this;
        }

        public AssetLoader<CoinDefinition> Loader => Build();

        public static implicit operator AssetLoader<CoinDefinition>(CoinDefinitionBuilder builder)
        {
            return builder.Build();
        }
    }
}