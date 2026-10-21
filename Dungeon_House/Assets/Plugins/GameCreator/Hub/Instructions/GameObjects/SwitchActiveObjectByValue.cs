using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;
using GameCreator.Runtime.VisualScripting;
using System.Collections.Generic;

[Version(0, 1, 5)]
[Title("Switch Activated Objects from List by Reference")]
[Category("Game Objects/Switch Active Object from List")]
[Keywords("Deactivate", "Activate", "List Objects")]
[Image(typeof(IconCubeSolid), ColorTheme.Type.Yellow)]
[Description("Deactivate all listed objects (optional) and then activate a specific object from the list that corresponds to a Name reference")]

[Serializable]
public class SwitchActiveObjectByValue : Instruction
{
	[Tooltip("If no reference then Element 0 stays active")]
	[SerializeField] private PropertyGetString referenceActivation = GetStringString.Create;

	[Tooltip("Set Names for Objects that are at the same index in Object List")]
	[SerializeField] private List<string> referenceNameOfObject;

	[SerializeField] private GameObject[] objectsList;

	[Tooltip("Toggle to deactivate all objects before activating the desired one")]
	[SerializeField] private bool deactivateBeforeActivating = false;

	private string objActivated;
	private string m_referenceActivation;
	private int indexInList;

	public override string Title => $"Activate from Object List = {this.referenceActivation}";

	protected override Task Run(Args args)
	{
		m_referenceActivation = this.referenceActivation.Get(args);
		indexInList = referenceNameOfObject.IndexOf(m_referenceActivation);

		if (string.IsNullOrEmpty(m_referenceActivation) || indexInList < 0)
		{
			ActivateDefault();
		}
		else
		{
			ObjActivateIndex();
		}

		return DefaultResult;
	}

	private void ActivateDefault()
	{
		if (deactivateBeforeActivating)
		{
			foreach (var obj in objectsList)
			{
				obj.SetActive(false);
			}
		}

		if (objectsList.Length > 0)
		{
			objectsList[0].SetActive(true);
			objActivated = referenceNameOfObject[0];
		}
	}

	private void ObjActivateIndex()
	{
		if (deactivateBeforeActivating)
		{
			foreach (var obj in objectsList)
			{
				obj.SetActive(false);
			}
		}

		if (indexInList < objectsList.Length)
		{
			objectsList[indexInList].SetActive(true);
			objActivated = referenceNameOfObject[indexInList];
		}
	}
}
