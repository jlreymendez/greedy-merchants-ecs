using System;
using Svelto.ECS.Serialization;

namespace Svelto.ECS
{
    /// <summary>
    /// Inherit from an ExtendibleEntityDescriptor to extend a base entity descriptor that can be used
    /// to swap and remove specialized entities from abstract engines
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public class ExtendibleEntityDescriptor<TType> : IEntityDescriptor where TType : IEntityDescriptor, new()
    {
        static ExtendibleEntityDescriptor()
        {
            if (typeof(ISerializableEntityDescriptor).IsAssignableFrom(typeof(TType)))
                throw new Exception(
                    $"SerializableEntityDescriptors cannot be used as base entity descriptor: {typeof(TType)}");
        }

        public ExtendibleEntityDescriptor(IComponentBuilder[] extraEntities)
        {
            _dynamicDescriptor = new DynamicEntityDescriptor<TType>(extraEntities);
        }

        public ExtendibleEntityDescriptor()
        {
            _dynamicDescriptor = new DynamicEntityDescriptor<TType>(true);
        }

        public ExtendibleEntityDescriptor<TType> ExtendWith<T>() where T : IEntityDescriptor, new()
        {
            _dynamicDescriptor.ExtendWith<T>();

            return this;
        }

        public ExtendibleEntityDescriptor<TType> ExtendWith(IComponentBuilder[] extraEntities)
        {
            _dynamicDescriptor.ExtendWith(extraEntities);

            return this;
        }

        public IComponentBuilder[] componentsToBuild => _dynamicDescriptor.componentsToBuild;

        DynamicEntityDescriptor<TType> _dynamicDescriptor;
    }
}