using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace GameCreator.Runtime.Variables
{
	[Title("Player Local Name Variable")]
	[Category("Variables/Player Local Name Variable")]

	[Image(typeof(IconPlayer), ColorTheme.Type.Teal)]
	[Description("Boolean stored in a Local Name Variable on the Player")]

	[Parameter("Variable Name", "Name of the Local Name Variable on the Player")]

	[Keywords("Bool", "Boolean", "Player", "Flag")]

	[Serializable] [HideLabelsInEditor]
	public class GetBoolPlayerLocalName : PropertyTypeGetBool
	{
		[SerializeField] protected IdString m_VariableName;

		public override bool Get(Args args) => this.GetValue();
		public override bool Get(GameObject gameObject) => this.GetValue();

		private bool GetValue()
		{
			if (ShortcutPlayer.Instance == null) return false;

			LocalNameVariables variables = ShortcutPlayer.Instance
				.GetComponent<LocalNameVariables>();

			if (variables == null) return false;

			string id = this.m_VariableName.String;
			if (!variables.Exists(id)) return false;

			object value = variables.Get(id);
			return value is bool flag && flag;
		}

		public static PropertyGetBool Create => new PropertyGetBool(
		new GetBoolPlayerLocalName()
		);

		public override string String => $"Player[{this.m_VariableName.String}]";
	}
}