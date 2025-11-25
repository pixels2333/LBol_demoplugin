using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using UnityEngine;
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class Xiguadao : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			bool flag = Random.Range(0f, 1f) > 0.5f;
			Guns guns = new Guns(flag ? "推进之银up" : "推进之银down");
			for (int i = 1; i < base.Value1; i++)
			{
				guns.Add(flag ? ((i % 2 == 0) ? "推进之银up" : "推进之银down") : ((i % 2 == 1) ? "推进之银up" : "推进之银down"));
			}
			foreach (GunPair gunPair in guns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			yield return base.BuffAction<Firepower>(base.Value2, 0, 0, 0, 0.2f);
			yield break;
			yield break;
		}
		private const string UpGun = "推进之银up";
		private const string DownGun = "推进之银down";
	}
}
