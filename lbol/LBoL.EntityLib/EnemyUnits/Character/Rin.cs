using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal.Guihuos;
using LBoL.EntityLib.PlayerUnits;
using LBoL.EntityLib.StatusEffects.Enemy;
using UnityEngine;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x02000245 RID: 581
	[UsedImplicitly]
	public sealed class Rin : EnemyUnit<IRinView>
	{
		// Token: 0x170000FD RID: 253
		// (get) Token: 0x06000914 RID: 2324 RVA: 0x0001399D File Offset: 0x00011B9D
		// (set) Token: 0x06000915 RID: 2325 RVA: 0x000139A5 File Offset: 0x00011BA5
		private Rin.MoveType Next { get; set; }

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x06000916 RID: 2326 RVA: 0x000139B0 File Offset: 0x00011BB0
		private string Spell
		{
			get
			{
				return base.GetSpellCardName(default(int?), 3);
			}
		}

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x06000917 RID: 2327 RVA: 0x000139CD File Offset: 0x00011BCD
		// (set) Token: 0x06000918 RID: 2328 RVA: 0x000139D5 File Offset: 0x00011BD5
		private StatusEffect SummonSe { get; set; }

		// Token: 0x06000919 RID: 2329 RVA: 0x000139E0 File Offset: 0x00011BE0
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Rin.MoveType.Summon;
			base.CountDown = 4;
			List<Rin.RinOrbColor> list = new List<Rin.RinOrbColor>();
			list.Add(Rin.RinOrbColor.Red);
			list.Add(Rin.RinOrbColor.Blue);
			list.Add(Rin.RinOrbColor.Green);
			List<Rin.RinOrbColor> list2 = list;
			list2.Shuffle(base.EnemyMoveRng);
			this._rinOrbs.Enqueue(new Rin.RinOrbData(list2[0], 0));
			this._rinOrbs.Enqueue(new Rin.RinOrbData(list2[1], 1));
			this._rinOrbs.Enqueue(new Rin.RinOrbData(list2[2], 2));
			foreach (Rin.RinOrbData rinOrbData in this._rinOrbs)
			{
				IRinView view = base.View;
				if (view != null)
				{
					view.SetOrb(Rin.GetEffectName(rinOrbData.Color), rinOrbData.OrbitIndex);
				}
			}
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.ReactBattleEvent<DieEventArgs>(base.Battle.EnemyDied, new Func<DieEventArgs, IEnumerable<BattleAction>>(this.OnEnemyDied));
		}

		// Token: 0x0600091A RID: 2330 RVA: 0x00013B08 File Offset: 0x00011D08
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			int? num = new int?(3);
			int? num2 = new int?(base.Count1);
			yield return new ApplyStatusEffectAction<RinSummon>(this, num, default(int?), default(int?), num2, 0f, true);
			this.SummonSe = base.GetStatusEffect<RinSummon>();
			if (base.Battle.Player is Koishi)
			{
				string text = "Chat.RinKoishi1".LocalizeFormat(new object[] { base.Battle.Player.ShortNameWithColor });
				yield return PerformAction.Chat(this, text, 3.5f, 0.5f, 0f, true);
			}
			else if (Enumerable.Any<Exhibit>(base.Battle.Player.Exhibits))
			{
				IReadOnlyList<Exhibit> exhibits = base.Battle.Player.Exhibits;
				int num3 = Random.Range(0, exhibits.Count);
				Exhibit exhibit = exhibits[num3];
				string text2 = "Chat.Rin1".LocalizeFormat(new object[] { exhibit.GetName() });
				yield return PerformAction.Chat(this, text2, 3.5f, 0.5f, 0f, true);
			}
			yield break;
		}

		// Token: 0x0600091B RID: 2331 RVA: 0x00013B18 File Offset: 0x00011D18
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs arg)
		{
			if (base.IsAlive)
			{
				Unit unit = arg.Unit;
				if (unit is Guihuo)
				{
					Rin.RinOrbColor color = Rin.GetColor(unit);
					if (color == Rin.RinOrbColor.None)
					{
						throw new InvalidOperationException("Died guihuo enemy has no color set.");
					}
					if (this._rinOrbs.Count >= 3)
					{
						throw new InvalidOperationException("Rin can only holds 3 orbs.");
					}
					this._rinOrbIndex++;
					this._rinOrbIndex %= 3;
					this._rinOrbs.Enqueue(new Rin.RinOrbData(color, this._rinOrbIndex));
				}
				IRinView view = base.View;
				yield return new WaitForCoroutineAction((view != null) ? view.RecycleOrb() : null);
				yield return new ApplyStatusEffectAction<RinAura>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
				int num = Enumerable.Count<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit e) => e is Guihuo);
				if (this.Next == Rin.MoveType.Explode && num <= 1)
				{
					if (!base.IsInTurn)
					{
						if (base.Battle.Player is Koishi)
						{
							yield return PerformAction.Chat(this, "Chat.RinKoishi2".LocalizeFormat(new object[] { base.Battle.Player.ShortNameWithColor }), 3f, 1f, 0f, true);
						}
						else
						{
							yield return PerformAction.Chat(this, "Chat.Rin2".Localize(true), 3f, 1f, 0f, true);
						}
						this.Next = Rin.MoveType.Defend;
						base.UpdateTurnMoves();
					}
				}
				else if (this.Next == Rin.MoveType.Spell && num == 0 && base.CountDown <= 0 && !base.IsInTurn)
				{
					if (base.Battle.Player is Koishi)
					{
						yield return PerformAction.Chat(this, "Chat.RinKoishi2".LocalizeFormat(new object[] { base.Battle.Player.ShortNameWithColor }), 3f, 1f, 0f, true);
					}
					else
					{
						yield return PerformAction.Chat(this, "Chat.Rin2".Localize(true), 3f, 1f, 0f, true);
					}
					this.Next = Rin.MoveType.SpellNoGuihuo;
					base.UpdateTurnMoves();
				}
			}
			yield break;
		}

		// Token: 0x0600091C RID: 2332 RVA: 0x00013B2F File Offset: 0x00011D2F
		private IEnumerable<BattleAction> SummonActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Animation(this, "shoot3", 0f, null, 0f, -1);
			int summonAmountMax = this.SummonSe.Level;
			int num = Enumerable.Count<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy is Guihuo);
			if (summonAmountMax + num > 3)
			{
				Debug.LogError("Rin is Trying to summon too many guihuos, which should not happen.");
				summonAmountMax = 3 - num;
			}
			int num2;
			for (int i = 0; i < summonAmountMax; i = num2 + 1)
			{
				Rin.RinOrbData rinOrbData = this._rinOrbs.Dequeue();
				int summonIndex = -1;
				for (int j = 0; j < 3; j++)
				{
					this._summonRootIndex++;
					this._summonRootIndex %= 3;
					if (!base.Battle.IsAnyoneInRootIndex(this._summonRootIndex))
					{
						summonIndex = this._summonRootIndex;
						break;
					}
				}
				if (summonIndex < 0)
				{
					throw new InvalidOperationException("Rin trying to summon when there is no place. (3 Guihuo alive.)");
				}
				this.SummonSe.Level--;
				yield return new SpawnEnemyAction(this, Rin.GetGuihuoType(rinOrbData.Color), summonIndex, 0f, 0.3f, true);
				EnemyUnit enemyByRootIndex = base.Battle.GetEnemyByRootIndex(summonIndex);
				if (enemyByRootIndex != null)
				{
					IRinView view = base.View;
					yield return new WaitForCoroutineAction((view != null) ? view.MoveOrbToEnemy(enemyByRootIndex) : null);
				}
				num2 = i;
			}
			yield break;
		}

		// Token: 0x0600091D RID: 2333 RVA: 0x00013B3F File Offset: 0x00011D3F
		private IEnumerable<BattleAction> ExplodeActions()
		{
			List<EnemyUnit> allGuihuo = Enumerable.ToList<EnemyUnit>(Enumerable.Where<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy is Guihuo));
			if (allGuihuo.Count >= 1)
			{
				Rin.<>c__DisplayClass18_0 CS$<>8__locals1 = new Rin.<>c__DisplayClass18_0();
				yield return new EnemyMoveAction(this, base.GetMove(2), true);
				yield return PerformAction.Animation(this, "skill1", 0.3f, null, 0f, -1);
				CS$<>8__locals1.lowestHpGuihuo = Enumerable.First<EnemyUnit>(allGuihuo);
				IEnumerable<EnemyUnit> enumerable = allGuihuo;
				Func<EnemyUnit, bool> func;
				if ((func = CS$<>8__locals1.<>9__1) == null)
				{
					func = (CS$<>8__locals1.<>9__1 = (EnemyUnit enemy) => enemy.Hp < CS$<>8__locals1.lowestHpGuihuo.Hp);
				}
				foreach (EnemyUnit enemyUnit in Enumerable.Where<EnemyUnit>(enumerable, func))
				{
					CS$<>8__locals1.lowestHpGuihuo = enemyUnit;
				}
				yield return new ForceKillAction(this, CS$<>8__locals1.lowestHpGuihuo);
				CS$<>8__locals1 = null;
			}
			yield break;
		}

		// Token: 0x0600091E RID: 2334 RVA: 0x00013B4F File Offset: 0x00011D4F
		private IEnumerable<BattleAction> SpellActions(bool noGuihuo)
		{
			base.CountDown = 4;
			yield return new EnemyMoveAction(this, this.Spell, true);
			yield return PerformAction.Spell(this, "死灰复燃");
			if (noGuihuo)
			{
				yield return new ApplyStatusEffectAction<Graze>(this, new int?(base.Defend), default(int?), default(int?), default(int?), 0f, true);
			}
			else
			{
				List<EnemyUnit> list = Enumerable.ToList<EnemyUnit>(Enumerable.Where<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy is Guihuo));
				if (list.Count == 0)
				{
					Debug.LogError("There is no guihuo alive, Rin should use SpellNoGuihuo.");
				}
				else
				{
					foreach (EnemyUnit guihuo in list)
					{
						yield return PerformAction.Animation(this, "skill1", 0.3f, null, 0f, -1);
						yield return new ForceKillAction(this, guihuo);
						guihuo = null;
					}
					List<EnemyUnit>.Enumerator enumerator = default(List<EnemyUnit>.Enumerator);
				}
			}
			yield return PerformAction.Animation(this, "shoot3", 0.3f, null, 0f, -1);
			int num = base.Count2 - this.SummonSe.Level;
			if (num > 0)
			{
				int? num2 = new int?(num);
				int? num3 = new int?(base.Count1);
				yield return new ApplyStatusEffectAction<RinSummon>(this, num2, default(int?), default(int?), num3, 0f, true);
				this.SummonSe.Count = this.SummonSe.Limit + 1;
			}
			yield break;
			yield break;
		}

		// Token: 0x0600091F RID: 2335 RVA: 0x00013B66 File Offset: 0x00011D66
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			IEnemyMove enemyMove;
			switch (this.Next)
			{
			case Rin.MoveType.Shoot:
				enemyMove = base.AttackMove(base.GetMove(0), "ERinShoot1", base.Damage1, 1, false, "Instant", false);
				break;
			case Rin.MoveType.Defend:
				enemyMove = base.DefendMove(this, base.GetMove(4), 0, 0, base.Defend, true, PerformAction.Animation(this, "defend", 0.5f, null, 0f, -1));
				break;
			case Rin.MoveType.Summon:
				enemyMove = new SimpleEnemyMove(Intention.Spawn(), this.SummonActions());
				break;
			case Rin.MoveType.Explode:
				enemyMove = new SimpleEnemyMove(Intention.ExplodeAlly(), this.ExplodeActions());
				break;
			case Rin.MoveType.Spell:
				enemyMove = new SimpleEnemyMove(Intention.SpellCard(this.Spell, default(int?), default(int?), false), this.SpellActions(false));
				break;
			case Rin.MoveType.SpellNoGuihuo:
				enemyMove = new SimpleEnemyMove(Intention.SpellCard(this.Spell, default(int?), default(int?), false), this.SpellActions(true));
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield return enemyMove;
			if (base.CountDown == 1)
			{
				yield return new SimpleEnemyMove(Intention.CountDown(base.CountDown));
			}
			yield break;
		}

		// Token: 0x06000920 RID: 2336 RVA: 0x00013B78 File Offset: 0x00011D78
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			int num2 = Enumerable.Count<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy is Guihuo);
			if (base.CountDown <= 0)
			{
				this.Next = ((num2 >= 1) ? Rin.MoveType.Spell : Rin.MoveType.SpellNoGuihuo);
				return;
			}
			if (this.SummonSe.Level > 0 && num2 <= 2)
			{
				this.Next = Rin.MoveType.Summon;
				return;
			}
			if (num2 == 2)
			{
				float num3 = base.EnemyMoveRng.NextFloat(0f, 1f);
				this.Next = ((num3 > 0.8f) ? Rin.MoveType.Explode : Rin.MoveType.Shoot);
				return;
			}
			if (num2 == 3)
			{
				this.Next = Rin.MoveType.Explode;
				return;
			}
			this.Next = Rin.MoveType.Shoot;
		}

		// Token: 0x06000921 RID: 2337 RVA: 0x00013C34 File Offset: 0x00011E34
		private static Type GetGuihuoType(Rin.RinOrbColor color)
		{
			Type type;
			switch (color)
			{
			case Rin.RinOrbColor.None:
				type = null;
				break;
			case Rin.RinOrbColor.Red:
				type = typeof(GuihuoRed);
				break;
			case Rin.RinOrbColor.Green:
				type = typeof(GuihuoGreen);
				break;
			case Rin.RinOrbColor.Blue:
				type = typeof(GuihuoBlue);
				break;
			default:
				throw new ArgumentOutOfRangeException("color", color, null);
			}
			return type;
		}

		// Token: 0x06000922 RID: 2338 RVA: 0x00013C98 File Offset: 0x00011E98
		private static Rin.RinOrbColor GetColor(Unit unit)
		{
			Rin.RinOrbColor rinOrbColor;
			if (!(unit is GuihuoRed))
			{
				if (!(unit is GuihuoGreen))
				{
					if (!(unit is GuihuoBlue))
					{
						rinOrbColor = Rin.RinOrbColor.None;
					}
					else
					{
						rinOrbColor = Rin.RinOrbColor.Blue;
					}
				}
				else
				{
					rinOrbColor = Rin.RinOrbColor.Green;
				}
			}
			else
			{
				rinOrbColor = Rin.RinOrbColor.Red;
			}
			return rinOrbColor;
		}

		// Token: 0x06000923 RID: 2339 RVA: 0x00013CCE File Offset: 0x00011ECE
		private static string GetEffectName(Rin.RinOrbColor color)
		{
			return "GuihuoTrail" + color.ToString();
		}

		// Token: 0x040000C8 RID: 200
		private readonly Queue<Rin.RinOrbData> _rinOrbs = new Queue<Rin.RinOrbData>(3);

		// Token: 0x040000CA RID: 202
		private const int SpellInterval = 4;

		// Token: 0x040000CB RID: 203
		private int _rinOrbIndex = 2;

		// Token: 0x040000CC RID: 204
		private int _summonRootIndex = -1;

		// Token: 0x0200075E RID: 1886
		private enum MoveType
		{
			// Token: 0x04000B2D RID: 2861
			Shoot,
			// Token: 0x04000B2E RID: 2862
			Defend,
			// Token: 0x04000B2F RID: 2863
			Summon,
			// Token: 0x04000B30 RID: 2864
			Explode,
			// Token: 0x04000B31 RID: 2865
			Spell,
			// Token: 0x04000B32 RID: 2866
			SpellNoGuihuo
		}

		// Token: 0x0200075F RID: 1887
		public class RinOrbData
		{
			// Token: 0x0600209C RID: 8348 RVA: 0x0004A9FF File Offset: 0x00048BFF
			public RinOrbData(Rin.RinOrbColor color, int orbitIndex)
			{
				this.Color = color;
				this.OrbitIndex = orbitIndex;
			}

			// Token: 0x1700057C RID: 1404
			// (get) Token: 0x0600209D RID: 8349 RVA: 0x0004AA15 File Offset: 0x00048C15
			public Rin.RinOrbColor Color { get; }

			// Token: 0x1700057D RID: 1405
			// (get) Token: 0x0600209E RID: 8350 RVA: 0x0004AA1D File Offset: 0x00048C1D
			public int OrbitIndex { get; }
		}

		// Token: 0x02000760 RID: 1888
		public enum RinOrbColor
		{
			// Token: 0x04000B36 RID: 2870
			None,
			// Token: 0x04000B37 RID: 2871
			Red,
			// Token: 0x04000B38 RID: 2872
			Green,
			// Token: 0x04000B39 RID: 2873
			Blue
		}
	}
}
