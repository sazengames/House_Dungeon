
using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.AI;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(1, 0, 3)]

    [Title("Wander")]
    [Description("[Requires NavMesh Agent] Sets a target destination for the character using the character's NavMeshAgent, then moves the character to that location.")]

    [Category("Characters/Navigation/Wander")]

    [Parameter("Character", "The character game object")]
    [Parameter("Layer Mask", "The new seed integer value")]
    [Parameter("Min Travel Distance", "Minimum Travel Distance")]
    [Parameter("Max Travel Distance", "Maximum Travel Distance")]
    [Parameter("Home", "Position from which travel distance is calculated")]
    [Parameter("Wait to Finish", "If true this Instruction waits until the Character reaches its destination or it is canceled")]
    [Parameter("Stop Distance", "Distance to the destination that the Character considers it has reached the target")]

    [Keywords("Walk", "Wander", "Patrol", "Roaming", "Run", "Position", "Location", "Destination")]

    [Image(typeof(IconCharacterWalk), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionWander : TInstructionCharacterNavigation
    {
        [SerializeField] private LayerMask layerMask = -1;
        [SerializeField] private float minTravelDistance = 2.0f;
        [SerializeField] private float maxTravelDistance = 4.0f;
        [SerializeField] private PropertyGetPosition home = GetPositionCharacter.Create;

        [SerializeField] private bool m_WaitToArrive = true;
        [SerializeField] private float m_StopDistance = 0f;

        private bool m_MovementComplete;

        public override string Title => $"{m_Character} Wander";

        protected override async Task Run(Args args)
        {

            this.m_MovementComplete = false;

            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return;

            float distance = UnityEngine.Random.Range(minTravelDistance, maxTravelDistance);

            character.Motion.MoveToLocation(
                RandomNavSphere(home.Get(args), distance, layerMask),
                this.m_StopDistance,
                OnFinish
            );

            if (this.m_WaitToArrive)
            {
                await this.Until(() => this.m_MovementComplete);
            }
        }

        Location RandomNavSphere(Vector3 origin, float distance, LayerMask layerMask)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
            randomDirection += origin;
            NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, distance, layerMask);
            return new Location(navHit.position);
        }

        private void OnFinish(Character character, bool success)
        {
            this.m_MovementComplete = true;
        }
    }
}