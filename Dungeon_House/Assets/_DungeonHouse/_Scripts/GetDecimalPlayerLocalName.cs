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
	[Description("Number stored in a Local Name Variable on the Player")]

	[Parameter("Variable Name", "Name of the Local Name Variable on the Player")]

	[Keywords("Float", "Decimal", "Double", "Player", "Durability")]

	[Serializable] [HideLabelsInEditor]
	public class GetDecimalPlayerLocalName : PropertyTypeGetDecimal
	{
		[SerializeField] protected IdString m_VariableName;

		public override double Get(Args args) => this.GetValue();
		public override double Get(GameObject gameObject) => this.GetValue();

		private double GetValue()
		{
			if (ShortcutPlayer.Instance == null) return 0;

			LocalNameVariables variables = ShortcutPlayer.Instance
				.GetComponent<LocalNameVariables>();

			if (variables == null) return 0;

			string id = this.m_VariableName.String;
			if (!variables.Exists(id)) return 0;

			object value = variables.Get(id);
			return value is IConvertible ? Convert.ToDouble(value) : 0;
		}

		public static PropertyGetDecimal Create => new PropertyGetDecimal(
		new GetDecimalPlayerLocalName()
		);

		public override string String => $"Player[{this.m_VariableName.String}]";
	}
}
