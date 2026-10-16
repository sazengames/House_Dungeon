using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Shooter;
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class GunMagazineSaveData
{
    public List<string> ids = new List<string>();
    public List<int> rounds = new List<int>();
}
public static class GunMagazineStore
{
    private static readonly Dictionary<string, int> Saved = new Dictionary<string, int>();
    private static readonly Dictionary<Character, string> Equipped = new Dictionary<Character, string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        Saved.Clear();
        Equipped.Clear();
    }

    public static void Save(RuntimeItem item, int rounds) => Saved[item.RuntimeID.String] = rounds;

    public static int Get(RuntimeItem item) =>
        Saved.TryGetValue(item.RuntimeID.String, out int rounds) ? rounds : 0;

    public static int Take(RuntimeItem item)
    {
        int rounds = Get(item);
        Saved[item.RuntimeID.String] = 0;
        return rounds;
    }

    public static void SetEquipped(Character character, RuntimeItem item)
    {
        if (character == null) return;
        Equipped[character] = item?.RuntimeID.String;
    }

    public static bool IsEquipped(Character character, RuntimeItem item) =>
        character != null
        && item != null
        && Equipped.TryGetValue(character, out string id)
        && id == item.RuntimeID.String;

    public static GunMagazineSaveData Export()
    {
        Dictionary<string, int> snapshot = new Dictionary<string, int>(Saved);

        // the equipped gun's rounds live in the weapon's munition, not in Saved
        foreach (KeyValuePair<Character, string> entry in Equipped)
        {
            if (entry.Key == null || string.IsNullOrEmpty(entry.Value)) continue;

            ShooterWeapon weapon = entry.Key.Combat.GetActiveWeapon<ShooterWeapon>();
            if (weapon == null) continue;

            if (entry.Key.Combat.RequestMunition(weapon) is ShooterMunition munition)
            {
                snapshot[entry.Value] = munition.InMagazine;
            }
        }

        GunMagazineSaveData data = new GunMagazineSaveData();
        foreach (KeyValuePair<string, int> entry in snapshot)
        {
            data.ids.Add(entry.Key);
            data.rounds.Add(entry.Value);
        }

        return data;
    }

    public static void Import(GunMagazineSaveData data)
    {
        Saved.Clear();
        if (data == null) return;

        int count = Math.Min(data.ids.Count, data.rounds.Count);
        for (int i = 0; i < count; i++) Saved[data.ids[i]] = data.rounds[i];

        // a gun that is already equipped gets its loaded rounds applied right away
        foreach (KeyValuePair<Character, string> entry in Equipped)
        {
            if (entry.Key == null || string.IsNullOrEmpty(entry.Value)) continue;
            if (!Saved.TryGetValue(entry.Value, out int rounds)) continue;

            ShooterWeapon weapon = entry.Key.Combat.GetActiveWeapon<ShooterWeapon>();
            if (weapon == null) continue;

            if (entry.Key.Combat.RequestMunition(weapon) is ShooterMunition munition)
            {
                munition.InMagazine = rounds;
            }
        }
    }

}