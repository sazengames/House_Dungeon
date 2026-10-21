using System;

namespace GameCreator.Runtime.Characters.IK
{
    [Serializable]
    public abstract class TRigAnimatorIK : TRig
    {
        public sealed override void OnUpdate(Character character)
        {
            this.DoUpdate(character);
        }
    }
}