using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
	[AddComponentMenu("")]
    [DisallowMultipleComponent]
	public class PoolManager : Singleton<PoolManager>
	{
        // PROPERTIES: ----------------------------------------------------------------------------
        
        [field: NonSerialized] private Dictionary<PoolKey, PoolData> Collection { get; set; }

        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void OnCreate()
        {
            base.OnCreate();
            this.Collection = new Dictionary<PoolKey, PoolData>();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public GameObject GetLastPicked(GameObject prefab)
        {
            if (prefab == null) return null;
            return this.Collection.TryGetValue(PoolKey.FromPrefab(prefab), out PoolData data)
                ? data.LastGet
                : null;
        }
        
        public GameObject Pick(GameObject prefab, int count, float duration = -1f)
        {
            if (prefab == null) return null;
            PoolKey poolKey = PoolKey.FromPrefab(prefab);

            if (!this.Collection.ContainsKey(poolKey)) this.CreatePool(prefab, count);
            return this.Collection[poolKey].Get(Vector3.zero, Quaternion.identity, duration);
        }

        public GameObject Pick(GameObject prefab, Vector3 position, Quaternion rotation, int count, float duration = -1f)
        {
            if (prefab == null) return null;
            PoolKey poolKey = PoolKey.FromPrefab(prefab);

            if (!this.Collection.ContainsKey(poolKey)) this.CreatePool(prefab, count);
            return this.Collection[poolKey].Get(position, rotation, duration);
        }

        public GameObject Pick(int collectionId, Vector3 position, Quaternion rotation, int count, float duration = -1f)
        {
            PoolKey poolKey = PoolKey.FromCollection(collectionId);
            
            if (!this.Collection.ContainsKey(poolKey)) this.CreatePool(collectionId, count);
            return this.Collection[poolKey].Get(position, rotation, duration);
        }

        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null) return;
            PoolKey poolKey = PoolKey.FromPrefab(prefab);
            
            if (this.Collection.TryGetValue(poolKey, out PoolData pool))
            {
                int currentPool = pool.ReadyCount;
                int prewarmCount = count - currentPool;
                
                if (prewarmCount > 0) pool.Prewarm(prewarmCount);
            }
            else
            {
                this.CreatePool(prefab, count);   
            }
        }
        
        public void Dispose(GameObject prefab)
        {
            if (prefab == null) return;
            PoolKey poolKey = PoolKey.FromPrefab(prefab);
            
            if (this.Collection.Remove(poolKey, out PoolData pool))
            {
                pool.Dispose();
            }
        }

        public void DontDestroyOnLoadPool(GameObject prefab)
        {
            if (prefab == null) return;
            PoolKey poolKey = PoolKey.FromPrefab(prefab);
            
            if (this.Collection.TryGetValue(poolKey, out PoolData pool))
            {
                pool.SetDontDestroyOnLoad();
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreatePool(GameObject prefab, int count)
        {
            PoolKey poolKey = PoolKey.FromPrefab(prefab);
            this.Collection.Add(poolKey, new PoolData(prefab, count));
        }
        
        private void CreatePool(int collectionId, int count)
        {
            this.Collection.Add(PoolKey.FromCollection(collectionId), new PoolData(collectionId, count));
        }
        
        // INTERNAL CALLBACKS: --------------------------------------------------------------------

        internal void OnDisableInstance(PoolKey prefabId, PoolInstance instance)
        {
            if (this.Collection.TryGetValue(prefabId, out PoolData poolData))
            {
                poolData.OnDisableInstance(instance);
                return;
            }
            
            if (instance == null) return;
            Destroy(instance.gameObject);
        }
        
        internal void OnDestroyInstance(PoolKey prefabId, PoolInstance instance)
        {
            if (this.Collection.TryGetValue(prefabId, out PoolData poolData))
            {
                poolData.OnDestroyInstance(instance);
                return;
            }
            
            if (instance == null) return;
            Destroy(instance.gameObject);
        }
    }
}