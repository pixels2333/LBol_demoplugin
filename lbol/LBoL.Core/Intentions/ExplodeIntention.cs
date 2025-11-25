using System;
using JetBrains.Annotations;
using LBoL.Core.Units;
namespace LBoL.Core.Intentions
{
	[UsedImplicitly]
	public sealed class ExplodeIntention : Intention
	{
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Explode;
			}
		}
		public DamageInfo Damage { get; internal set; }
		public string DamageText
		{
			get
			{
				return base.CalculateDamage(this.Damage).ToString();
			}
		}
	}
}
