using GreedyMerchants.Data.Ship;
using GreedyMerchants.ECS.Grid;
using GreedyMerchants.ECS.Unity;
using Svelto.ECS;
using UnityEngine;

namespace GreedyMerchants.Tests.Runtime.Utils
{
    public static class The
    {
        public static SECSContext Context
        {
            get
            {
                if (context == null)
                {
                    context = new SECSContext();
                }

                return context;
            }
            set
            {
                context?.Dispose();
                context = value;
            }
        }
        static SECSContext context;


        public static IEntityFactory EntityFactory => Context.EntityFactory;
        public static IEntityStreamConsumerFactory ConsumerFactory => Context.ConsumerFactory;
        public static IEntityFunctions EntityFunctions => Context.EntityFunctions;
        public static EntitiesDB DB => Context.EntitiesDB;
        public static ITime Time => Context.Time;

        public static GameObjectFactory GameObjectFactory => Context.GameObjectFactory;

        public static GridContext Grid
        {
            get
            {
                if (grid == null) grid = new GridBuilder();
                return grid;
            }
            set
            {
                grid = null;
            }
        }
        static GridContext grid;

        public static AssetLoader<ShipDefinition> ShipDefinition
        {
            get
            {
                if (shipDefinition == null) shipDefinition = new ShipDefinitionBuilder();
                return shipDefinition;
            }
            set
            {
                shipDefinition = null;
            }
        }
        static AssetLoader<ShipDefinition> shipDefinition;
    }
}