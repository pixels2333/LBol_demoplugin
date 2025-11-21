using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.GapOptions;
using LBoL.Core.Stations;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000156 RID: 342
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3)]
	public sealed class Baota : Exhibit
	{
		// Token: 0x060004AC RID: 1196 RVA: 0x0000C208 File Offset: 0x0000A408
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, delegate(UnitEventArgs _)
			{
				base.Blackout = true;
			});
		}

		// Token: 0x060004AD RID: 1197 RVA: 0x0000C254 File Offset: 0x0000A454
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}

		// Token: 0x060004AE RID: 1198 RVA: 0x0000C25D File Offset: 0x0000A45D
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (base.Counter > 0)
			{
				base.NotifyActivating();
				ManaGroup manaGroup = base.Mana * base.Counter;
				yield return new GainManaAction(manaGroup);
				yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Counter), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x0000C26D File Offset: 0x0000A46D
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.GapOptionsGenerating, delegate(StationEventArgs args)
			{
				if (base.Counter < base.Value1)
				{
					((GapStation)args.Station).GapOptions.Add(Library.CreateGapOption<UpgradeBaota>());
				}
			});
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x0000C28C File Offset: 0x0000A48C
		public void GapOption()
		{
			int num = base.Counter + 1;
			base.Counter = num;
			base.NotifyActivating();
		}
	}
}
