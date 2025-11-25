using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.EntityLib.Dolls
{
	[UsedImplicitly]
	public sealed class ChargeDoll : Doll
	{
		[UsedImplicitly]
		public new DamageInfo Damage
		{
			get
			{
				return DamageInfo.Reaction((float)this._counter, false);
			}
		}
		public override int? UpCounter
		{
			get
			{
				return new int?(this._counter);
			}
		}
		public override Color UpCounterColor
		{
			get
			{
				return Color.cyan;
			}
		}
		public override int? DownCounter
		{
			get
			{
				return new int?(base.Value1);
			}
		}
		public override void Initialize()
		{
			base.Initialize();
			this._counter = base.Config.Value1.GetValueOrDefault();
		}
		protected override IEnumerable<BattleAction> PassiveActions()
		{
			base.NotifyPassiveActivating();
			this._counter += base.Value1;
			this.NotifyChanged();
			yield return PerformAction.Doll(this, null, "", 0.1f, "ChargeDoll charge");
			yield break;
		}
		protected override IEnumerable<BattleAction> ActiveActions()
		{
			base.NotifyActiveActivating();
			yield return new DamageAction(base.Owner, base.Battle.RandomAliveEnemy, this.Damage, "Instant", GunType.Single);
			yield break;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			base.NotifyActiveActivating();
			yield return new DamageAction(base.Owner, base.Battle.RandomAliveEnemy, this.Damage, "Instant", GunType.Single);
			yield break;
		}
		private int _counter;
	}
}
