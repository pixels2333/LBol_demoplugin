using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using LBoL.Base;
using LBoL.Core.Cards;

namespace LBoL.Core
{
	// Token: 0x0200004B RID: 75
	public interface IGameRunVisualTrigger
	{
		// Token: 0x0600034E RID: 846
		void OnGainMaxHp(int deltaMaxHp, bool triggerVisual);

		// Token: 0x0600034F RID: 847
		void OnLoseMaxHp(int deltaMaxHp, bool triggerVisual);

		// Token: 0x06000350 RID: 848
		void OnSetHpAndMaxHp(int hp, int maxHp, bool triggerVisual);

		// Token: 0x06000351 RID: 849
		void OnEnemySetHpAndMaxHp(int index, int hp, int maxHp, bool triggerVisual);

		// Token: 0x06000352 RID: 850
		void OnDamage(DamageInfo damage, bool triggerVisual);

		// Token: 0x06000353 RID: 851
		void OnHeal(int amount, bool triggerVisual, string audioName);

		// Token: 0x06000354 RID: 852
		void OnGainMoney(int value, bool triggerVisual, [MaybeNull] VisualSourceData sourceData);

		// Token: 0x06000355 RID: 853
		void OnConsumeMoney(int value);

		// Token: 0x06000356 RID: 854
		void OnLoseMoney(int value);

		// Token: 0x06000357 RID: 855
		void OnGainPower(int value, bool triggerVisual);

		// Token: 0x06000358 RID: 856
		void OnConsumePower(int value, bool triggerVisual);

		// Token: 0x06000359 RID: 857
		void OnLosePower(int value, bool triggerVisual);

		// Token: 0x0600035A RID: 858
		void OnAddDeckCards(Card[] cards, bool triggerVisual, [MaybeNull] VisualSourceData sourceData);

		// Token: 0x0600035B RID: 859
		void OnRemoveDeckCards(Card[] cards, bool triggerVisual);

		// Token: 0x0600035C RID: 860
		void OnUpgradeDeckCards(Card[] cards, bool triggerVisual);

		// Token: 0x0600035D RID: 861
		IEnumerator OnGainExhibit(Exhibit exhibit, bool triggerVisual, [MaybeNull] VisualSourceData sourceData);

		// Token: 0x0600035E RID: 862
		void OnLoseExhibit(Exhibit exhibit, bool triggerVisual);

		// Token: 0x0600035F RID: 863
		void OnSetBaseMana(ManaGroup mana, bool triggerVisual);

		// Token: 0x06000360 RID: 864
		void OnGainBaseMana(ManaGroup mana, bool triggerVisual);

		// Token: 0x06000361 RID: 865
		void OnLoseBaseMana(ManaGroup mana, bool triggerVisual);

		// Token: 0x06000362 RID: 866
		void CustomVisual(string visualName, bool triggerVisual);
	}
}
