using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(1, 0, 0)]
[Title("On Collision Enter Layer")]
[Category("Physics/On Collision Enter Layer")]
[Description("Executed when a game object belongs to any of the layer mask values and enters the collider")]

[Image(typeof(IconLayers), ColorTheme.Type.Green)]

[Keywords("Crash", "Touch", "Bump", "Collision")]

[Serializable]

public class EventCollisionEnterLayer : GameCreator.Runtime.VisualScripting.Event
{
    //[SerializeField] private TagValue m_Tag = new TagValue();
    [SerializeField] private LayerMask m_LayerMask = Physics.DefaultRaycastLayers;
    private bool _canExec = false;
    private float timer = 0f;

    protected override void OnAwake(Trigger trigger)
    {
        base.OnAwake(trigger);
        trigger.RequireRigidbody();
    }

    protected override void OnFixedUpdate(Trigger trigger)
    {
        if (!_canExec)
        {
            if (timer <= 0.1f) timer += Time.fixedDeltaTime;

            if (timer >= 0.1f) _canExec = true;
        }
    }

    protected override void OnCollisionEnter3D(Trigger trigger, Collision collision)
    {
        if (!_canExec) return;
        base.OnCollisionEnter3D(trigger, collision);

        if (!this.IsActive) return;
        if (!CheckLayerMask(collision.gameObject)) return;

        GetGameObjectLastCollidedEnter.Instance = collision.gameObject;
        _ = this.m_Trigger.Execute(collision.gameObject);
    }

    protected override void OnCollisionEnter2D(Trigger trigger, Collision2D collision)
    {
        if (!_canExec) return;
        base.OnCollisionEnter2D(trigger, collision);

        if (!this.IsActive) return;
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
