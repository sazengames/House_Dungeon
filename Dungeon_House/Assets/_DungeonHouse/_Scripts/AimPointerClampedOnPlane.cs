using System;
using GameCreator.Runtime.Cameras;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Shooter
{
    [Title("Pointer on Plane (Clamped)")]
    [Description("Aims towards the pointer projected onto a plane, clamped to a cone around the character's head bone")]

    [Category("Pointer on Plane (Clamped)")]
    [Image(typeof(IconCursor), ColorTheme.Type.Yellow)]

    [Serializable]
    public class AimPointerClampedOnPlane : TAim
    {
        private enum Axis
        {
            XZ,
            XY,
            YZ
        }

        private const float INFINITY = 999f;

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Camera = GetGameObjectCameraMain.Create;
        [SerializeField]
        private InputPropertyValueVector2 m_Cursor = new InputPropertyValueVector2(
            new InputValueVector2MousePosition()
        );

        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectSelf.Create();

        [SerializeField] private PropertyGetDecimal m_MinDistance = GetDecimalConstantPointOne.Create;
        [SerializeField] private Axis m_Plane = Axis.XZ;

        [SerializeField] private float m_MaxPitchUp = 45f;
        [SerializeField] private float m_MaxPitchDown = 30f;
        [SerializeField] private float m_MaxYaw = 90f;

        [SerializeField] private float m_SmoothTime = 0f;
        [SerializeField] private float m_InitialDelay = 0f;

        // RUNTIME STATE: -------------------------------------------------------------------------

        [NonSerialized] private Vector3 m_SmoothedPoint;
        [NonSerialized] private bool m_HasPreviousPoint;
        [NonSerialized] private float m_ElapsedSinceEnter;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override Vector3 GetPoint(Args args)
        {
            Camera camera = this.m_Camera.Get<Camera>(args);
            if (camera == null) return default;

            Transform target = this.m_Target.Get<Transform>(args);
            if (target == null) return default;

            float minDistance = (float) this.m_MinDistance.Get(args);
            Vector2 cursor = this.m_Cursor.Read();

            Vector3 rawPoint = this.RaycastFromCursor(camera, cursor, target, minDistance);
            Vector3 clampedPoint = this.ClampToHeadCone(rawPoint, target);

            return this.SmoothPoint(clampedPoint);
        }

        public override void Enter(Character character)
        {
            base.Enter(character);
            this.m_Cursor.OnStartup();
            this.m_ElapsedSinceEnter = 0f;

            if (this.m_InitialDelay > 0f)
            {
                Animator animator = character.Animim.Animator;
                Transform headBone = animator != null
                    ? animator.GetBoneTransform(HumanBodyBones.Head)
                    : null;

                Vector3 origin = headBone != null ? headBone.position : character.transform.position;
                this.m_SmoothedPoint = origin + character.transform.forward * INFINITY;
                this.m_HasPreviousPoint = true;
            }
            else
            {
                this.m_HasPreviousPoint = false;
            }
        }

        public override void Exit(Character character)
        {
            base.Exit(character);
            this.m_Cursor.OnDispose();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private Vector3 RaycastFromCursor(
            Camera camera, Vector2 cursor, Transform target, float minDistance)
        {
            Ray ray = camera.ScreenPointToRay(cursor);

            Plane plane = new Plane(
                this.m_Plane switch
                {
                    Axis.XZ => Vector3.up,
                    Axis.XY => Vector3.forward,
                    Axis.YZ => Vector3.right,
                    _ => throw new ArgumentOutOfRangeException()
                },
                target.position
            );

            return plane.Raycast(ray, out float distance) && distance >= minDistance
                ? ray.GetPoint(distance)
                : target.TransformPoint(Vector3.forward * INFINITY);
        }

        private Vector3 ClampToHeadCone(Vector3 rawPoint, Transform target)
        {
            Character character = target.GetComponent<Character>();
            if (character == null) return rawPoint;

            Animator animator = character.Animim.Animator;
            if (animator == null) return rawPoint;

            Transform headBone = animator.GetBoneTransform(HumanBodyBones.Head);
            if (headBone == null) return rawPoint;

            Vector3 headPos = headBone.position;
            Vector3 toTarget = rawPoint - headPos;
            float distance = toTarget.magnitude;

            if (distance < 0.001f) return rawPoint;

            Vector3 dir = toTarget / distance;
            Vector3 charForward = target.forward;

            // Pitch via asin: positive = up, negative = down. Continuous everywhere.
            float pitch = Mathf.Asin(Mathf.Clamp(dir.y, -1f, 1f)) * Mathf.Rad2Deg;

            // Yaw: horizontal angle relative to character forward
            Vector3 dirFlat = new Vector3(dir.x, 0f, dir.z);
            float dirFlatMag = dirFlat.magnitude;
            float yaw;

            if (dirFlatMag > 0.001f)
            {
                dirFlat /= dirFlatMag;
                yaw = Vector3.SignedAngle(charForward, dirFlat, Vector3.up);
            }
            else
            {
                // Aiming nearly straight up or down — yaw is irrelevant, keep character forward
                yaw = 0f;
            }

            // Clamp: positive pitch = up (limited by MaxPitchUp), negative = down (limited by MaxPitchDown)
            float clampedPitch = Mathf.Clamp(pitch, -this.m_MaxPitchDown, this.m_MaxPitchUp);
            float clampedYaw = Mathf.Clamp(yaw, -this.m_MaxYaw, this.m_MaxYaw);

            // Early out if no clamping was needed
            if (Mathf.Approximately(pitch, clampedPitch) && Mathf.Approximately(yaw, clampedYaw))
            {
                return rawPoint;
            }

            // Reconstruct direction from clamped angles using simple trig
            float pitchRad = clampedPitch * Mathf.Deg2Rad;
            float cosPitch = Mathf.Cos(pitchRad);

            Vector3 yawDir = Quaternion.AngleAxis(clampedYaw, Vector3.up) * charForward;
            Vector3 clampedDir = new Vector3(
                yawDir.x * cosPitch,
                Mathf.Sin(pitchRad),
                yawDir.z * cosPitch
            ).normalized;

            return headPos + clampedDir * distance;
        }

        private Vector3 SmoothPoint(Vector3 target)
        {
            this.m_ElapsedSinceEnter += Time.deltaTime;

            float effectiveSmoothTime = this.m_ElapsedSinceEnter < this.m_InitialDelay
                ? this.m_InitialDelay
                : this.m_SmoothTime;

            if (effectiveSmoothTime <= 0f || !this.m_HasPreviousPoint)
            {
                this.m_SmoothedPoint = target;
                this.m_HasPreviousPoint = true;
                return target;
            }

            float t = 1f - Mathf.Exp(-Time.deltaTime / effectiveSmoothTime);
            this.m_SmoothedPoint = Vector3.Lerp(this.m_SmoothedPoint, target, t);
            return this.m_SmoothedPoint;
        }
    }
}
