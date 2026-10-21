using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(1, 0, 0)]

[Title("Set rigidbody sleep")]
[Description("Set rigidbody sleep")]
[Category("Physics 3D/Set rigidbody sleep")]

[Keywords("Physics", "Rigidbody","Sleep")]
[Image(typeof(IconPhysics), ColorTheme.Type.Yellow, typeof(OverlayArrowRight))]
[Serializable]
public class InstructionSetRigidbodySleep : Instruction
{
    [SerializeField] private PropertyGetGameObject m_Rigidbody = GetGameObjectSelf.Create();
    
    public override string Title => $"Set{this.m_Rigidbody} to sleep";
    protected override Task Run(Args args)
    {
        GameObject gameObject = this.m_Rigidbody.Get(args);

        Rigidbody rigidbody = gameObject.Get<Rigidbody>();
        rigidbody.Sleep();
        return DefaultResult;
    }
}
