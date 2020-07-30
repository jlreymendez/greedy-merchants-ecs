﻿using Svelto.ECS.Hybrid;
using UnityEngine;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Common
{
    [SelectionBase]
    public class TransformImplementor : MonoBehaviour, IImplementor, ITransformComponent
    {
        public float3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public quaternion Rotation
        {
            get => transform.rotation;
            set => transform.rotation = value;
        }

        public float3 Forward
        {
            get => transform.forward;
            set => transform.forward = value;
        }

        public float3 Right
        {
            get => transform.right;
            set => transform.right = value;
        }

        public float3 Up
        {
            get => transform.up;
            set => transform.up = value;
        }

        public float3 Scale
        {
            get => transform.localScale;
            set => transform.localScale = value;
        }
    }
}