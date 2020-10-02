using System.Collections;
using GreedyMerchants.ECS.Coin;
using GreedyMerchants.ECS.Grid;
using GreedyMerchants.ECS.Grid.Engines;
using GreedyMerchants.Tests.Runtime.Utils;
using NUnit.Framework;
using Svelto.ECS;
using Svelto.ECS.Extensions.Unity;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

namespace GreedyMerchants.Tests.Runtime
{
    public class CoinSpawningTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // note: Important we are instantiating since we will be changing some value. DON'T change to load.
            yield return The.CoinDefinition.Instantiate();

            The.Context.AddEngine(new CoinSpawningEngine(1234, The.CoinDefinition, The.EntityFactory, The.GameObjectFactory, The.EntityFunctions, The.Time));
            The.Context.AddEngine(new GridCellRecyclingEngine(The.EntityFunctions, The.Grid.Utils));
        }

        [TearDown]
        public void TearDown()
        {
            TearDownUtils.DestroyAllEntityGameObjects();
            The.Context = null;
        }

        [UnityTest]
        public IEnumerator Only_One_Grid_Cell_To_Spawn()
        {
            var coinsToSpawn = 2;
            The.CoinDefinition.Result.MaxCoins = coinsToSpawn;
            The.Context.Play();

            EGID cellEgid = A.GridCell.WithPosition(float2.zero);
            yield return new WaitForSeconds(0.5f);

            var freeCells = The.DB.Count<GridCellComponent>(GridGroups.GridWaterNoCoinGroup);
            var takenCells = The.DB.Count<GridCellComponent>(GridGroups.GridWaterHasCoinGroup);
            var spawnedCoins = The.DB.Count<CoinComponent>(CoinGroups.SpawnedCoinsGroup);

            Assert.AreEqual(0, freeCells, "All cells should be taken");
            Assert.AreEqual(takenCells, spawnedCoins, "Taken grid cells should equal spawned coins");
            Assert.AreEqual(coinsToSpawn - spawnedCoins, The.DB.Count<CoinComponent>(CoinGroups.RecycledCoinsGroup), "Some coins should be ready");
        }
    }
}
