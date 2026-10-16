using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

public class GunMagazineSave : MonoBehaviour, IGameSave
{
    private void OnEnable()
    {
        _ = SaveLoadManager.Subscribe(this);
    }

    private void OnDisable()
    {
        SaveLoadManager.Unsubscribe(this);
    }

    // IGameSave: --------------------------------------------------------------

    public string SaveID => "gun-magazines";
    public bool IsShared => false;

    public Type SaveType => typeof(GunMagazineSaveData);

    public object GetSaveData(bool includeNonSavable)
    {
        return GunMagazineStore.Export();
    }

    public LoadMode LoadMode => LoadMode.Greedy;

    public Task OnLoad(object value)
    {
        GunMagazineStore.Import(value as GunMagazineSaveData);
        return Task.CompletedTask;
    }
}