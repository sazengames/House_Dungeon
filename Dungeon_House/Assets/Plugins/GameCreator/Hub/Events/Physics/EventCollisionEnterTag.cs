using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(1, 0, 0)]
[Title("On Collision Enter Tag")]
[Category("Physics/On Collision Enter Tag")]
[Description("Executed when the Trigger collides with a game object with specific tag")]

[Image(typeof(IconCollision), ColorTheme.Type.Blue)]

[Keywords("Crash", "Touch", "Bump", "Collision")]

[Serializable]
public class EventCollisionEnterTag : GameCreator.Runtime.VisualScripting.Event
{
    [SerializeField] private TagValue m_Tag = new TagValue();

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
        if (!collision.gameObject.CompareTag(this.m_Tag.Value)) return;

        GetGameObjectLastCollidedEnter.Instance = collision.gameObject;
        _ = this.m_Trigger.Execute(collision.gameObject);
    }

    protected override void OnCollisionEnter2D(Trigger trigger, Collision2D collision)
    {
        if (!_canExec) return;
        base.OnCollisionEnter2D(trigger, collision);

        if (!this.IsActive) return;
        if (!collision.gameObject.CompareTag(this.m_Tag.Value)) return;

        GetGameObjectLastCollidedEnter.Instance = collision.gameObject;
        _ = this.m_Trigger.Execute(collision.gameObject);
    }
}
