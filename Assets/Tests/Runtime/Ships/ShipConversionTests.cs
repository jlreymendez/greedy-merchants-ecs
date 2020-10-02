using System.Collections;
using GreedyMerchants.ECS.Ship;
using GreedyMerchants.Tests.Runtime.Utils;
using NUnit.Framework;
using Svelto.ECS;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

namespace GreedyMerchants.Tests.Runtime.Ships
{
    public class ShipConversionTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // note: We are instantiating because we are changing values. DON'T use load.
            yield return The.ShipDefinition.Instantiate();
            The.ShipDefinition.Result.TimeBetweenConversion = 0.1f;
            The.ShipDefinition.Result.ConversionTransitionTime = 0.1f;

            The.Context.AddEngine(new ShipLevelConversionEngine(The.ShipDefinition));
        }

        [TearDown]
        public void TearDown()
        {
            TearDownUtils.DestroyAllEntityGameObjects();
            The.Context = null;
        }

        [UnityTest]
        public IEnumerator All_Normal_Alive_To_One_Merchant_Test()
        {
            var (egid1, ship1) = A.Player.WithId(1).WithPosition(float3.zero).WithCoins(0);
            var (egid2, ship2) = A.Player.WithId(2).WithPosition(float3.zero).WithCoins(0);
            var (egid3, ship3) = A.Player.WithId(3).WithPosition(float3.zero).WithCoins(0);
            var (egid4, ship4) = A.Player.WithId(4).WithPosition(float3.zero).WithCoins(1);

            The.Context.Play();
            yield return null;

            Assert.AreEqual(ShipLevel.Normal, The.DB.QueryEntity<ShipLevelComponent>(egid1).Level);
            Assert.AreEqual(ShipLevel.Normal, The.DB.QueryEntity<ShipLevelComponent>(egid2).Level);
            Assert.AreEqual(ShipLevel.Normal, The.DB.QueryEntity<ShipLevelComponent>(egid3).Level);
            Assert.AreEqual(ShipLevel.Normal, The.DB.QueryEntity<ShipLevelComponent>(egid4).Level);

            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(ShipLevel.Normal, The.DB.QueryEntity<ShipLevelComponent>(egid1).Level);
            Assert.AreEqual(ShipLevel.Normal, The.DB.QueryEntity<ShipLevelComponent>(egid2).Level);
            Assert.AreEqual(ShipLevel.Normal, The.DB.QueryEntity<ShipLevelComponent>(egid3).Level);
            Assert.AreEqual(ShipLevel.Merchant, The.DB.QueryEntity<ShipLevelComponent>(egid4).Level);
        }

        [UnityTest]
        public IEnumerator All_Normal_Alive_To_One_Merchant_And_One_Pirate_Test()
        {
            var (egid1, ship1) = A.Player.WithId(1).WithPosition(float3.zero).WithCoins(0);
            var (egid2, ship2) = A.Player.WithId(2).WithPosition(float3.zero).WithCoins(1);
            var (egid3, ship3) = A.Player.WithId(3).WithPosition(float3.zero).WithCoins(1);
            var (egid4, ship4) = A.Player.WithId(4).WithPosition(float3.zero).WithCoins(2);

            The.Context.Play();
            yield return null;

            Assert.AreEqual(ShipLevel.Normal, The.DB.QueryEntity<ShipLevelComponent>(egid1).Level);
            Assert.AreEqual(ShipLevel.Normal, The.DB.QueryEntity<ShipLevelComponent>(egid2).Level);
            Assert.AreEqual(ShipLevel.Normal, The.DB.QueryEntity<ShipLevelComponent>(egid3).Level);
            Assert.AreEqual(ShipLevel.Normal, The.DB.QueryEntity<ShipLevelComponent>(egid4).Level);

            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(ShipLevel.Pirate, The.DB.QueryEntity<ShipLevelComponent>(egid1).Level);
            Assert.AreEqual(ShipLevel.Normal, The.DB.QueryEntity<ShipLevelComponent>(egid2).Level);
            Assert.AreEqual(ShipLevel.Normal, The.DB.QueryEntity<ShipLevelComponent>(egid3).Level);
            Assert.AreEqual(ShipLevel.Merchant, The.DB.QueryEntity<ShipLevelComponent>(egid4).Level);
        }
    }
}