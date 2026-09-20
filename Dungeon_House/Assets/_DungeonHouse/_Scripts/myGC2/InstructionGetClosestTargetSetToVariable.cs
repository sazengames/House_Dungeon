using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Serializable]
public class InstructionGetClosestTargetSetToVariable : Instruction
{
    [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
    [SerializeField]
    private PropertySetGameObject m_Set = SetGameObjectNone.Create;

   
    public override string Title => $"Get Closest Target from {this.m_Character} Set to a Variable";
    protected override Task Run(Args args)
    {
        Character character = this.m_Character.Get<Character>(args);
        if (character == null) return DefaultResult;

        
       
        Targets targets = character.Combat.Targets;
        List<GameObject> list = targets.List;

        float minDistance = 9999f;
        GameObject nextCandidate = null;

        foreach (GameObject candidate in list)
        {
            Vector3 position = candidate.transform.position;
            float distance = Vector3.Distance(character.transform.position, position);

            if (distance >= minDistance) continue;

            minDistance = distance;
            nextCandidate = candidate;
        }

        
        this.m_Set.Set(nextCandidate, args);
        return DefaultResult;
    }
}
