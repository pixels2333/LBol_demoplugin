using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.EnemyUnits.Normal;
using LBoL.EntityLib.EnemyUnits.Normal.Drones;
using LBoL.EntityLib.EnemyUnits.Normal.Maoyus;
using LBoL.EntityLib.EnemyUnits.Normal.Ravens;
using LBoL.EntityLib.EnemyUnits.Normal.Shenlings;
using LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus;
using LBoL.EntityLib.Exhibits.Seija;

namespace LBoL.EntityLib.StatusEffects.Enemy.Seija
{
	// Token: 0x020000D4 RID: 212
	public sealed class SihunYuSe : SeijaSe
	{
		// Token: 0x17000051 RID: 81
		// (get) Token: 0x060002ED RID: 749 RVA: 0x00007E95 File Offset: 0x00006095
		protected override Type ExhibitType
		{
			get
			{
				return typeof(SihunYu);
			}
		}

		// Token: 0x060002EE RID: 750 RVA: 0x00007EA4 File Offset: 0x000060A4
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			this.Counter = 0;
			RandomGen randomGen = new RandomGen(base.GameRun.FinalBossSeed);
			this._twoPool.Shuffle(randomGen);
			this._onePool.Shuffle(randomGen);
			this._twoPoolStrong.Shuffle(randomGen);
			this._onePoolStrong.Shuffle(randomGen);
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded));
			this.React(PerformAction.Sfx("GuirenItem", 0f));
			this.React(PerformAction.EffectMessage(unit, "SeijaExhibitManager", "AddExhibit", this));
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x060002EF RID: 751 RVA: 0x00007F53 File Offset: 0x00006153
		// (set) Token: 0x060002F0 RID: 752 RVA: 0x00007F5B File Offset: 0x0000615B
		private int Counter { get; set; }

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x060002F1 RID: 753 RVA: 0x00007F64 File Offset: 0x00006164
		// (set) Token: 0x060002F2 RID: 754 RVA: 0x00007F6C File Offset: 0x0000616C
		private bool IsStrong { get; set; }

		// Token: 0x060002F3 RID: 755 RVA: 0x00007F75 File Offset: 0x00006175
		private IEnumerable<BattleAction> OnOwnerTurnEnded(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			Unit owner = base.Owner;
			Seija seija = owner as Seija;
			if (seija != null)
			{
				base.NotifyActivating();
				if (this.Counter % 3 == 0)
				{
					if (!this.IsStrong && seija.ItemCount >= 4)
					{
						this.IsStrong = true;
						this.Counter = 0;
					}
					List<ValueTuple<Type, Type>> list = (this.IsStrong ? this._twoPoolStrong : this._twoPool);
					int num = this.Counter / 3 % list.Count;
					ValueTuple<Type, Type> valueTuple = list[num];
					Type item = valueTuple.Item1;
					Type b = valueTuple.Item2;
					if (!base.Battle.IsAnyoneInRootIndex(0))
					{
						yield return new SpawnEnemyAction(seija, item, 0, 0f, 0.3f, true);
					}
					if (!base.Battle.IsAnyoneInRootIndex(1))
					{
						yield return new SpawnEnemyAction(seija, b, 1, 0f, 0.3f, true);
					}
					b = null;
				}
				else
				{
					List<Type> list2 = (this.IsStrong ? this._onePoolStrong : this._onePool);
					int num2 = (this.Counter / 3 * 2 + this.Counter % 3 - 1) % list2.Count;
					Type type = list2[num2];
					int num3 = this.Counter % 3 + 2;
					if (!base.Battle.IsAnyoneInRootIndex(num3))
					{
						yield return new SpawnEnemyAction(seija, type, num3, 0f, 0.3f, true);
					}
				}
				int num4 = this.Counter + 1;
				this.Counter = num4;
			}
			yield break;
		}

		// Token: 0x060002F4 RID: 756 RVA: 0x00007F88 File Offset: 0x00006188
		public SihunYuSe()
		{
			List<ValueTuple<Type, Type>> list = new List<ValueTuple<Type, Type>>();
			list.Add(new ValueTuple<Type, Type>(typeof(RavenWen3), typeof(RavenGuo3)));
			list.Add(new ValueTuple<Type, Type>(typeof(MaoyuBlack), typeof(MaoyuBlack)));
			this._twoPool = list;
			List<Type> list2 = new List<Type>();
			list2.Add(typeof(WhiteFairy));
			list2.Add(typeof(DollBlue));
			list2.Add(typeof(Yaoshi));
			list2.Add(typeof(Scout));
			this._onePool = list2;
			List<ValueTuple<Type, Type>> list3 = new List<ValueTuple<Type, Type>>();
			list3.Add(new ValueTuple<Type, Type>(typeof(YinyangyuRed), typeof(YinyangyuBlue)));
			list3.Add(new ValueTuple<Type, Type>(typeof(Purifier), typeof(Purifier)));
			list3.Add(new ValueTuple<Type, Type>(typeof(ShenlingWhite), typeof(ShenlingPurple)));
			this._twoPoolStrong = list3;
			List<Type> list4 = new List<Type>();
			list4.Add(typeof(BlackFairy));
			list4.Add(typeof(Terminator));
			list4.Add(typeof(WaterGirl));
			list4.Add(typeof(LangTiangou));
			list4.Add(typeof(YaTiangou));
			list4.Add(typeof(SuwakoLimao));
			this._onePoolStrong = list4;
			base..ctor();
		}

		// Token: 0x04000027 RID: 39
		private readonly List<ValueTuple<Type, Type>> _twoPool;

		// Token: 0x04000028 RID: 40
		private readonly List<Type> _onePool;

		// Token: 0x04000029 RID: 41
		private readonly List<ValueTuple<Type, Type>> _twoPoolStrong;

		// Token: 0x0400002A RID: 42
		private readonly List<Type> _onePoolStrong;
	}
}
