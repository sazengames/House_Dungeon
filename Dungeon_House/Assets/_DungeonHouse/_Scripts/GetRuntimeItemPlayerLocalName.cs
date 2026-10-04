using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace GameCreator.Runtime.Inventory
{
	[Title("Player Local Name Variable")]
	[Category("Variables/Player Local Name Variable")]

	[Image(typeof(IconProperty), ColorTheme.Type.Teal)]
	[Description("Runtime Item stored in a Local Name Variable on the Player")]

	[Parameter("Variable Name", "Name of the Local Name Variable on the Player")]

	[Serializable] [HideLabelsInEditor]
	public class GetRuntimeItemPlayerLocalName : PropertyTypeGetRuntimeItem
	{
		[SerializeField] protected IdString m_VariableName;

		public override RuntimeItem Get(Args args)
		{
			if (ShortcutPlayer.Instance == null) return null;

			LocalNameVariables variables = ShortcutPlayer.Instance
				.GetComponent<LocalNameVariables>();

			if (variables == null) return null;

			string id = this.m_VariableName.String;
			if (!variables.Exists(id)) return null;

			return variables.Get(id) as RuntimeItem;
		}

		public static PropertyGetRuntimeItem Create => new PropertyGetRuntimeItem(
		new GetRuntimeItemPlayerLocalName()
		);

		public override string String => $"Player[{this.m_VariableName.String}]";
	}
}