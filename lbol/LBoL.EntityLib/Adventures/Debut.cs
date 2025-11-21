using System;
using System.Collections;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.JadeBoxes;
using LBoL.Core.Randoms;
using Yarn;

namespace LBoL.EntityLib.Adventures
{
	// Token: 0x02000500 RID: 1280
	public sealed class Debut : Adventure
	{
		// Token: 0x060010CE RID: 4302 RVA: 0x0001D424 File Offset: 0x0001B624
		protected override void InitVariables(IVariableStorage storage)
		{
			int num = 20;
			int num2 = 200;
			if (base.GameRun.Puzzles.HasFlag(PuzzleFlag.HalfDrug))
			{
				num = ((float)num * 0.5f).RoundToInt();
				num2 = ((float)num2 * 0.5f).RoundToInt();
			}
			storage.SetValue("$money", 100f);
			storage.SetValue("$maxHp", (float)num);
			storage.SetValue("$power", (float)num2);
			Card[] array = base.GameRun.RollCards(base.GameRun.DebutRng, new CardWeightTable(RarityWeightTable.OnlyUncommon, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), 3, false, false, null);
			Card[] array2 = base.GameRun.RollCards(base.GameRun.DebutRng, new CardWeightTable(RarityWeightTable.OnlyRare, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), 1, false, false, null);
			this._exhibit = base.Stage.GetNeutralShiningExhibit();
			storage.SetValue("$uncommonCard1", array[0].Id);
			storage.SetValue("$uncommonCard2", array[1].Id);
			storage.SetValue("$uncommonCard3", array[2].Id);
			storage.SetValue("$rareCard", array2[0].Id);
			storage.SetValue("$exhibit", this._exhibit.Id);
			Card card = base.GameRun.RollTransformCard(base.GameRun.DebutRng, new CardWeightTable(RarityWeightTable.EliteCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), false, false, null);
			storage.SetValue("$transformCard", card.Id);
			int money = base.GameRun.Money;
			storage.SetValue("$allMoney", (float)money);
			Exhibit exhibit = base.Stage.RollExhibitInAdventure(new ExhibitWeightTable(RarityWeightTable.OnlyRare, AppearanceWeightTable.NotInShop), null);
			storage.SetValue("$rareExhibit", exhibit.Id);
			storage.SetValue("$huiyeFlag", base.GameRun.KaguyaInDebut);
			storage.SetValue("$twoColorStart", base.GameRun.HasJadeBox<TwoColorStart>());
		}

		// Token: 0x060010CF RID: 4303 RVA: 0x0001D628 File Offset: 0x0001B828
		[RuntimeCommand("rollBonus", "")]
		[UsedImplicitly]
		public void RollBonus()
		{
			this._bonusNos = new UniqueRandomPool<int>(false)
			{
				{ 0, 1f },
				{ 1, 1f },
				{ 2, 1f },
				{ 3, 1f },
				{ 4, 1f },
				{ 5, 1f }
			}.SampleMany(base.GameRun.DebutRng, 2, true);
			base.Storage.SetValue("$bonusNo1", (float)this._bonusNos[0]);
			base.Storage.SetValue("$bonusNo2", (float)this._bonusNos[1]);
			for (int i = 0; i < 6; i++)
			{
				this._optionTitles[i] = base.Storage.GetValue(string.Format("$option{0}Source", i + 1));
			}
			base.Storage.SetValue("$bonusOption1", this._optionTitles[this._bonusNos[0]]);
			base.Storage.SetValue("$bonusOption2", this._optionTitles[this._bonusNos[1]]);
			for (int j = 0; j < 2; j++)
			{
				base.Storage.SetValue(string.Format("$bonusTarget{0}", j + 1), string.Format("Bonus{0}", this._bonusNos[j] + 1));
				switch (this._bonusNos[j])
				{
				case 0:
					base.Storage.SetValue("$tipUncommonCard", (float)(j + 3));
					break;
				case 1:
					base.Storage.SetValue("$tipRareCard", (float)(j + 3));
					break;
				case 2:
					base.Storage.SetValue("$tipRareExhibit", (float)(j + 3));
					break;
				case 5:
					base.Storage.SetValue("$tipTransformCard", (float)(j + 3));
					break;
				}
			}
		}

		// Token: 0x060010D0 RID: 4304 RVA: 0x0001D803 File Offset: 0x0001BA03
		[RuntimeCommand("exchangeExhibit", "")]
		[UsedImplicitly]
		public IEnumerator ExchangeExhibit(int optionIndex = -1)
		{
			base.GameRun.LoseExhibit(base.GameRun.Player.Exhibits[0], false, true);
			yield return base.GameRun.GainExhibitRunner(this._exhibit, true, new VisualSourceData
			{
				SourceType = VisualSourceType.Vn,
				Index = optionIndex
			});
			yield break;
		}

		// Token: 0x04000114 RID: 276
		private Exhibit _exhibit;

		// Token: 0x04000115 RID: 277
		private int[] _bonusNos;

		// Token: 0x04000116 RID: 278
		private readonly string[] _optionTitles = new string[6];

		// Token: 0x04000117 RID: 279
		private const int Money = 100;
	}
}
