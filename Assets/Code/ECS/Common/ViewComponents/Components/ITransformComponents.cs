﻿using Unity.Mathematics;

namespace GreedyMerchants.ECS.Common
{
    public interface IPositionComponent
    {
        float3 Position { get; set; }
    }

    public interface IRotationComponent
    {
        quaternion Rotation { get; set; }
    }

    public interface IScaleComponent
    {
        float3 Scale { get; set; }
    }

    public interface ITransformComponent : IPositionComponent, IRotationComponent, IScaleComponent { }
}