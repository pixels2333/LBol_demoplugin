using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.MultiColor
{
	// Token: 0x02000057 RID: 87
	[UsedImplicitly]
	public sealed class PrismriverSe : StatusEffect
	{
		// Token: 0x06000128 RID: 296 RVA: 0x00004321 File Offset: 0x00002521
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<ManaEventArgs>(base.Battle.ManaGaining, new GameEventHandler<ManaEventArgs>(this.OnManaGaining));
		}

		// Token: 0x06000129 RID: 297 RVA: 0x00004340 File Offset: 0x00002540
		private void OnManaGaining(ManaEventArgs args)
		{
			if (args.Value.WithPhilosophy(0) != ManaGroup.Empty)
			{
				base.NotifyActivating();
				args.Value = ManaGroup.Philosophies(args.Value.Amount);
				args.AddModifier(this);
			}
		}
	}
}
