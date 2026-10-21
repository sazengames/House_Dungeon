using System;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Cameras
{
    [Title("Follow Target (Zoom)")]
    [Category("Follow Target Zoom")]
    [Image(typeof(IconShotFollow), ColorTheme.Type.Blue)]

    [Description("Follows the target from a certain distance and allows zooming in and out")]

    [Serializable]
    public class ShotTypeFollowZoom : TShotTypeLook
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private ShotSystemZoom m_Zoom;
        [SerializeField] private ShotSystemFollowZoom m_ShotSystemFollowZoom;

        // PROPERTIES: ----------------------------------------------------------------------------

        public ShotSystemZoom Zoom => this.m_Zoom;
        public ShotSystemFollowZoom Follow => this.m_ShotSystemFollowZoom;

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public ShotTypeFollowZoom()
        {
            this.m_Zoom = new ShotSystemZoom();
            this.m_ShotSystemFollowZoom = new ShotSystemFollowZoom();

            this.m_ShotSystems.Add(this.m_ShotSystemLook.Id, this.m_ShotSystemLook);
            this.m_ShotSystems.Add(this.m_Zoom.Id, this.m_Zoom);
            this.m_ShotSystems.Add(this.m_ShotSystemFollowZoom.Id, this.m_ShotSystemFollowZoom);
        }

        // OVERRIDE METHODS: ----------------------------------------------------------------------

        protected override void OnBeforeAwake(ShotCamera shotCamera)
        {
            base.OnBeforeAwake(shotCamera);

            this.m_Zoom?.OnAwake(this);
            this.m_ShotSystemFollowZoom?.OnAwake(this);
        }

        protected override void OnBeforeStart(ShotCamera shotCamera)
        {
            base.OnBeforeStart(shotCamera);

            this.m_Zoom?.OnStart(this);
            this.m_ShotSystemFollowZoom?.OnStart(this);
        }

        protected override void OnBeforeDestroy(ShotCamera shotCamera)
        {
            base.OnBeforeDestroy(shotCamera);

            this.m_Zoom?.OnDestroy(this);
            this.m_ShotSystemFollowZoom?.OnDestroy(this);
        }

        protected override void OnBeforeEnable(TCamera camera)
        {
            base.OnBeforeEnable(camera);

            this.m_Zoom?.OnEnable(this, camera);
            this.m_ShotSystemFollowZoom?.OnEnable(this, camera);
        }

        protected override void OnBeforeDisable(TCamera camera)
        {
            base.OnBeforeDisable(camera);

            this.m_Zoom?.OnDisable(this, camera);
            this.m_ShotSystemFollowZoom?.OnDisable(this, camera);
        }

        protected override void OnBeforeUpdate()
        {
            base.OnBeforeUpdate();

            // Zoom must update first so the follow system reads the current zoom level
            this.m_Zoom?.OnUpdate(this);
            this.m_ShotSystemFollowZoom?.OnUpdate(this);
        }

        // GIZMOS: --------------------------------------------------------------------------------

        public override void DrawGizmos(Transform transform)
        {
            base.DrawGizmos(transform);
            if (!Application.isPlaying) return;

            this.m_Zoom?.OnDrawGizmos(this, transform);
            this.m_ShotSystemFollowZoom?.OnDrawGizmos(this, transform);
        }

        public override void DrawGizmosSelected(Transform transform)
        {
            base.DrawGizmosSelected(transform);
            if (!Application.isPlaying) return;

            this.m_Zoom?.OnDrawGizmosSelected(this, transform);
            this.m_ShotSystemFollowZoom?.OnDrawGizmosSelected(this, transform);
        }
    }
}
