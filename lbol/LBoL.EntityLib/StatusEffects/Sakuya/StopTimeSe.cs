using System;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Shining;

namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	// Token: 0x02000022 RID: 34
	public sealed class StopTimeSe : StatusEffect
	{
		// Token: 0x06000051 RID: 81 RVA: 0x00002818 File Offset: 0x00000A18
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<ManaEventArgs>(base.Battle.ManaLosing, new GameEventHandler<ManaEventArgs>(this.OnManaLosing));
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00002838 File Offset: 0x00000A38
		private void OnManaLosing(ManaEventArgs args)
		{
			if (args.Cause == ActionCause.TurnEnd)
			{
				ManaGroup value = args.Value;
				if (value.Philosophy > 0 && base.Battle.Player.HasExhibit<XianzheShi>())
				{
					value.Philosophy = 0;
				}
				ManaColor[] array = value.EnumerateComponents().SampleManyOrAll(base.Level, base.GameRun.BattleRng);
				if (!array.Empty<ManaColor>())
				{
					base.NotifyActivating();
					args.Value -= ManaGroup.FromComponents(array);
					args.AddModifier(this);
				}
			}
		}
	}
}
