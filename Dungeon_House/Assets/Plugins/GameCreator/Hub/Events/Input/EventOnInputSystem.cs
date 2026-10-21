using System;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace GameCreator.Runtime.VisualScripting
{
	[Version(1, 0, 4)]
    [Title("On Input System")]
    [Category("Input/On Input System")]
    [Description("Detects when a key of InputSystem perform a action")]

    [Image(typeof(IconButton), ColorTheme.Type.Blue)]
    [Keywords("Down", "Up", "Hold", "Input")]
    [Keywords("Keyboard", "Mouse", "Button", "Gamepad", "Controller", "Joystick")]

    [Serializable]
	public class EventOnInputSystem : Event
    {

	    [SerializeField] private InputActionFromAsset m_Input = new InputActionFromAsset();
	    private enum InputEvent
	    {
		    OnKeyDown, OnKeyHold, OnKeyUp
	    }
	    [SerializeField] private InputEvent typeInput;
	    
        protected override void OnStart(Trigger trigger)
        {
            base.OnStart(trigger);
	         m_Input.InputAction.Enable();
        }

        protected override void OnUpdate(Trigger trigger)
        {
            
	        if(m_Input.InputAction.WasReleasedThisFrame() && typeInput == InputEvent.OnKeyUp)
	        {
		        _ = this.m_Trigger.Execute(this.Self);
		     }
	        if(m_Input.InputAction.IsPressed() && typeInput == InputEvent.OnKeyHold)
	        {
		        _ = this.m_Trigger.Execute(this.Self);
		     }
	        if(m_Input.InputAction.WasPressedThisFrame() && typeInput == InputEvent.OnKeyDown)
	        {
		        _ = this.m_Trigger.Execute(this.Self);
		     }
	     }

    }
}