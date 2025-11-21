using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Seija;

namespace LBoL.EntityLib.StatusEffects.Enemy.Seija
{
	// Token: 0x020000D3 RID: 211
	[UsedImplicitly]
	public sealed class ShendengSe : SeijaSe
	{
		// Token: 0x1700004E RID: 78
		// (get) Token: 0x060002E5 RID: 741 RVA: 0x00007DA3 File Offset: 0x00005FA3
		protected override Type ExhibitType
		{
			get
			{
				return typeof(Shendeng);
			}
		}

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x060002E6 RID: 742 RVA: 0x00007DAF File Offset: 0x00005FAF
		// (set) Token: 0x060002E7 RID: 743 RVA: 0x00007DB7 File Offset: 0x00005FB7
		private int Counter { get; set; }

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x060002E8 RID: 744 RVA: 0x00007DC0 File Offset: 0x00005FC0
		// (set) Token: 0x060002E9 RID: 745 RVA: 0x00007DC8 File Offset: 0x00005FC8
		private List<Type> Types { get; set; }

		// Token: 0x060002EA RID: 746 RVA: 0x00007DD4 File Offset: 0x00005FD4
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			this.Counter = 0;
			List<Type> list = new List<Type>();
			list.Add(typeof(Weak));
			list.Add(typeof(Fragil));
			list.Add(typeof(Vulnerable));
			this.Types = list;
			base.ReactOwnerEvent<GameEventArgs>(base.Battle.AllEnemyTurnEnded, new EventSequencedReactor<GameEventArgs>(this.OnAllEnemyTurnEnded));
			this.React(PerformAction.Sfx("GuirenItem", 0f));
			this.React(PerformAction.EffectMessage(unit, "SeijaExhibitManager", "AddExhibit", this));
		}

		// Token: 0x060002EB RID: 747 RVA: 0x00007E7D File Offset: 0x0000607D
		private IEnumerable<BattleAction> OnAllEnemyTurnEnded(GameEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (this.Counter % 3 == 0)
			{
				this.Types.Shuffle(base.GameRun.EnemyBattleRng);
			}
			base.NotifyActivating();
			Type type = this.Types[this.Counter % 3];
			Unit player = base.Battle.Player;
			int? num = new int?(base.Level);
			yield return new ApplyStatusEffectAction(type, player, default(int?), num, default(int?), default(int?), 0f, false);
			int num2 = this.Counter + 1;
			this.Counter = num2;
			yield break;
		}
	}
}
