using System;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Adventures;
using LBoL.EntityLib.Exhibits;
namespace LBoL.EntityLib.Stages.NormalStages
{
	public abstract class NormalStageBase : Stage
	{
		protected NormalStageBase()
		{
			this.isNormalStage = true;
			base.EnemyPoolAct1 = new UniqueRandomPool<string>(true) { { "11", 1f } };
			base.EnemyPoolAct2 = new UniqueRandomPool<string>(true) { { "14", 1f } };
			base.EnemyPoolAct3 = new UniqueRandomPool<string>(true) { { "17", 1f } };
			base.EliteEnemyPool = new UniqueRandomPool<string>(true) { { "Aya", 1f } };
			base.SentinelExhibitType = typeof(KongZhanpinhe);
			base.SupplyAdventureType = typeof(Supply);
			base.TradeAdventureType = typeof(RinnosukeTrade);
			base.ShopExhibitWeightTable = new ExhibitWeightTable(new RarityWeightTable(0.5f, 0.33f, 0.17f, 0f), new AppearanceWeightTable(1f, 2f, 0f, 0f));
			base.ShopExhibitWeightTableShopOnly = new ExhibitWeightTable(new RarityWeightTable(0.5f, 0.33f, 0.17f, 0f), AppearanceWeightTable.OnlyShopOnly);
			base.EliteEnemyExhibitWeightTable = new ExhibitWeightTable(new RarityWeightTable(0.5f, 0.33f, 0.17f, 0f), new AppearanceWeightTable(1f, 0f, 1f, 0f));
			base.SupplyExhibitWeightTable = new ExhibitWeightTable(new RarityWeightTable(0.3f, 0.5f, 0.2f, 0f), new AppearanceWeightTable(1f, 0f, 1f, 0f));
			base.DrinkTeaAdditionalCardWeight = new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.Hierarchy, CardTypeWeightTable.CanBeLoot, false);
			base.EnemyCardOnlyPlayerWeight = new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot, false);
			base.EnemyCardWithFriendWeight = new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.PlayerAndFriend, CardTypeWeightTable.CanBeLoot, false);
			base.EnemyCardNeutralWeight = new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.OnlyNeutral, CardTypeWeightTable.CanBeLoot, false);
			base.EnemyCardWeight = new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.Hierarchy, CardTypeWeightTable.CanBeLoot, false);
			base.EliteEnemyCardCharaWeight = new CardWeightTable(RarityWeightTable.EliteCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot, false);
			base.EliteEnemyCardFriendWeight = new CardWeightTable(RarityWeightTable.EliteCard, OwnerWeightTable.PlayerAndFriend, CardTypeWeightTable.CanBeLoot, false);
			base.EliteEnemyCardNeutralWeight = new CardWeightTable(RarityWeightTable.EliteCard, OwnerWeightTable.OnlyNeutral, CardTypeWeightTable.CanBeLoot, false);
			base.EliteEnemyCardWeight = new CardWeightTable(RarityWeightTable.EliteCard, OwnerWeightTable.Hierarchy, CardTypeWeightTable.CanBeLoot, false);
			base.BossCardCharaWeight = new CardWeightTable(RarityWeightTable.OnlyRare, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot, false);
			base.BossCardFriendWeight = new CardWeightTable(RarityWeightTable.OnlyRare, OwnerWeightTable.PlayerAndFriend, CardTypeWeightTable.CanBeLoot, false);
			base.BossCardNeutralWeight = new CardWeightTable(RarityWeightTable.OnlyRare, OwnerWeightTable.OnlyNeutral, CardTypeWeightTable.CanBeLoot, false);
			base.BossCardWeight = new CardWeightTable(RarityWeightTable.OnlyRare, OwnerWeightTable.Hierarchy, CardTypeWeightTable.CanBeLoot, false);
		}
		public override Type GetAdventure()
		{
			if (base.AdventureHistory.Count == 0 && base.FirstAdventure != null)
			{
				return base.FirstAdventure;
			}
			return base.GetAdventure();
		}
	}
}
