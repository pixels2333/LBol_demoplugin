using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Sakuya;

namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	// Token: 0x02000018 RID: 24
	[UsedImplicitly]
	public sealed class KnifeDamageUpSe : StatusEffect
	{
		// Token: 0x06000034 RID: 52 RVA: 0x000025B0 File Offset: 0x000007B0
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageDealingEventArgs>(unit.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
		}

		// Token: 0x06000035 RID: 53 RVA: 0x000025CC File Offset: 0x000007CC
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (args.DamageInfo.DamageType == DamageType.Attack && args.ActionSource is Knife)
			{
				args.DamageInfo = args.DamageInfo.IncreaseBy(base.Level);
				args.AddModifier(this);
			}
		}
	}
}
