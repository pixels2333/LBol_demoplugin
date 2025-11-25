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
	[UsedImplicitly]
	public sealed class ShendengSe : SeijaSe
	{
		protected override Type ExhibitType
		{
			get
			{
				return typeof(Shendeng);
			}
		}
		private int Counter { get; set; }
		private List<Type> Types { get; set; }
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
