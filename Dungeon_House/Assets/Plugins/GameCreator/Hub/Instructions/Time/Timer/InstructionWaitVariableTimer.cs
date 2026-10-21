using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Serializable]
[AddComponentMenu("Game Creator/Instructions/Wait Variable Timer")]
[Version(0, 1, 0)]
[Title("Wait Variable Timer")]
[Description("Waits for a duration defined by a variable and updates another variable each tick. Supports increment, decrement, or setting the remaining time. Executes an action when the timer completes.")]
[Category("Time/Timer/Wait Variable Timer")]
[Parameter("WaitTime", "The variable that defines the wait duration (in seconds)")]
[Parameter("OnCompleteAction", "Action to perform when the timer completes")]
[Parameter("OnCompleteSet", "Variable to update each tick")]
[Parameter("OnCompleteInstruction", "Instruction to execute when the timer completes")]
[Parameter("TickUpdateMode", "How to update the variable each tick (Increase, Decrease, SetRemaining)")]
[Keywords("Time", "Timer", "Variable", "Wait", "Countdown", "Tick", "Action")]
public class InstructionWaitVariableTimer : Instruction
{
    public enum OnCompleteAction
    {
        None,
        SetVariable,
        IncrementVariable,
        RunInstruction
    }

    public enum TickUpdateMode
    {
        Increase,
        Decrease,
        SetRemaining
    }

    [SerializeField, Tooltip("Bekleme s�resi (saniye) - GC2 de�i�ken se�ici ile tam uyumlu")] private PropertyGetDecimal m_WaitTime = GetDecimalGlobalName.Create;
    [SerializeField, Tooltip("Bekleme tamamland���nda yap�lacak i�lemi se�in")] private OnCompleteAction m_OnCompleteAction = OnCompleteAction.None;
    [SerializeField, Tooltip("Tick s�ras�nda g�ncellenecek de�i�ken")] private PropertySetNumber m_OnCompleteSet = SetNumberNone.Create;
    [SerializeField, Tooltip("Bekleme tamamland���nda �al��acak Instruction")] private Instruction m_OnCompleteInstruction;
    [SerializeField, Tooltip("Tick s�ras�nda de�i�ken g�ncelleme modu")] private TickUpdateMode m_TickUpdateMode = TickUpdateMode.Increase;

#if UNITY_EDITOR
    // Inspector'da sadece ilgili alanlar� g�ster
    public bool ShouldShowOnCompleteSet() => m_OnCompleteAction == OnCompleteAction.SetVariable || m_OnCompleteAction == OnCompleteAction.IncrementVariable;
    public bool ShouldShowOnCompleteInstruction() => m_OnCompleteAction == OnCompleteAction.RunInstruction;
#endif

    private async Task RunInstruction(Instruction instruction, Args args)
    {
        if (instruction == null) return;
        var method = instruction.GetType().GetMethod("Run", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (method != null)
        {
            var result = method.Invoke(instruction, new object[] { args });
            if (result is Task task) await task;
        }
    }

    protected override async Task Run(Args args)
    {
        double elapsed = 0;
        double tick = 1.0;
        try
        {
            while (true)
            {
                double waitSeconds = (double)m_WaitTime.Get(args); // HER TICK'TE G�NCELLE
                double remaining = waitSeconds - elapsed;
                if (remaining <= 0) break;
                if ((m_OnCompleteAction == OnCompleteAction.SetVariable || m_OnCompleteAction == OnCompleteAction.IncrementVariable) && m_OnCompleteSet != null)
                {
                    double current = 0;
                    bool valid = true;
                    try { current = m_OnCompleteSet.Get(args); } catch { valid = false; }
                    if (valid)
                    {
                        switch (m_TickUpdateMode)
                        {
                            case TickUpdateMode.Increase:
                                m_OnCompleteSet.Set(current + 1, args);
                                break;
                            case TickUpdateMode.Decrease:
                                m_OnCompleteSet.Set(current - 1, args);
                                break;
                            case TickUpdateMode.SetRemaining:
                                m_OnCompleteSet.Set(remaining, args);
                                break;
                        }
                    }
                }
                double delay = Math.Min(tick, remaining);
                await Task.Delay(TimeSpan.FromSeconds(delay));
                elapsed += delay;
            }
            // S�re bitince se�ilen aksiyonu uygula
            switch (m_OnCompleteAction)
            {
                case OnCompleteAction.SetVariable:
                    try { m_OnCompleteSet.Set(0, args); } catch { }
                    break;
                case OnCompleteAction.IncrementVariable:
                    try
                    {
                        double current = m_OnCompleteSet.Get(args);
                        m_OnCompleteSet.Set(current + 1, args);
                    }
                    catch { }
                    break;
                case OnCompleteAction.RunInstruction:
                    if (m_OnCompleteInstruction != null)
                        await RunInstruction(m_OnCompleteInstruction, args);
                    break;
                case OnCompleteAction.None:
                default:
                    break;
            }
        }
        catch { /* Play mode ��k��� veya variable asset unload durumunda sessizce yut */ }
    }
}
