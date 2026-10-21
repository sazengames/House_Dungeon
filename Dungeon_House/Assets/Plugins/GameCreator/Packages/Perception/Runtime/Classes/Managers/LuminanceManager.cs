using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Perception
{
    [AddComponentMenu("")]
    public class LuminanceManager : Singleton<LuminanceManager> 
    {
        private struct LuminanceData
        {
            // PROPERTIES: ------------------------------------------------------------------------
            
            public int Frame { get; }
            public Vector3 Position { get; }
            public float Luminance { get; }
            
            // CONSTRUCTOR: -----------------------------------------------------------------------
            
            public LuminanceData(int frame, Vector3 position, float luminance)
            {
                this.Frame = frame;
                this.Position = position;
                this.Luminance = luminance;
            }
        }
        
        ///////////////////////////////////////////////////////////////////////////////////////////
        
        private const float TIMEOUT_CLEAR_CACHE = 100f;
        
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private readonly Dictionary<EntityId, Luminance> m_Instances = new Dictionary<EntityId, Luminance>();

        [NonSerialized] 
        private readonly Dictionary<EntityId, LuminanceData> m_Cache = new Dictionary<EntityId, LuminanceData>();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        [field: NonSerialized] public float AmbientIntensity { get; set; }

        public IReadOnlyDictionary<EntityId, Luminance> Instances => this.m_Instances;
        
        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void OnCreate()
        {
            base.OnCreate();
            
            ScheduleManager.Instance.RunInterval(
                this.ClearCache,
                TIMEOUT_CLEAR_CACHE,
                TimeMode.UpdateMode.UnscaledTime
            );
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Insert(Luminance instance)
        {
            EntityId instanceId = instance.GetEntityId();
            this.m_Instances[instanceId] = instance;
        }
        
        public void Remove(Luminance instance)
        {
            EntityId instanceId = instance.GetEntityId();
            this.m_Instances.Remove(instanceId);
        }
        
        public float LuminanceAt(Transform target)
        {
            EntityId targetId = target.GetEntityId();
            if (this.m_Cache.TryGetValue(targetId, out LuminanceData data))
            {
                if (data.Frame == Time.frameCount && target.position == data.Position)
                {
                    return data.Luminance;
                }
            }
            
            float intensity = this.AmbientIntensity;
            
            foreach (KeyValuePair<EntityId, Luminance> entry in this.Instances)
            {
                Luminance instance = entry.Value;
                if (instance == null) continue;
                intensity += instance.IntensityTo(target);
            }
            
            this.m_Cache[targetId] = new LuminanceData(
                Time.frameCount,
                target.position,
                intensity
            );
            
            return intensity;
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ClearCache()
        {
            this.m_Cache.Clear();
        }
    }
}