using System;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
    /// <summary>
    /// Identifies a pool. Pools created from a prefab are identified by the prefab's Entity ID,
    /// while prefab-less pools are identified by an arbitrary collection ID.
    /// </summary>
    internal readonly struct PoolKey : IEquatable<PoolKey>
    {
        // MEMBERS: -------------------------------------------------------------------------------

        private readonly EntityId m_EntityId;
        private readonly int m_CollectionId;

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        private PoolKey(EntityId entityId, int collectionId)
        {
            this.m_EntityId = entityId;
            this.m_CollectionId = collectionId;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static PoolKey FromPrefab(GameObject prefab)
        {
            return new PoolKey(prefab.GetEntityId(), 0);
        }

        public static PoolKey FromCollection(int collectionId)
        {
            return new PoolKey(EntityId.None, collectionId);
        }

        // EQUALITY: ------------------------------------------------------------------------------

        public bool Equals(PoolKey other)
        {
            return this.m_EntityId.Equals(other.m_EntityId) &&
                   this.m_CollectionId == other.m_CollectionId;
        }

        public override bool Equals(object obj) => obj is PoolKey other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.m_EntityId, this.m_CollectionId);

        public override string ToString()
        {
            return this.m_EntityId == EntityId.None
                ? this.m_CollectionId.ToString()
                : this.m_EntityId.ToString();
        }
    }
}
