using System.Collections;
using GreedyMerchants.ECS.Coin;
using GreedyMerchants.ECS.Ship;
using GreedyMerchants.Tests.Runtime.Utils;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

namespace GreedyMerchants.Tests.Runtime.ShipTests
{
    public class ShipToCoinCollisionsTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return The.ShipDefinition.Load();

            The.Context.AddEngine(new ShipCollisionsEngine(The.Time));
            The.Context.AddEngine(new ShipCoinPickupEngine(The.ConsumerFactory));
            The.Context.AddEngine(new CoinRecyclerEngine(The.EntityFunctions, The.Grid.Utils));
        }

        [TearDown]
        public void TearDown()
        {
            The.Context = null;
        }

        [UnityTest]
        public IEnumerator One_Ship_To_Coin_Multiple_Collisions_In_One_Frame()
        {
            var (shipEgid, ship) = A.Player.WithId(1).WithPosition(new float3(1));
            var (coinEgid, coin) = A.Coin.WithId(1).WithPosition(new float3(1));

            The.Context.Play();
            yield return new WaitForSeconds(0.5f);

            Assert.IsFalse(The.DB.Exists<ShipComponent>(coinEgid), "Coin should have been recycled.");

            GameObject.Destroy(ship);
            GameObject.Destroy(coin);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Two_Ships_To_Coin_Multiple_Collisions_In_One_Frame()
        {
            var (shipEgid1, ship1) = A.Player.WithId(1).WithPosition(new float3(1));
            var (shipEgid2, ship2) = A.Player.WithId(2).WithPosition(new float3(1));
            var (coinEgid, coin) = A.Coin.WithId(1).WithPosition(new float3(1));

            The.Context.Play();
            yield return new WaitForSeconds(0.5f);

            Assert.IsFalse(The.DB.Exists<ShipComponent>(coinEgid), "Coin should have been recycled.");

            GameObject.Destroy(ship1);
            GameObject.Destroy(ship2);
            GameObject.Destroy(coin);

            yield return null;
        }
    }
}