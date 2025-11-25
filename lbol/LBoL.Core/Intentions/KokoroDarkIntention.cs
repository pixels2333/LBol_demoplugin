using System;
using JetBrains.Annotations;
using LBoL.Core.Units;
namespace LBoL.Core.Intentions
{
	[UsedImplicitly]
	public sealed class KokoroDarkIntention : Intention
	{
		public override IntentionType Type
		{
			get
			{
				return IntentionType.KokoroDark;
			}
		}
		public DamageInfo Damage { get; internal set; }
		public int Count { get; internal set; }
		public string DamageText
		{
			get
			{
				return base.CalculateDamage(this.Damage).ToString();
			}
		}
	}
}
