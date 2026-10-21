using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Shooter
{
    [Serializable]
    public class Magazine
    {
        [SerializeField] private Ammo m_Ammo;

        [SerializeField] private PropertyGetBool m_HasMagazine = GetBoolTrue.Create;
        [SerializeField] private PropertyGetInteger m_MagazineSize = GetDecimalInteger.Create(10);
        [SerializeField] private PropertyGetBool m_AutoReload = GetBoolTrue.Create;
        
        // GETTER METHODS: ------------------------------------------------------------------------

        public int GetTotalAmmo(Args args)
        {
            return this.m_Ammo != null ? this.m_Ammo.GetTotalAmmo(args) : 0;
        }
        
        public bool GetHasMagazine(Args args)
        {
            return this.m_HasMagazine.Get(args);
        }
        
        public int GetMagazineSize(Args args)
        {
            return (int) this.m_MagazineSize.Get(args);
        }
        
        public bool AutoReload(Args args) => this.m_AutoReload.Get(args);

        public int Unload(ShooterMunition munition, Args args)
        {
            if (munition == null || this.m_Ammo == null) return 0;

            int amount = munition.InMagazine;
            if (amount <= 0) return 0;

            munition.InMagazine = 0;
            this.m_Ammo.Return(amount, args);
            return amount;
        }
        public void ReturnRounds(int amount, Args args)
        {
            if (this.m_Ammo == null || amount <= 0) return;
            this.m_Ammo.Return(amount, args);
        }
        // INTERNAL METHODS: ----------------------------------------------------------------------

        internal bool EnoughAmmo(ShotData shotData, Args args)
        {
            if (this.m_Ammo == null) return false;

            Character character = args.Self.Get<Character>();
            if (character == null) return false;

            if (this.m_HasMagazine.Get(args))
            {
                ShooterMunition munition = character.Combat.RequestMunition(shotData.Weapon) as ShooterMunition;
                return shotData.Cartridges <= (munition?.InMagazine ?? 0);
            }

            return this.m_Ammo.IsInfinite || shotData.Cartridges <= this.m_Ammo.GetTotalAmmo(args);
        }

        internal void OnLoad(int loaded, Args args)
        {
            if (this.m_Ammo == null) return;
            if (!this.m_HasMagazine.Get(args)) return;
            this.m_Ammo.Remove(loaded, args);
        }

        internal void DiscardMagazine(int amount, Args args)
        {
            //if (this.m_HasMagazine.Get(args) == false) return;
            
            //if (this.m_Ammo == null) return;
            //this.m_Ammo.Remove(amount, args);
        }

        internal void OnShoot(int cartridgesShot, Args args)
        {
            if (this.m_Ammo == null) return;
            if (this.m_HasMagazine.Get(args)) return; // already removed when loaded
            this.m_Ammo.Remove(cartridgesShot, args);
        }

        // GIZMOS: --------------------------------------------------------------------------------

        public void StageGizmos(StagingGizmos stagingGizmos)
        { }
    }
}