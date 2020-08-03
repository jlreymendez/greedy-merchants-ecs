using GreedyMerchants.ECS.Coin;
using Svelto.ECS;
using Svelto.ECS.Extensions.Unity;

namespace Code.ECS.Coin.Descriptors
{
    public class CoinEntityDescriptor : IEntityDescriptor
    {
        public IComponentBuilder[] componentsToBuild
        {
            get => new IComponentBuilder[]
            {
                new ComponentBuilder<CoinComponent>(),
                new ComponentBuilder<CoinViewComponent>(),
                new ComponentBuilder<EGIDTrackerViewComponent>(),
            };
        }
    }
}