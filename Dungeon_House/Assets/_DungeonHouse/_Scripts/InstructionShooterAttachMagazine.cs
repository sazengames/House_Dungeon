using System;
using System.Reflection;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Shooter;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Title("Attach Gun Magazine")]
[Description("Spawns a magazine on the equipped gun and registers it with the Shooter reload, " +
             "so the next reload treats it as the Previous Magazine and replaces it")]
[Category("Shooter/Reload/Attach Gun Magazine")]

[Parameter("Character", "The character holding the gun")]
[Parameter("Magazine Prefab", "The magazine model to spawn (same prefab as the Reload's Magazine clip)")]
[Parameter("Attach To", "Object to parent the magazine to, e.g. the gun prop or its magazine socket")]
[Parameter("Local Position", "Position inside Attach To (copy from the Reload's Magazine clip)")]
[Parameter("Local Rotation", "Rotation inside Attach To (copy from the Reload's Magazine clip)")]
[Parameter("Only If Loaded", "Skip when the gun has 0 rounds, so an empty gun shows no magazine")]

[Keywords("Magazine", "Reload", "Clip", "Equip", "Preload", "Attach")]
[Image(typeof(IconPistol), ColorTheme.Type.Green)]
[Serializable]
public class InstructionShooterAttachMagazine : Instruction
{
    // The Shooter keeps the attached magazine in Reloading.CurrentMagazine. Its setter is
    // internal to the Shooter module, so it's set through reflection here.
    private static readonly PropertyInfo CURRENT_MAGAZINE = typeof(Reloading).GetProperty(
        "CurrentMagazine", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
    );

    // EXPOSED MEMBERS: -----------------------------------------------------------------------

    [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
    [SerializeField] private PropertyGetGameObject m_MagazinePrefab = new PropertyGetGameObject();
    [SerializeField] private PropertyGetGameObject m_AttachTo = new PropertyGetGameObject();

    [SerializeField] private Vector3 m_LocalPosition = Vector3.zero;
    [SerializeField] private Vector3 m_LocalRotation = Vector3.zero;

    [SerializeField] private bool m_OnlyIfLoaded = false;

    // PROPERTIES: ----------------------------------------------------------------------------

    public override string Title => $"Attach {this.m_MagazinePrefab} to {this.m_AttachTo}";

    // RUN METHOD: ----------------------------------------------------------------------------

    protected override Task Run(Args args)
    {
        Character character = this.m_Character.Get<Character>(args);
        if (character == null) return DefaultResult;

        ShooterStance stance = character.Combat.RequestStance<ShooterStance>();
        Reloading reloading = stance?.Reloading;
        if (reloading == null) return DefaultResult;

        if (this.m_OnlyIfLoaded)
        {
            ShooterWeapon weapon = character.Combat.GetActiveWeapon<ShooterWeapon>();
            if (weapon != null && character.Combat.RequestMunition(weapon) is ShooterMunition munition &&
                munition.InMagazine <= 0)
            {
                return DefaultResult;
            }
        }

        GameObject prefab = this.m_MagazinePrefab.Get(args);
        GameObject parent = this.m_AttachTo.Get(args);
        if (prefab == null || parent == null) return DefaultResult;

        // Remove a magazine left over from a previous equip, so they never stack up.
        GameObject old = reloading.CurrentMagazine;
        if (old != null) UnityEngine.Object.Destroy(old);

        GameObject magazine = UnityEngine.Object.Instantiate(prefab, parent.transform);
        magazine.transform.localPosition = this.m_LocalPosition;
        magazine.transform.localRotation = Quaternion.Euler(this.m_LocalRotation);
        magazine.SetActive(true);

        if (CURRENT_MAGAZINE != null && CURRENT_MAGAZINE.CanWrite)
        {
            CURRENT_MAGAZINE.SetValue(reloading, magazine);
        }
        else
        {
            Debug.LogWarning("[Attach Gun Magazine] Couldn't register the magazine with the " +
                             "Shooter reload. The next reload won't remove it.");
        }

        return DefaultResult;
    }
}
