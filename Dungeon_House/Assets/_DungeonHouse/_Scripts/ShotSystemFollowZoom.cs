using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Cameras
{
    [Serializable]
    public class ShotSystemFollowZoom : TShotSystem
    {
        public static readonly int ID = nameof(ShotSystemFollowZoom).GetHashCode();

        private static readonly Vector3 DEFAULT_DISTANCE = new Vector3(0f, 2f, -3f);

        private const float MIN_RADIUS = 0.01f;

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Follow = GetGameObjectPlayer.Create();

        [SerializeField] private PropertyGetDirection m_Distance = new PropertyGetDirection(DEFAULT_DISTANCE);

        [NonSerialized] private ShotSystemZoom m_Zoom;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override int Id => ID;

        public Transform Follow
        {
            set => this.m_Follow = GetGameObjectInstance.Create(value);
        }

        public Vector3 Distance
        {
            set => this.m_Distance = new PropertyGetDirection(value);
        }

        /// <summary>
        /// The current distance between the camera and the followed target after zoom is applied
        /// </summary>
        [field: NonSerialized] public float Radius { get; private set; }

        // IMPLEMENTS: ----------------------------------------------------------------------------

        public override void OnAwake(TShotType shotType)
        {
            base.OnAwake(shotType);

            this.m_Zoom = shotType.GetSystem(ShotSystemZoom.ID) as ShotSystemZoom;
        }

        public override void OnUpdate(TShotType shotType)
        {
            base.OnUpdate(shotType);

            GameObject followValue = this.m_Follow.Get(shotType.Args);
            Vector3 distanceValue = this.m_Distance.Get(shotType.Args);

            if (followValue == null) return;

            Vector3 offset = this.CalculateZoomedOffset(distanceValue);
            shotType.Position = followValue.transform.position + offset;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private Vector3 CalculateZoomedOffset(Vector3 distance)
        {
            float baseRadius = distance.magnitude;
            this.Radius = baseRadius;

            // Without a zoom system (or a degenerate offset) behave like a plain follow shot
            if (this.m_Zoom == null || baseRadius <= float.Epsilon) return distance;

            float minDistance = this.m_Zoom.MinDistance;

            float maxRadius = Mathf.Max(MIN_RADIUS, baseRadius - minDistance);
            this.Radius = minDistance + maxRadius * this.m_Zoom.Level;

            return distance / baseRadius * this.Radius;
        }
    }
}
