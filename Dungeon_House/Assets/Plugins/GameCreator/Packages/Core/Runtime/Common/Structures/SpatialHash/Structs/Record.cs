using Unity.Mathematics;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
    internal struct Record
    {
        public EntityId UniqueCode { get; }
        public float3 Position { get; }
        public bool IsDynamic { get; }

        public Record(EntityId uniqueCode, float3 position, bool isDynamic)
        {
            this.UniqueCode = uniqueCode;
            this.Position = position;
            this.IsDynamic = isDynamic;
        }
    }
}