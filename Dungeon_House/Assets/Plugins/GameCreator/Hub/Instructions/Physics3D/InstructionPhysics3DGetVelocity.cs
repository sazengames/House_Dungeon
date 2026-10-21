using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(1, 1, 1)]

    [Title("Get Velocity 3D")]
    [Description("Get Velocity 3D of Rigidbody in various measures")]

    [Category("Physics 3D/Get Velocity 3D")]

    [Parameter("Set", "Where the value is set")]
    [Parameter("GameObject", "GameObject with Rigidbody")]
    [Parameter("Measure", "Measure of Velocity")]
    [Parameter("Round to", "Round to digits")]

    [Keywords("Velocity", "Rigidbody", "Physics", "Physics 3D")]
    [Image(typeof(IconPhysics), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionPhysics3DGetVelocity : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private PropertySetNumber m_Set = SetNumberGlobalName.Create;

        [SerializeField] 
        private PropertyGetGameObject m_GameObject = GetGameObjectSelf.Create();

        [SerializeField]
        private Measure m_Measure = Measure.MetersPerSecond;
        private enum Measure
        {
            MetersPerSecond,
            KilometersPerHour,
            MilesPerHour
        }

        [SerializeField]
        private PropertyGetInteger m_RoundTo = new PropertyGetInteger();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Set {this.m_Set} = {this.m_GameObject}";

        // RUN METHOD: ----------------------------------------------------------------------------
        
        protected override Task Run(Args args)
        {
            Rigidbody m_Rigidbody = this.m_GameObject.Get(args).GetComponentInChildren<Rigidbody>();
            double m_Velocity;
            #if UNITY_2023_1_OR_NEWER
                m_Velocity = m_Rigidbody.linearVelocity.magnitude;
            #else
                m_Velocity = m_Rigidbody.velocity.magnitude;
            #endif

            // Velocity value using chosen measure
            double m_VelocityFinalValue = 0;
            int digits = (int)m_RoundTo.Get(args);


            switch (m_Measure)
            {
                case Measure.MetersPerSecond:
                    m_VelocityFinalValue = Math.Round(m_Velocity, digits);
                    break;
                case Measure.KilometersPerHour:
                    m_VelocityFinalValue = Math.Round(m_Velocity * 3600f / 1000f, digits);
                    break;
                case Measure.MilesPerHour:
                    m_VelocityFinalValue = Math.Round(m_Velocity * 3600f / 1609.34, digits);
                    break;
                default:
                    break;
            }
            
            this.m_Set.Set(m_VelocityFinalValue, args);

            return DefaultResult;
        }
    }
}