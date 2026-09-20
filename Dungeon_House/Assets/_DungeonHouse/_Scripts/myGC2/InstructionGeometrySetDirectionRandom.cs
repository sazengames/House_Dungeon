using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(1, 0, 0)]
[Title("Set Direction Random")]
[Description("Changes the value of a Vector3 randomly based on minimum and maximum values")]

[Category("Math/Geometry/Set Direction Random")]

[Parameter("Set", "Dynamic variable where the resulting value is set")]
[Parameter("Min", "The minimum value that could be set")]
[Parameter("Max", "The maximum value that could be set")]

[Keywords("Change", "Vector3", "Vector2", "Towards", "Look", "Variable", "Random")]
[Image(typeof(IconVector3), ColorTheme.Type.Green, typeof(OverlayArrowRight))]

[Serializable]
public class InstructionGeometrySetDirectionRandom : Instruction
{
    // MEMBERS: -------------------------------------------------------------------------------

    [SerializeField]
    private PropertySetVector3 m_Set = SetVector3None.Create;

    [SerializeField]
    private PropertyGetDirection m_Min = new PropertyGetDirection();
    [SerializeField]
    private PropertyGetDirection m_Max = new PropertyGetDirection();
    // PROPERTIES: ----------------------------------------------------------------------------

    public override string Title => $"Set Direction {this.m_Set} = Random from {this.m_Min} to {this.m_Max}";

    // RUN METHOD: ----------------------------------------------------------------------------

    protected override Task Run(Args args)
    {
        //Vector3 value = this.m_From.Get(args);
        Vector3 value = new Vector3(
            UnityEngine.Random.Range(this.m_Min.Get(args).x, this.m_Max.Get(args).x), 
            UnityEngine.Random.Range(this.m_Min.Get(args).y, this.m_Max.Get(args).y), 
            UnityEngine.Random.Range(this.m_Min.Get(args).z, this.m_Max.Get(args).z))  ;
        
        this.m_Set.Set(value, args);

        return DefaultResult;
    }
}
