using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(1, 0, 0)]
[Title("On Collide Layer")]
[Category("AAAScripts/Physics/On Collide Layer")]
[Description("Executed when the Trigger collides with a game object in a specific layer")]

[Image(typeof(IconLayers), ColorTheme.Type.Green)]

[Keywords("Crash", "Touch", "Bump", "Collision", "Layer")]

[Serializable]
public class EventCollideWithLayer : TEventPhysics
{
    [SerializeField] private LayerMask m_LayerMask = Physics.DefaultRaycastLayers;
    protected override void OnCollisionEnter3D(Trigger trigger, Collision collision)
    {
        base.OnCollisionEnter3D(trigger, collision);

        if (!this.IsActive) return;
        if (!this.Match(collision.gameObject)) return;
        if (!CheckLayerMask(collision.gameObject)) return;
        GetGameObjectLastCollidedEnter.Instance = collision.gameObject;
        _ = this.m_Trigger.Execute(collision.gameObject);
    }

    protected override void OnCollisionEnter2D(Trigger trigger, Collision2D collision)
    {
        base.OnCollisionEnter2D(trigger, collision);

        if (!this.IsActive) return;
        if (!this.Match(collision.gameObject)) return;
        if (!CheckLayerMask(collision.gameObject)) return;
        GetGameObjectLastCollidedEnter.Instance = collision.gameObject;
        _ = this.m_Trigger.Execute(collision.gameObject);
    }
    protected bool CheckLayerMask(GameObject gameObject)
    {
        if (gameObject == null) return false;

        int bitmask = this.m_LayerMask.value & (1 << gameObject.layer);
        return bitmask > 0;
    }
}

