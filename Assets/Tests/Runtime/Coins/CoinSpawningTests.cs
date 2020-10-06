using System.Collections;
using Code.ECS.Coin.Descriptors;
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
            The.Context.AddEngine(new GridCellCoinFilteringEngine(The.Grid.Utils));
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

            var freeCellFilter = The.DB.GetFilters()
                .CreateOrGetFilterForGroup<GridCellComponent>(GridGroups.FreeCellFilter, GridGroups.GridWaterGroup);

            var takenCells = The.DB.Count<GridCellComponent>(GridGroups.GridWaterGroup);
            var spawnedCoins = The.DB.Count<CoinComponent>(CoinGroups.SpawnedCoinsGroup);

            Assert.AreEqual(0, freeCellFilter.filteredIndices.Count(), "All cells should be taken");
            Assert.AreEqual(takenCells, spawnedCoins, "Taken grid cells should equal spawned coins");
            Assert.AreEqual(coinsToSpawn - spawnedCoins, The.DB.Count<CoinComponent>(CoinGroups.RecycledCoinsGroup), "Some coins should be ready");
        }

        [UnityTest]
        public IEnumerator Only_One_Grid_Cell_Spawn_After_Pickup()
        {
            var coinsToSpawn = 2;
            The.CoinDefinition.Result.MaxCoins = coinsToSpawn;
            The.Context.Play();

            EGID cellEgid = A.GridCell.WithPosition(float2.zero);
            yield return new WaitForSeconds(0.5f);

            The.EntityFunctions.SwapEntityGroup<CoinEntityDescriptor>(0, CoinGroups.SpawnedCoinsGroup, CoinGroups.RecycledCoinsGroup);

            yield return new WaitForSeconds(0.5f);

            var spawnCount = The.DB.Count<CoinComponent>(CoinGroups.SpawnedCoinsGroup);
            Assert.AreEqual(1, spawnCount);
            Assert.IsTrue(The.DB.Exists<CoinComponent>(0, CoinGroups.RecycledCoinsGroup));
        }

        [UnityTest]
        public IEnumerator Only_One_Grid_Cell_Spawn_After_Multiple_Pickups()
        {
            var coinsToSpawn = 2;
            The.CoinDefinition.Result.CoinsSpawnTime = 0.1f;
            The.CoinDefinition.Result.MaxCoins = coinsToSpawn;
            The.Context.Play();

            EGID cellEgid = A.GridCell.WithPosition(float2.zero);
            yield return new WaitForSeconds(SmallWait);

            The.EntityFunctions.SwapEntityGroup<CoinEntityDescriptor>(0, CoinGroups.SpawnedCoinsGroup, CoinGroups.RecycledCoinsGroup);
            yield return new WaitForSeconds(SmallWait);

            The.EntityFunctions.SwapEntityGroup<CoinEntityDescriptor>(1, CoinGroups.SpawnedCoinsGroup, CoinGroups.RecycledCoinsGroup);
            yield return new WaitForSeconds(SmallWait);

            var spawnCount = The.DB.Count<CoinComponent>(CoinGroups.SpawnedCoinsGroup);
            Assert.AreEqual(1, spawnCount);
            Assert.IsTrue(The.DB.Exists<CoinComponent>(0, CoinGroups.SpawnedCoinsGroup));
            Assert.IsTrue(The.DB.Exists<CoinComponent>(1, CoinGroups.RecycledCoinsGroup));
        }

        [UnityTest]
        public IEnumerator Three_Grid_Cells_Spawn_After_Multiple_Pickups()
        {
            var coinsToSpawn = 4;
            The.CoinDefinition.Result.CoinsSpawnTime = 0.1f;
            The.CoinDefinition.Result.MaxCoins = coinsToSpawn;
            The.Context.Play();

            EGID cellEgid1 = A.GridCell.WithPosition(float2.zero);
            EGID cellEgid2 = A.GridCell.WithPosition(new float2(1, 0));
            EGID cellEgid3 = A.GridCell.WithPosition(new float2(2, 0));
            yield return new WaitForSeconds(SmallWait);

            The.EntityFunctions.SwapEntityGroup<CoinEntityDescriptor>(0, CoinGroups.SpawnedCoinsGroup, CoinGroups.RecycledCoinsGroup);
            yield return new WaitForSeconds(SmallWait);

            The.EntityFunctions.SwapEntityGroup<CoinEntityDescriptor>(3, CoinGroups.SpawnedCoinsGroup, CoinGroups.RecycledCoinsGroup);
            yield return new WaitForSeconds(SmallWait);

            The.EntityFunctions.SwapEntityGroup<CoinEntityDescriptor>(0, CoinGroups.SpawnedCoinsGroup, CoinGroups.RecycledCoinsGroup);
            yield return new WaitForSeconds(SmallWait);

            The.EntityFunctions.SwapEntityGroup<CoinEntityDescriptor>(3, CoinGroups.SpawnedCoinsGroup, CoinGroups.RecycledCoinsGroup);
            yield return new WaitForSeconds(SmallWait);

            var spawnCount = The.DB.Count<CoinComponent>(CoinGroups.SpawnedCoinsGroup);
            Assert.AreEqual(3, spawnCount);
            Assert.IsTrue(The.DB.Exists<CoinComponent>(0, CoinGroups.SpawnedCoinsGroup));
            Assert.IsTrue(The.DB.Exists<CoinComponent>(3, CoinGroups.RecycledCoinsGroup));
        }

        const float SmallWait = 0.3f;
    }
}
