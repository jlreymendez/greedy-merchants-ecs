using System.Collections;
using GreedyMerchants.ECS.Ship;
using GreedyMerchants.Tests.Runtime.Utils;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

namespace GreedyMerchants.Tests.Runtime.ShipTests
{
    public class ShipToShipCollisionsTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return The.ShipDefinition.Load();

            The.Context.AddEngine(new ShipCollisionsEngine(The.Time));
            The.Context.AddEngine(new ShipAttackEngine(The.ConsumerFactory, The.ShipDefinition));
            The.Context.AddEngine(new ShipSinkingEngine(The.EntityFunctions));
        }

        [TearDown]
        public void TearDown()
        {
            The.Context = null;
        }

        [UnityTest]
        public IEnumerator Normal_Ship_To_Merchant_Multiple_Collisions_In_One_Frame()
        {
            var (egid1, ship1) = A.Player.WithId(1).WithPosition(new float3(1));
            var (egid2, ship2) = A.Player.WithId(2).WithPosition(new float3(1)).WithLevel(ShipLevel.Merchant);

            The.Context.Play();
            yield return new WaitForSeconds(0.5f);

            Assert.IsFalse(The.DB.Exists<ShipComponent>(egid2), "Ship 2 should have been sunk.");

            GameObject.Destroy(ship1);
            GameObject.Destroy(ship2);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Two_Normal_Ships_To_Merchant_Multiple_Collisions_In_One_Frame()
        {
            var (egid1, ship1) = A.Player.WithId(1).WithPosition(new float3(1));
            var (egid2, ship2) = A.Player.WithId(2).WithPosition(new float3(1)).WithLevel(ShipLevel.Merchant);
            var (egid3, ship3) = A.Player.WithId(3).WithPosition(new float3(1));

            yield return new WaitForFixedUpdate();

            The.Context.Play();
            yield return new WaitForSeconds(0.5f);

            Assert.IsFalse(The.DB.Exists<ShipComponent>(egid2), "Player 2 should have been sunk.");

            GameObject.Destroy(ship1);
            GameObject.Destroy(ship2);
            GameObject.Destroy(ship3);

            yield return null;
        }
    }
}