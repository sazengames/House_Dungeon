using System.Collections.Generic;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.UnityUI;
using GameCreator.Runtime.Inventory;
using UnityEngine;

public class UseButtonLabel : MonoBehaviour
{
    [SerializeField] private TextReference m_Label = new TextReference();
    [SerializeField] private string m_DefaultText = "Use";
    [SerializeField] private string m_GunText = "Unload Ammo";
    [SerializeField] private List<Item> m_Guns = new List<Item>();

    private void LateUpdate()
    {
        RuntimeItem selected = RuntimeItem.UI_LastItemSelected;
        bool isGun = selected != null && this.m_Guns.Contains(selected.Item);

        this.m_Label.Text = isGun ? this.m_GunText : this.m_DefaultText;
    }
}
