using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[Version(1, 0, 2)]

[Title(" Mouse Cursor World Position")]
[Description("Get Mouse Cursor World Position and set to a Variable")]

[Category("Transforms/Mouse Cursor World Position")]

[Keywords( "Mouse", "Cursor", "Position")]
[Image(typeof(IconCursor), ColorTheme.Type.Yellow)]

[Serializable]
public class InstructionWorldMousePosition : Instruction
{
	// MEMBERS: -------------------------------------------------------------------------------
	[SerializeField] private LayerMask m_layerMask ; // don't set to Everything Layer
	[SerializeField] private PropertySetVector3 m_Set = new PropertySetVector3();
	
   RaycastHit m_hitData;
	Vector3 m_worldPosition;
	Vector2 m_pointerPosition;
	Ray m_ray;
	
	// PROPERTIES: ----------------------------------------------------------------------------

	public override string Title => string.Format("Set Mouse World Position to", this.m_Set );
	
	// RUN METHOD: ----------------------------------------------------------------------------
    protected override Task Run(Args args)
    {
	       m_pointerPosition = Mouse.current.position.ReadValue();
		    
          m_ray = Camera.main.ScreenPointToRay( m_pointerPosition);
            
		    if (Physics.Raycast(m_ray, out m_hitData, 1000, m_layerMask))
		    {
			    m_worldPosition = m_hitData.point;
			    this.m_Set.Set(m_worldPosition, args);
		    }
		   
		  
        return DefaultResult;
    }
}
