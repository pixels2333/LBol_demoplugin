using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.MultiColor
{
	[UsedImplicitly]
	public sealed class PrismriverSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<ManaEventArgs>(base.Battle.ManaGaining, new GameEventHandler<ManaEventArgs>(this.OnManaGaining));
		}
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
