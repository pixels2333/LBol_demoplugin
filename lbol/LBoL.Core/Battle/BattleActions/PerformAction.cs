using System;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public class PerformAction : SimpleAction
	{
		public PerformAction.PerformArgs Args { get; }
		private PerformAction(PerformAction.PerformArgs args)
		{
			this.Args = args;
		}
		public static PerformAction ViewCard(Card card)
		{
			return new PerformAction(new PerformAction.ViewCardArgs
			{
				Card = card,
				Zone = card.Zone
			});
		}
		public static PerformAction Gun(Unit source, Unit target, string gunId, float waitTime = 0f)
		{
			return new PerformAction(new PerformAction.GunArgs
			{
				Source = source,
				Target = target,
				GunId = gunId,
				WaitTime = waitTime
			});
		}
		public static PerformAction Doll(Doll doll, Unit target, string gunId, float waitTime, string debugString = "")
		{
			return new PerformAction(new PerformAction.DollArgs
			{
				Doll = doll,
				Target = target,
				GunId = gunId,
				WaitTime = waitTime,
				DebugString = debugString
			});
		}
		public static PerformAction Animation(Unit source, string animationName, float waitTime = 0f, string sfxId = null, float sfxDelay = 0f, int shakeLevel = -1)
		{
			return new PerformAction(new PerformAction.AnimationArgs
			{
				Source = source,
				AnimationName = animationName,
				WaitTime = waitTime,
				SfxId = sfxId,
				SfxDelay = sfxDelay,
				ShakeLevel = shakeLevel
			});
		}
		public static PerformAction Sfx(string id, float delay = 0f)
		{
			return new PerformAction(new PerformAction.SfxArgs
			{
				Id = id,
				Delay = delay
			});
		}
		public static PerformAction UiSound(string id)
		{
			return new PerformAction(new PerformAction.UiSoundArgs
			{
				Id = id
			});
		}
		public static PerformAction Chat(Unit source, string content, float chatTime, float delay = 0f, float waitTime = 0f, bool talk = true)
		{
			return new PerformAction(new PerformAction.ChatArgs
			{
				Source = source,
				Content = content,
				ChatTime = chatTime,
				Delay = delay,
				WaitTime = waitTime,
				Talk = talk
			});
		}
		public static PerformAction Spell(Unit source, string spellName)
		{
			return new PerformAction(new PerformAction.SpellArgs
			{
				Source = source,
				SpellName = spellName
			});
		}
		public static PerformAction Effect(Unit source, string effectName, float delay = 0f, string sfxId = null, float sfxDelay = 0f, PerformAction.EffectBehavior effectType = PerformAction.EffectBehavior.PlayOneShot, float waitTime = 0f)
		{
			return new PerformAction(new PerformAction.EffectArgs
			{
				Source = source,
				EffectName = effectName,
				Delay = delay,
				SfxId = sfxId,
				SfxDelay = sfxDelay,
				EffectType = effectType,
				WaitTime = waitTime
			});
		}
		public static PerformAction EffectMessage(Unit source, string effectName, string message, object args)
		{
			return new PerformAction(new PerformAction.EffectMessageArgs
			{
				Source = source,
				EffectName = effectName,
				Message = message,
				Args = args
			});
		}
		public static PerformAction SePop(Unit source, string popContent)
		{
			return new PerformAction(new PerformAction.SePopArgs
			{
				Source = source,
				PopContent = popContent
			});
		}
		public static PerformAction SummonFriend(Card card)
		{
			return new PerformAction(new PerformAction.SummonFriendArgs
			{
				Card = card
			});
		}
		public static PerformAction TransformModel(Unit source, string modelName)
		{
			return new PerformAction(new PerformAction.TransformModelArgs
			{
				Source = source,
				ModelName = modelName
			});
		}
		public static PerformAction DeathAnimation(Unit source)
		{
			return new PerformAction(new PerformAction.DeathAnimationArgs
			{
				Source = source
			});
		}
		public static PerformAction Wait(float time, bool unscale = false)
		{
			return new PerformAction(new PerformAction.WaitArgs
			{
				Time = time,
				Unscale = unscale
			});
		}
		public abstract class PerformArgs
		{
		}
		public sealed class ViewCardArgs : PerformAction.PerformArgs
		{
			public Card Card { get; set; }
			public CardZone Zone { get; set; }
		}
		public sealed class GunArgs : PerformAction.PerformArgs
		{
			public Unit Source { get; set; }
			public Unit Target { get; set; }
			public string GunId { get; set; }
			public float WaitTime { get; set; }
		}
		public sealed class DollArgs : PerformAction.PerformArgs
		{
			public Doll Doll { get; set; }
			public Unit Target { get; set; }
			public string GunId { get; set; }
			public float WaitTime { get; set; }
			public string DebugString { get; set; }
		}
		public sealed class AnimationArgs : PerformAction.PerformArgs
		{
			public Unit Source { get; set; }
			public string AnimationName { get; set; }
			public float WaitTime { get; set; }
			public string SfxId { get; set; }
			public float SfxDelay { get; set; }
			public int ShakeLevel { get; set; }
		}
		public sealed class SfxArgs : PerformAction.PerformArgs
		{
			public string Id { get; set; }
			public float Delay { get; set; }
		}
		public sealed class UiSoundArgs : PerformAction.PerformArgs
		{
			public string Id { get; set; }
		}
		public sealed class ChatArgs : PerformAction.PerformArgs
		{
			public Unit Source { get; set; }
			public string Content { get; set; }
			public float ChatTime { get; set; }
			public float Delay { get; set; }
			public float WaitTime { get; set; }
			public bool Talk { get; set; }
		}
		public sealed class SpellArgs : PerformAction.PerformArgs
		{
			public Unit Source { get; set; }
			public string SpellName { get; set; }
		}
		public sealed class EffectArgs : PerformAction.PerformArgs
		{
			public Unit Source { get; set; }
			public string EffectName { get; set; }
			public float Delay { get; set; }
			public float WaitTime { get; set; }
			public string SfxId { get; set; }
			public float SfxDelay { get; set; }
			public PerformAction.EffectBehavior EffectType { get; set; }
		}
		public class EffectMessageArgs : PerformAction.PerformArgs
		{
			public Unit Source { get; set; }
			public string EffectName { get; set; }
			public string Message { get; set; }
			public object Args { get; set; }
		}
		public sealed class SePopArgs : PerformAction.PerformArgs
		{
			public Unit Source { get; set; }
			public string PopContent { get; set; }
		}
		public sealed class SummonFriendArgs : PerformAction.PerformArgs
		{
			public Card Card { get; set; }
		}
		public sealed class TransformModelArgs : PerformAction.PerformArgs
		{
			public Unit Source { get; set; }
			public string ModelName { get; set; }
		}
		public sealed class DeathAnimationArgs : PerformAction.PerformArgs
		{
			public Unit Source { get; set; }
		}
		public sealed class WaitArgs : PerformAction.PerformArgs
		{
			public float Time { get; set; }
			public bool Unscale { get; set; }
		}
		public enum EffectBehavior
		{
			PlayOneShot,
			Add,
			Remove,
			DieOut
		}
	}
}
