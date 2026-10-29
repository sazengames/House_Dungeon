using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Characters.IK
{
    // Asset that holds a hand pose for the "Set Hand IK" instruction, like a Shooter Sight.
    // The rig reads it every frame, so you can tweak it in Play mode and the hand updates
    // immediately. Because it's an asset, the changes are kept after you stop Play mode.

    [CreateAssetMenu(
        fileName = "Hand IK Pose",
        menuName = "Game Creator/Characters/Hand IK Pose",
        order    = 200
    )]

    [Serializable]
    public class HandIKPose : ScriptableObject, IHandIKSettings
    {
        [Tooltip("Offset from the target, in the target's local space")]
        [SerializeField] private Vector3 m_PositionOffset = Vector3.zero;

        [Tooltip("If on, the hand also rotates to match the target")]
        [SerializeField] private bool m_MatchRotation = true;

        [Tooltip("Extra rotation applied on top of the target's rotation")]
        [SerializeField] private Vector3 m_RotationOffset = Vector3.zero;

        [Tooltip("How strongly the hand is pulled to the target")]
        [SerializeField] [Range(0f, 1f)] private float m_Weight = 1f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Vector3 PositionOffset => this.m_PositionOffset;
        public Vector3 RotationOffset => this.m_RotationOffset;
        public bool MatchRotation => this.m_MatchRotation;
        public float Weight => this.m_Weight;

        // Always live: the rig re-reads the asset every frame.
        public bool LiveUpdate => true;
    }
}
