using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using LBoL.Base;
using LBoL.Core.Cards;
namespace LBoL.Core
{
	public interface IGameRunVisualTrigger
	{
		void OnGainMaxHp(int deltaMaxHp, bool triggerVisual);
		void OnLoseMaxHp(int deltaMaxHp, bool triggerVisual);
		void OnSetHpAndMaxHp(int hp, int maxHp, bool triggerVisual);
		void OnEnemySetHpAndMaxHp(int index, int hp, int maxHp, bool triggerVisual);
		void OnDamage(DamageInfo damage, bool triggerVisual);
		void OnHeal(int amount, bool triggerVisual, string audioName);
		void OnGainMoney(int value, bool triggerVisual, [MaybeNull] VisualSourceData sourceData);
		void OnConsumeMoney(int value);
		void OnLoseMoney(int value);
		void OnGainPower(int value, bool triggerVisual);
		void OnConsumePower(int value, bool triggerVisual);
		void OnLosePower(int value, bool triggerVisual);
		void OnAddDeckCards(Card[] cards, bool triggerVisual, [MaybeNull] VisualSourceData sourceData);
		void OnRemoveDeckCards(Card[] cards, bool triggerVisual);
		void OnUpgradeDeckCards(Card[] cards, bool triggerVisual);
		IEnumerator OnGainExhibit(Exhibit exhibit, bool triggerVisual, [MaybeNull] VisualSourceData sourceData);
		void OnLoseExhibit(Exhibit exhibit, bool triggerVisual);
		void OnSetBaseMana(ManaGroup mana, bool triggerVisual);
		void OnGainBaseMana(ManaGroup mana, bool triggerVisual);
		void OnLoseBaseMana(ManaGroup mana, bool triggerVisual);
		void CustomVisual(string visualName, bool triggerVisual);
	}
}
