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
        float3 Forward { get; set; }
        float3 Right { get; set; }
        float3 Up { get; set; }
    }

    public interface IScaleComponent
    {
        float3 Scale { get; set; }
    }

    public interface ITransformComponent : IPositionComponent, IRotationComponent, IScaleComponent { }
}