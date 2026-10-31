using System;
using System.Threading.Tasks;
using EmeraldAI;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace EmeraldGC2
{
    // MOVEMENT: ----------------------------------------------------------------------------------

    [Title("Set Emerald AI Wander Type")]
    [Description("Changes how an Emerald AI moves when it isn't fighting")]
    [Category("Emerald AI/Movement/Set Emerald AI Wander Type")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Wander Type", "Dynamic, Waypoints, Stationary, Destination or Custom")]
    [Keywords("Wander", "Patrol", "Idle", "Roam", "Stationary", "Waypoints", "Emerald")]
    [Image(typeof(IconFootprint), ColorTheme.Type.Blue)]
    [Serializable]
    public class InstructionEmeraldSetWanderType : TInstructionEmerald
    {
        [SerializeField] private EmeraldMovement.WanderTypes m_WanderType = EmeraldMovement.WanderTypes.Dynamic;

        public override string Title => $"{this.m_AI} wanders {this.m_WanderType}";

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetLiveAI(args);
            if (ai != null) EmeraldAPI.Movement.ChangeWanderType(ai, this.m_WanderType);
            return DefaultResult;
        }
    }

    [Title("Send Emerald AI To Position")]
    [Description("Sends an Emerald AI to a position. Optionally waits until it arrives")]
    [Category("Emerald AI/Movement/Send Emerald AI To Position")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Position", "Where to go")]
    [Parameter("Mode", "Go There: a one-off trip. Custom Destination: stays there. " +
                       "Wander Around: wanders around the position from now on")]
    [Parameter("Wait To Arrive", "Hold the instruction list until the AI arrives (or Timeout runs out)")]
    [Parameter("Timeout", "Seconds to wait at most. 0 = no limit")]
    [Keywords("Move", "Go", "Walk", "Destination", "Navigate", "Emerald")]
    [Image(typeof(IconLocationDrop), ColorTheme.Type.Blue)]
    [Serializable]
    public class InstructionEmeraldSendToPosition : TInstructionEmerald
    {
        public enum Modes { GoThere, CustomDestination, WanderAround }

        [SerializeField] private PropertyGetPosition m_Position = GetPositionTarget.Create();
        [SerializeField] private Modes m_Mode = Modes.GoThere;
        [SerializeField] private bool m_WaitToArrive = false;
        [SerializeField] private float m_Timeout = 10f;

        public override string Title => $"Send {this.m_AI} to {this.m_Position}" + (this.m_WaitToArrive ? " and wait" : "");

        protected override async Task Run(Args args)
        {
            EmeraldSystem ai = this.GetLiveAI(args);
            if (ai == null) return;

            Vector3 position = this.m_Position.Get(args);

            switch (this.m_Mode)
            {
                case Modes.GoThere: EmeraldAPI.Movement.SetDestination(ai, position); break;
                case Modes.CustomDestination: EmeraldAPI.Movement.SetCustomDestination(ai, position); break;
                case Modes.WanderAround: EmeraldAPI.Movement.SetDynamicWanderPosition(ai, position); break;
            }

            if (!this.m_WaitToArrive) return;

            float start = UnityEngine.Time.time;
            await this.Until(() =>
            {
                if (ai == null || EmeraldBridge.IsDead(ai)) return true;
                if (this.m_Timeout > 0f && UnityEngine.Time.time - start >= this.m_Timeout) return true;

                UnityEngine.AI.NavMeshAgent agent = ai.m_NavMeshAgent;
                if (agent == null || !agent.isOnNavMesh) return true;
                if (agent.pathPending) return false;

                return agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.25f) + 0.1f;
            });
        }
    }

    [Title("Stop Emerald AI Movement")]
    [Description("Stops or resumes an Emerald AI's movement, e.g. for dialogue or cutscenes")]
    [Category("Emerald AI/Movement/Stop Emerald AI Movement")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Action", "Stop or Resume")]
    [Keywords("Stop", "Pause", "Freeze", "Resume", "Hold", "Dialogue", "Cutscene", "Emerald")]
    [Image(typeof(IconStop), ColorTheme.Type.Red)]
    [Serializable]
    public class InstructionEmeraldStopMovement : TInstructionEmerald
    {
        public enum Actions { Stop, Resume }

        [SerializeField] private Actions m_Action = Actions.Stop;

        public override string Title => $"{this.m_Action} movement of {this.m_AI}";

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetLiveAI(args);
            if (ai == null) return DefaultResult;

            if (this.m_Action == Actions.Stop) EmeraldAPI.Movement.StopMovement(ai);
            else EmeraldAPI.Movement.ResumeMovement(ai);

            return DefaultResult;
        }
    }

    // ANIMATION & SOUND: -------------------------------------------------------------------------

    [Title("Play Emerald AI Emote")]
    [Description("Plays one of the emote animations from the AI's Animation Profile")]
    [Category("Emerald AI/Animation/Play Emerald AI Emote")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Emote ID", "The emote's ID in the Animation Profile")]
    [Parameter("Mode", "Play once, start looping, or stop the loop")]
    [Keywords("Emote", "Taunt", "Gesture", "Animation", "Emerald")]
    [Image(typeof(IconCharacterGesture), ColorTheme.Type.Purple)]
    [Serializable]
    public class InstructionEmeraldPlayEmote : TInstructionEmerald
    {
        public enum Modes { PlayOnce, Loop, StopLoop }

        [SerializeField] private PropertyGetInteger m_EmoteID = new PropertyGetInteger(1);
        [SerializeField] private Modes m_Mode = Modes.PlayOnce;

        public override string Title => $"{this.m_AI} emote {this.m_EmoteID} ({this.m_Mode})";

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetLiveAI(args);
            if (ai == null) return DefaultResult;

            int id = (int) this.m_EmoteID.Get(args);
            switch (this.m_Mode)
            {
                case Modes.PlayOnce: EmeraldAPI.Animation.PlayEmoteAnimation(ai, id); break;
                case Modes.Loop: EmeraldAPI.Animation.LoopEmoteAnimation(ai, id); break;
                case Modes.StopLoop: EmeraldAPI.Animation.StopLoopEmoteAnimation(ai, id); break;
            }

            return DefaultResult;
        }
    }

    [Title("Play Emerald AI Sound")]
    [Description("Plays a sound through the AI's Emerald Sound Profile")]
    [Category("Emerald AI/Animation/Play Emerald AI Sound")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Sound", "Which sound from the Sound Profile to play")]
    [Parameter("Sound Effect ID", "Only for Sound Effect: the effect's ID in the Sound Profile")]
    [Keywords("Sound", "Growl", "Roar", "Audio", "Voice", "Emerald")]
    [Image(typeof(IconMusicNote), ColorTheme.Type.Yellow)]
    [Serializable]
    public class InstructionEmeraldPlaySound : TInstructionEmerald
    {
        public enum Sounds { Idle, Attack, Warning, Injured, Death, Block, RandomEffect, SoundEffect }

        [SerializeField] private Sounds m_Sound = Sounds.Warning;
        [SerializeField] private PropertyGetInteger m_SoundEffectID = new PropertyGetInteger(1);

        public override string Title => $"{this.m_AI} plays {this.m_Sound} sound";

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetAI(args);
            if (ai == null) return DefaultResult;

            switch (this.m_Sound)
            {
                case Sounds.Idle: EmeraldAPI.Sound.PlayIdleSound(ai); break;
                case Sounds.Attack: EmeraldAPI.Sound.PlayAttackSound(ai); break;
                case Sounds.Warning: EmeraldAPI.Sound.PlayWarningSound(ai); break;
                case Sounds.Injured: EmeraldAPI.Sound.PlayInjuredSound(ai); break;
                case Sounds.Death: EmeraldAPI.Sound.PlayDeathSound(ai); break;
                case Sounds.Block: EmeraldAPI.Sound.PlayBlockSound(ai); break;
                case Sounds.RandomEffect: EmeraldAPI.Sound.PlayRandomSoundEffect(ai); break;
                case Sounds.SoundEffect: EmeraldAPI.Sound.PlaySoundEffect(ai, (int) this.m_SoundEffectID.Get(args)); break;
            }

            return DefaultResult;
        }
    }
}
