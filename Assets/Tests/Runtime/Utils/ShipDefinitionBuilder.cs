using GreedyMerchants.Data.Ship;

namespace GreedyMerchants.Tests.Runtime.Utils
{
    public class ShipDefinitionBuilder
    {
        string _key = "ShipDefinition";

        public AssetLoader<ShipDefinition> Build()
        {
            return new AssetLoader<ShipDefinition>(_key);
        }

        public ShipDefinitionBuilder WithKey(string key)
        {
            _key = key;
            return this;
        }

        public AssetLoader<ShipDefinition> Loader => Build();

        public static implicit operator AssetLoader<ShipDefinition>(ShipDefinitionBuilder builder)
        {
            return builder.Build();
        }
    }
}