using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameCreator.Runtime.VisualScripting
{
	[Version(0, 0, 1)]
    
	[Title("Don't Destroy")]
	[Description("Don't Destroy a game object on Load Scene.Action before Loading")]

	[Category("Game Objects/Don't Destroy")]

	[Keywords("Remove", "Delete", "Flush")]
	[Image(typeof(IconCubeOutline), ColorTheme.Type.Red, typeof(OverlayPlus))]
    
	[Serializable]
	public class InstructionDontDestroyGameObject : TInstructionGameObject
	{
		// PROPERTIES: ----------------------------------------------------------------------------
		
		public override string Title => $"Don't Destroy {this.m_GameObject}";

		// RUN METHOD: ----------------------------------------------------------------------------

		protected override Task Run(Args args)
		{
			GameObject gameObject = this.m_GameObject.Get(args);
			if (gameObject == null) return DefaultResult;
            
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			
			
			return DefaultResult;
		}
		
	}
	
}