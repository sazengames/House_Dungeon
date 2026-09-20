using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Title("Toogle Emmission Property")]
[Description("Toogle the emmission property of a material on or off")]

[Image(typeof(IconColor), ColorTheme.Type.Yellow)]

[Category("Renderer/Toogle Emmission Property")]

//[Parameter("Property", "Name of the property to change")]
//[Parameter("Color", "Color target that the instantiated Material turns into")]
//[Parameter("Duration", "How long it takes to perform the transition")]
//[Parameter("Easing", "The change rate of the transition over time")]
//[Parameter("Wait to Complete", "Whether to wait until the transition is finished or not")]

[Keywords("Set", "Shader", "Hue")]
[Serializable]
public class InstructionRendererToggleEmmision : TInstructionRenderer
{
    [SerializeField] private bool m_Enable = false;
    [SerializeField] private bool m_MultiMaterials = false;
    protected override Task Run(Args args)
    {
        GameObject gameObject = this.m_Renderer.Get(args);
        if (gameObject == null) return DefaultResult;

        Renderer renderer = gameObject.Get<Renderer>();
        if (renderer == null) return DefaultResult;

        bool value = this.m_Enable;
        if (m_MultiMaterials)
        {
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                if (value) renderer.materials[i].EnableKeyword("_EMISSION");
                else renderer.materials[i].DisableKeyword("_EMISSION");
            }
        }
        else
        {
            if (value) renderer.material.EnableKeyword("_EMISSION");
            else renderer.material.DisableKeyword("_EMISSION");
        }
        
        
        // Your code here...
        return DefaultResult;
    }
}
