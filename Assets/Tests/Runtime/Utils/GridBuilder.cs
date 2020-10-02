using System.Collections;
using GreedyMerchants.ECS.Grid;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GreedyMerchants.Tests.Runtime.Utils
{
    public class GridContext
    {
        public Grid Grid { get; private set; }
        public GridTilemapRepresentation Land { get; private set; }
        public GridUtils Utils { get; private set; }

        GameObject _gameObject;
        Grid _grid;
        Tilemap _tilemap;
        AssetLoader<GameObject> _loader;

        public GridContext(string key, GridUtils utils)
        {
            Utils = utils;
            _loader = new AssetLoader<GameObject>(key);
        }

        public IEnumerator Instantiate()
        {
            yield return _loader.Instantiate();
            _gameObject = _loader.Result;
            Grid = _gameObject.GetComponent<Grid>();
            var tilemap = _gameObject.transform.Find("Land").GetComponent<Tilemap>();
            Land = new GridTilemapRepresentation(Grid, tilemap, Utils);
        }
    }

    public class GridBuilder
    {
        string _key = "GridPrefab";
        GridUtils _utils = new GridUtils(new uint2(30, 18), new float2(1));

        public GridContext Build()
        {
            return new GridContext(_key, _utils);
        }

        public GridBuilder WithKey(string key)
        {
            _key = key;
            return this;
        }

        public GridBuilder WithUtils(GridUtils utils)
        {
            _utils = utils;
            return this;
        }

        public static implicit operator GridContext(GridBuilder grid)
        {
            return grid.Build();
        }
    }
}
