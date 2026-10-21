using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

[Version(1, 0, 0)]
[Title("Weighted Random Event")]
[Description("Runs one instruction list from a list, each with a different chance (weight)")]
[Category("Events/Weighted Random Event")]

[Parameter("Events", "List of instruction lists to choose from")]
[Parameter("Weights", "List of weights (chances) for each event")]

[Image(typeof(IconDice), ColorTheme.Type.Green)]
[Serializable]
public class InstructionWeightedRandomEvent : Instruction
{
    [Serializable]
    public class WeightedInstructionList
    {
        public string eventName;
        public InstructionList instructions = new InstructionList();
        [Min(0)] public int weight = 1;
    }

    [Tooltip("List of instruction lists and their weights")]
    public List<WeightedInstructionList> events = new List<WeightedInstructionList>();

    public override string Title => "Run one weighted random event";

    protected override async Task Run(Args args)
    {
        if (events == null || events.Count == 0)
        {
            Debug.LogWarning("No events set in WeightedRandomEvent instruction.");
            return;
        }

        // Calculate total weight
        int totalWeight = 0;
        foreach (var evt in events)
        {
            totalWeight += Mathf.Max(0, evt.weight);
        }

        if (totalWeight == 0)
        {
            Debug.LogWarning("All weights are zero in WeightedRandomEvent instruction.");
            return;
        }

        // Pick a random value
        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var evt in events)
        {
            cumulative += Mathf.Max(0, evt.weight);
            if (randomValue < cumulative)
            {
                if (evt.instructions != null)
                {
                    await evt.instructions.Run(args);
                }
                break;
            }
        }
    }
}