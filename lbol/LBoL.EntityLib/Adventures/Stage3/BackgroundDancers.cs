using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Exhibits.Common;
using UnityEngine;
using Yarn;

namespace LBoL.EntityLib.Adventures.Stage3
{
	// Token: 0x02000504 RID: 1284
	[AdventureInfo(WeighterType = typeof(BackgroundDancers.BackgroundDancersWeighter))]
	public sealed class BackgroundDancers : Adventure
	{
		// Token: 0x060010DF RID: 4319 RVA: 0x0001DF58 File Offset: 0x0001C158
		protected override void InitVariables(IVariableStorage storage)
		{
			this._1Out10Hp = Mathf.CeilToInt((float)base.GameRun.Player.MaxHp * 0.09f);
			this._2Out10Hp = 2 * this._1Out10Hp;
			this._money = 100;
			this._maxHp = 4;
			this._power = 30;
			this._img2Alpha = 0;
			storage.SetValue("$hpLose", (float)this._1Out10Hp);
			storage.SetValue("$doubleHpLose", (float)this._2Out10Hp);
			storage.SetValue("$moneyGain", (float)this._money);
			storage.SetValue("$maxHpGain", (float)this._maxHp);
			storage.SetValue("$power", (float)this._power);
		}

		// Token: 0x060010E0 RID: 4320 RVA: 0x0001E010 File Offset: 0x0001C210
		[RuntimeCommand("rollOptions", "")]
		[UsedImplicitly]
		public void RollOptions()
		{
			for (int i = 0; i < 6; i++)
			{
				this._optionTitles[i] = base.Storage.GetValue(string.Format("$option{0}Source", i + 1));
				this._optionDialogues[i] = base.Storage.GetValue(string.Format("$option{0}Dialogue", i + 1));
			}
			this._optionIndices = new UniqueRandomPool<int>(false)
			{
				{ 0, 1f },
				{ 1, 1f },
				{ 2, 1f },
				{ 3, 1f },
				{ 4, 1f },
				{ 5, 1f }
			}.SampleMany(base.GameRun.AdventureRng, 3, true);
			base.Storage.SetValue("$option1", this._optionTitles[this._optionIndices[0]]);
			base.Storage.SetValue("$option1Dialogue", this._optionDialogues[this._optionIndices[0]]);
			base.Storage.SetValue("$option2", this._optionTitles[this._optionIndices[1]]);
			base.Storage.SetValue("$option2Dialogue", this._optionDialogues[this._optionIndices[1]]);
			base.Storage.SetValue("$option3", this._optionTitles[this._optionIndices[2]]);
			base.Storage.SetValue("$option3Dialogue", this._optionDialogues[this._optionIndices[2]]);
			this.toolCards = base.GameRun.RollCards(base.GameRun.AdventureRng, new CardWeightTable(RarityWeightTable.EliteCard, OwnerWeightTable.Valid, CardTypeWeightTable.OnlyTool, false), 2, false, false, null);
			base.Storage.SetValue("$reward2Rare", this.toolCards[0].Id);
			base.Storage.SetValue("$reward2Rare2", this.toolCards[1].Id);
			this.exhibitReward = base.Stage.GetSpecialAdventureExhibit();
			base.Storage.SetValue("$IsSentinel", this.exhibitReward.Config.IsSentinel);
			base.Storage.SetValue("$reward5Exhibit", this.exhibitReward.Config.Id);
			this.abilityCard = base.GameRun.RollCards(base.GameRun.AdventureRng, new CardWeightTable(CardTypeWeightTable.OnlyAbility), 1, false, false, null);
			base.Storage.SetValue("$reward6Ability", this.abilityCard[0].Config.Id);
			for (int j = 0; j < 3; j++)
			{
				switch (this._optionIndices[j])
				{
				case 0:
				case 2:
				case 3:
					break;
				case 1:
					base.Storage.SetValue("$tip2Rare", (float)(j + 1));
					break;
				case 4:
					base.Storage.SetValue("$tip5Exhibit", (float)(j + 1));
					break;
				case 5:
					base.Storage.SetValue("$tip6Ability", (float)(j + 1));
					break;
				default:
					throw new ArgumentException();
				}
			}
		}

		// Token: 0x060010E1 RID: 4321 RVA: 0x0001E31C File Offset: 0x0001C51C
		[RuntimeCommand("selectOption", "")]
		[UsedImplicitly]
		public IEnumerator SelectOption(int index)
		{
			int selected = this._optionIndices[index - 1];
			switch (selected)
			{
			case 0:
				this.ForMoney();
				break;
			case 1:
				this.ForTechnique();
				break;
			case 2:
				this.ForStrength();
				break;
			case 3:
				this.ForPower();
				break;
			case 4:
				yield return this.ForExhibit();
				break;
			case 5:
				this.ForAbility();
				break;
			default:
				throw new ArgumentException();
			}
			UniqueRandomPool<int> uniqueRandomPool = new UniqueRandomPool<int>(false);
			for (int i = 0; i < 6; i++)
			{
				if (!Enumerable.Contains<int>(this._optionIndices, i))
				{
					uniqueRandomPool.Add(i, 1f);
				}
			}
			int num = uniqueRandomPool.Sample(base.GameRun.AdventureRng);
			this._optionIndices[index - 1] = num;
			base.Storage.SetValue("$option1", this._optionTitles[this._optionIndices[0]]);
			base.Storage.SetValue("$option1Dialogue", this._optionDialogues[this._optionIndices[0]]);
			base.Storage.SetValue("$option2", this._optionTitles[this._optionIndices[1]]);
			base.Storage.SetValue("$option2Dialogue", this._optionDialogues[this._optionIndices[1]]);
			base.Storage.SetValue("$option3", this._optionTitles[this._optionIndices[2]]);
			base.Storage.SetValue("$option3Dialogue", this._optionDialogues[this._optionIndices[2]]);
			if (selected == 1)
			{
				this.toolCards = base.GameRun.RollCards(base.GameRun.AdventureRng, new CardWeightTable(RarityWeightTable.EliteCard, OwnerWeightTable.Valid, CardTypeWeightTable.OnlyTool, false), 2, false, false, null);
				base.Storage.SetValue("$reward2Rare", this.toolCards[0].Id);
				base.Storage.SetValue("$reward2Rare2", this.toolCards[1].Id);
			}
			if (selected == 4)
			{
				this.exhibitReward = base.Stage.GetSpecialAdventureExhibit();
				base.Storage.SetValue("$IsSentinel", this.exhibitReward.Config.IsSentinel);
				base.Storage.SetValue("$reward5Exhibit", this.exhibitReward.Config.Id);
			}
			if (selected == 5)
			{
				this.abilityCard = base.GameRun.RollCards(base.GameRun.AdventureRng, new CardWeightTable(CardTypeWeightTable.OnlyAbility), 1, false, false, null);
				base.Storage.SetValue("$reward6Ability", this.abilityCard[0].Config.Id);
			}
			base.Storage.SetValue("$tip2Rare", 0f);
			base.Storage.SetValue("$tip5Exhibit", 0f);
			base.Storage.SetValue("$tip6Ability", 0f);
			for (int j = 0; j < 3; j++)
			{
				switch (this._optionIndices[j])
				{
				case 0:
				case 2:
				case 3:
					break;
				case 1:
					base.Storage.SetValue("$tip2Rare", (float)(j + 1));
					break;
				case 4:
					base.Storage.SetValue("$tip5Exhibit", (float)(j + 1));
					break;
				case 5:
					base.Storage.SetValue("$tip6Ability", (float)(j + 1));
					break;
				default:
					throw new ArgumentException();
				}
			}
			yield break;
		}

		// Token: 0x060010E2 RID: 4322 RVA: 0x0001E332 File Offset: 0x0001C532
		private void ChangeAlpha()
		{
			if (this._img2Alpha < 100)
			{
				this._img2Alpha += 20;
				this._img2Alpha = Mathf.Clamp(this._img2Alpha, 0, 100);
			}
			Debug.Log(this._img2Alpha);
		}

		// Token: 0x060010E3 RID: 4323 RVA: 0x0001E374 File Offset: 0x0001C574
		private void ForMoney()
		{
			base.GameRun.Damage(this._1Out10Hp, DamageType.HpLose, true, true, this);
			if (base.GameRun.Status == GameRunStatus.Failure)
			{
				return;
			}
			base.GameRun.GainMoney(this._money, true, new VisualSourceData
			{
				SourceType = VisualSourceType.Vn
			});
		}

		// Token: 0x060010E4 RID: 4324 RVA: 0x0001E3C3 File Offset: 0x0001C5C3
		private void ForTechnique()
		{
			base.GameRun.Damage(this._2Out10Hp, DamageType.HpLose, true, true, this);
			if (base.GameRun.Status == GameRunStatus.Failure)
			{
				return;
			}
			base.GameRun.AddDeckCards(this.toolCards, false, null);
		}

		// Token: 0x060010E5 RID: 4325 RVA: 0x0001E3FC File Offset: 0x0001C5FC
		private void ForStrength()
		{
			base.GameRun.Damage(this._1Out10Hp, DamageType.HpLose, true, true, this);
			if (base.GameRun.Status == GameRunStatus.Failure)
			{
				return;
			}
			base.GameRun.GainMaxHp(this._maxHp, true, true);
		}

		// Token: 0x060010E6 RID: 4326 RVA: 0x0001E435 File Offset: 0x0001C635
		private void ForPower()
		{
			base.GameRun.Damage(this._1Out10Hp, DamageType.HpLose, true, true, this);
			if (base.GameRun.Status == GameRunStatus.Failure)
			{
				return;
			}
			base.GameRun.GainPower(this._power, false);
		}

		// Token: 0x060010E7 RID: 4327 RVA: 0x0001E46D File Offset: 0x0001C66D
		private IEnumerator ForExhibit()
		{
			base.GameRun.Damage(this._2Out10Hp, DamageType.HpLose, true, true, this);
			if (base.GameRun.Status == GameRunStatus.Failure)
			{
				yield break;
			}
			yield return base.GameRun.GainExhibitRunner(this.exhibitReward, true, new VisualSourceData
			{
				SourceType = VisualSourceType.Vn,
				Index = 1
			});
			Exhibit exhibit = this.exhibitReward;
			if ((exhibit is Cookie || exhibit is Taozi || exhibit is Pingguo || exhibit is Putao) && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.CookieInBackdoor);
			}
			yield break;
		}

		// Token: 0x060010E8 RID: 4328 RVA: 0x0001E47C File Offset: 0x0001C67C
		private void ForAbility()
		{
			base.GameRun.Damage(this._1Out10Hp, DamageType.HpLose, true, true, this);
			if (base.GameRun.Status == GameRunStatus.Failure)
			{
				return;
			}
			base.GameRun.AddDeckCards(this.abilityCard, false, null);
		}

		// Token: 0x0400011B RID: 283
		private int _1Out10Hp;

		// Token: 0x0400011C RID: 284
		private int _money;

		// Token: 0x0400011D RID: 285
		private int _maxHp;

		// Token: 0x0400011E RID: 286
		private int _2Out10Hp;

		// Token: 0x0400011F RID: 287
		private int _power;

		// Token: 0x04000120 RID: 288
		private int _img2Alpha;

		// Token: 0x04000121 RID: 289
		private readonly string[] _optionTitles = new string[6];

		// Token: 0x04000122 RID: 290
		private readonly string[] _optionDialogues = new string[6];

		// Token: 0x04000123 RID: 291
		private int[] _optionIndices;

		// Token: 0x04000124 RID: 292
		private Card[] toolCards;

		// Token: 0x04000125 RID: 293
		private Exhibit exhibitReward;

		// Token: 0x04000126 RID: 294
		private Card[] abilityCard;

		// Token: 0x02000A5D RID: 2653
		private class BackgroundDancersWeighter : IAdventureWeighter
		{
			// Token: 0x06003721 RID: 14113 RVA: 0x00085BC9 File Offset: 0x00083DC9
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((gameRun.Player.Hp >= gameRun.Player.MaxHp / 2) ? 1 : 0);
			}
		}
	}
}
