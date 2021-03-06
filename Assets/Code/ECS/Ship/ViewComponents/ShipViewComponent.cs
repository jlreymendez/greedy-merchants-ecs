﻿using GreedyMerchants.ECS.Common;
using Svelto.ECS;
using Svelto.ECS.Hybrid;

namespace GreedyMerchants.ECS.Ship
{
    public struct ShipViewComponent : IEntityViewComponent
    {
        public ITransformComponent Transform;
        public ISpriteRendererComponent Renderer;
        public IPhysicsComponent Physics;
        public IShipExplosionComponent Explosion;
        public IShipTriggerComponent Trigger;
        public IShipAudioComponent Audio;
        public IShipCoinsHudComponent CoinsHud;
        public IShipPointsHudComponent PointsHud;

        public EGID ID { get; set; }
    }
}