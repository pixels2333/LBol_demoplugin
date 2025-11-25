using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Opponent;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Character
{
	[UsedImplicitly]
	public sealed class Siji : EnemyUnit
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			this._reason = 0;
			this._gunIndex = 0;
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			string spellcard = "净颇梨审判";
			Type type = typeof(Reimu);
			string id = base.GameRun.Player.Id;
			if (!(id == "Reimu"))
			{
				if (!(id == "Marisa"))
				{
					if (!(id == "Sakuya"))
					{
						if (!(id == "Cirno"))
						{
							if (!(id == "Koishi"))
							{
								if (!(id == "Alice"))
								{
									spellcard += " -博丽灵梦-";
								}
								else
								{
									spellcard += " -爱丽丝-";
									type = typeof(Alice);
								}
							}
							else
							{
								spellcard += " -古明地恋-";
								type = typeof(Koishi);
							}
						}
						else
						{
							spellcard += " -琪露诺-";
							type = typeof(Cirno);
						}
					}
					else
					{
						spellcard += " -十六夜咲夜-";
						type = typeof(Sakuya);
					}
				}
				else
				{
					spellcard += " -雾雨魔理沙-";
					type = typeof(Marisa);
				}
			}
			else
			{
				spellcard += " -博丽灵梦-";
			}
			yield return PerformAction.Chat(this, "Chat.Siji1".Localize(true), 3f, 0f, 3.2f, true);
			string str = "Chat.Siji2".LocalizeFormat(new object[] { base.Battle.Player.GetName() });
			yield return PerformAction.Animation(this, "shoot2", 0f, null, 0f, -1);
			yield return PerformAction.Chat(this, str, 3f, 0f, 3.2f, true);
			yield return PerformAction.Animation(this, "spell", 0f, null, 0f, -1);
			yield return PerformAction.Spell(this, spellcard);
			yield return new SpawnEnemyAction(this, type, 2, 0f, 0.3f, false);
			int count = base.Battle.Player.Exhibits.Count;
			int num = 1;
			if (count >= 20)
			{
				num = 3;
			}
			else if (count >= 10)
			{
				num = 2;
			}
			yield return new ApplyStatusEffectAction<SijiZui>(this, new int?(num), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			int level = base.GetStatusEffect<SijiZui>().Level;
			yield return base.AttackMove(base.GetMove(this._gunIndex), "十王审判" + this._gunIndex.ToString(), base.Damage1 + base.Damage2 * (level - 1) + this._gunIndex, 1, this._gunIndex >= 3, "Instant", true);
			if (base.TurnCounter % 3 == 2)
			{
				this._reason++;
				if (this._reason >= 4)
				{
					this._reason = 1;
				}
				yield return new SimpleEnemyMove(Intention.PositiveEffect(), this.BuffActions());
			}
			if (this._gunIndex < 9)
			{
				this._gunIndex++;
			}
			yield break;
		}
		private IEnumerable<BattleAction> BuffActions()
		{
			yield return new ApplyStatusEffectAction<SijiZui>(this, new int?((this._reason == 3) ? 2 : 1), default(int?), default(int?), default(int?), 0f, true);
			yield return PerformAction.Animation(this, "shoot2", 0f, null, 0f, -1);
			string text;
			if (base.GameRun.ExtraFlags.Contains("MystiaSin") && base.TurnCounter < 7 && this._reason == 1)
			{
				text = "Chat.SijiMystia";
			}
			else
			{
				text = "Chat.SijiTurn" + this._reason.ToString();
			}
			yield return PerformAction.Chat(this, text.Localize(true), 3f, 0f, 0f, true);
			yield break;
		}
		private int _reason;
		private int _gunIndex;
	}
}
