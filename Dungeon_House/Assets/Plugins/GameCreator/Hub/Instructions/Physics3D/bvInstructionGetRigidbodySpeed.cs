using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;


    namespace bvScript
    {

        [Version(1, 0, 0)]
        [Title("Get Rigibdoy Speed")]
        [Description("Returns the speed of a rigidbody in Vector3 and/or float magnitude")]

        [Category("Physics 3D/GetRigidBodySpeed")]

        [Parameter("Rigidbody", "Rigidbody to Measure")]
        [Parameter("Vector3", "Vector3 to store")]
        [Parameter("Magnitude", "Float to store")]

        [Keywords("Speed", "Physics", "RigidBody", "Variable", "bv")]

        [Image(typeof(IconAnimator), ColorTheme.Type.Blue)]

        [Serializable]
        public class bvInstructionGetRigidbodySpeed : Instruction
        {
            [SerializeField] private PropertyGetGameObject currentRB = GetGameObjectSelf.Create();
            [Space]
            [SerializeField] private PropertySetVector3 vec3Velocity = new PropertySetVector3();
            [SerializeField] private PropertySetNumber magVelocity = new PropertySetNumber();

        public override string Title => $"Measure {this.currentRB} speed";
        protected override Task Run(Args args)
            {

                GameObject m_gameObject = this.currentRB.Get(args);
                if (m_gameObject == null) return DefaultResult;

                Rigidbody m_rigidbody = m_gameObject.Get<Rigidbody>();
                if (m_rigidbody == null) return DefaultResult;

                Vector3 currentVelocity = m_rigidbody.linearVelocity;
                float tempVelocity = currentVelocity.magnitude;

                magVelocity.Set(tempVelocity, args);
                vec3Velocity.Set(currentVelocity, args);

                return DefaultResult;
            }
        }
    }