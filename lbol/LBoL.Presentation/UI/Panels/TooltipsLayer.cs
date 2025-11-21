using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.Presentation.InputSystemExtend;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000BF RID: 191
	public class TooltipsLayer : UiPanel
	{
		// Token: 0x170001BA RID: 442
		// (get) Token: 0x06000B3F RID: 2879 RVA: 0x0003A548 File Offset: 0x00038748
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Tooltip;
			}
		}

		// Token: 0x06000B40 RID: 2880 RVA: 0x0003A54B File Offset: 0x0003874B
		private void Awake()
		{
			this._rectTransform = base.GetComponent<RectTransform>();
		}

		// Token: 0x06000B41 RID: 2881 RVA: 0x0003A55C File Offset: 0x0003875C
		private static Card GetSingleReferenceCard(IEnumerable<Card> referenceCards, string debugName)
		{
			Card card;
			using (IEnumerator<Card> enumerator = referenceCards.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					card = null;
				}
				else
				{
					card = enumerator.Current;
				}
			}
			return card;
		}

		// Token: 0x06000B42 RID: 2882 RVA: 0x0003A5A0 File Offset: 0x000387A0
		private RectTransform CreateReferenceCardWidget(RectTransform tooltipTrans, Card relativeCard)
		{
			CardWidget cardWidget = Object.Instantiate<CardWidget>(this.cardTemplate, tooltipTrans);
			cardWidget.GetComponentInChildren<GamepadCardCursor>().GetComponent<Selectable>().interactable = false;
			CanvasGroup component = cardWidget.GetComponent<CanvasGroup>();
			component.blocksRaycasts = false;
			component.DOFade(1f, 0.1f).From(0f, true, false).SetDelay(0.2f)
				.SetUpdate(true)
				.SetTarget(cardWidget);
			cardWidget.Card = relativeCard;
			RectTransform rectTransform = cardWidget.RectTransform;
			rectTransform.localScale = new Vector3(0.85f, 0.85f, 1f);
			return rectTransform;
		}

		// Token: 0x06000B43 RID: 2883 RVA: 0x0003A631 File Offset: 0x00038831
		public static int ShowNormal(TooltipSource source)
		{
			return UiManager.GetPanel<TooltipsLayer>().InternalShowNormal(source);
		}

		// Token: 0x06000B44 RID: 2884 RVA: 0x0003A640 File Offset: 0x00038840
		private int InternalShowNormal(TooltipSource source)
		{
			this.HideTooltip(this._currentId);
			TooltipWidget tooltipWidget = Object.Instantiate<TooltipWidget>(this.tooltipTemplate, base.transform);
			tooltipWidget.Set(source);
			GameObject gameObject = tooltipWidget.gameObject;
			TooltipPositioner tooltipPositioner = gameObject.AddComponent<TooltipPositioner>();
			tooltipPositioner.TargetTransform = source.TargetRectTransform;
			tooltipPositioner.TooltipPositions = source.TooltipPositions;
			tooltipPositioner.TooltipSize = tooltipWidget.Size;
			tooltipPositioner.Gap = source.Gap;
			this._currentTooltip.Add(gameObject);
			int num = this._currentId + 1;
			this._currentId = num;
			return num;
		}

		// Token: 0x06000B45 RID: 2885 RVA: 0x0003A6CB File Offset: 0x000388CB
		public static int ShowCard(ICardTooltipSource source, bool showSelfInstead = false)
		{
			return UiManager.GetPanel<TooltipsLayer>().InternalShowCard(source, showSelfInstead);
		}

		// Token: 0x06000B46 RID: 2886 RVA: 0x0003A6DC File Offset: 0x000388DC
		private int InternalShowCard(ICardTooltipSource source, bool showSelfInstead)
		{
			this.HideTooltip(this._currentId);
			Card card = source.Card;
			GameObject gameObject = Utils.CreateGameObject(this._rectTransform, "CardTooltip: " + card.DebugName);
			RectTransform rectTransform = (RectTransform)gameObject.transform;
			this._currentTooltip.Add(gameObject);
			this._currentId++;
			EntityTooltipWidget entityTooltipWidget = Object.Instantiate<EntityTooltipWidget>(this.entityTooltipTemplate, rectTransform);
			entityTooltipWidget.SetCard(card, false);
			Vector2 vector = entityTooltipWidget.RectTransform.sizeDelta;
			if (GameMaster.IsLargeTooltips)
			{
				entityTooltipWidget.RectTransform.localScale = new Vector3(1.5f, 1.5f, 1f);
				vector *= 1.5f;
			}
			float num;
			float num2;
			vector.Deconstruct(out num, out num2);
			float num3 = num;
			float num4 = num2;
			RectTransform rectTransform2 = null;
			if (showSelfInstead)
			{
				rectTransform2 = this.CreateReferenceCardWidget(rectTransform, card);
			}
			else
			{
				Card singleReferenceCard = TooltipsLayer.GetSingleReferenceCard(card.EnumerateRelativeCards(), card.DebugName);
				if (singleReferenceCard != null)
				{
					rectTransform2 = this.CreateReferenceCardWidget(rectTransform, singleReferenceCard);
				}
			}
			Vector2 vector2;
			if (rectTransform2 != null)
			{
				(rectTransform2.sizeDelta * 0.85f).Deconstruct(out num2, out num);
				float num5 = num2;
				float num6 = num;
				float num7 = num3 + 10f + num5;
				float num8 = Math.Max(num4, num6);
				vector2 = new Vector2(num7, num8);
				rectTransform.sizeDelta = new Vector2(num7, num8);
				rectTransform2.localPosition = new Vector3(-(num7 - num5) / 2f, (num8 - num6) / 2f);
				entityTooltipWidget.RectTransform.localPosition = new Vector3((num7 - num3) / 2f, (num8 - num4) / 2f);
			}
			else
			{
				vector2 = new Vector2(num3, num4);
				rectTransform.sizeDelta = vector2;
			}
			TooltipPositioner tooltipPositioner = gameObject.AddComponent<TooltipPositioner>();
			tooltipPositioner.TargetTransform = source.RectTransform;
			tooltipPositioner.TooltipPositions = source.TooltipPositions;
			tooltipPositioner.TooltipSize = vector2;
			tooltipPositioner.Gap = 10f;
			return this._currentId;
		}

		// Token: 0x06000B47 RID: 2887 RVA: 0x0003A8D3 File Offset: 0x00038AD3
		public static int ShowCardMultiple(IMultiCardTooltipSource source)
		{
			return UiManager.GetPanel<TooltipsLayer>().InternalShowCardMultiple(source);
		}

		// Token: 0x06000B48 RID: 2888 RVA: 0x0003A8E0 File Offset: 0x00038AE0
		private int InternalShowCardMultiple(IMultiCardTooltipSource source)
		{
			this.HideTooltip(this._currentId);
			GameObject gameObject = Utils.CreateGameObject(this._rectTransform, "MultiCardTooltip");
			RectTransform rectTransform = (RectTransform)gameObject.transform;
			this._currentTooltip.Add(gameObject);
			this._currentId++;
			Vector2 vector = Vector2.zero;
			foreach (Card card in source.Cards)
			{
				RectTransform rectTransform2 = this.CreateReferenceCardWidget(rectTransform, card);
				float num;
				float num2;
				(rectTransform2.sizeDelta * 0.85f).Deconstruct(out num, out num2);
				float num3 = num;
				float num4 = num2;
				float num5 = 10f + num3;
				float num6 = num4;
				vector = new Vector2(num5, num6);
				rectTransform.sizeDelta = new Vector2(num5, num6);
				rectTransform2.localPosition = new Vector3(-(num5 - num3) / 2f, (num6 - num4) / 2f);
			}
			TooltipPositioner tooltipPositioner = gameObject.AddComponent<TooltipPositioner>();
			tooltipPositioner.TargetTransform = source.RectTransform;
			tooltipPositioner.TooltipPositions = source.TooltipPositions;
			tooltipPositioner.TooltipSize = vector;
			tooltipPositioner.Gap = 10f;
			return this._currentId;
		}

		// Token: 0x06000B49 RID: 2889 RVA: 0x0003AA1C File Offset: 0x00038C1C
		public static int ShowExhibit(ExhibitTooltipSource source)
		{
			return UiManager.GetPanel<TooltipsLayer>().InternalShowExhibit(source);
		}

		// Token: 0x06000B4A RID: 2890 RVA: 0x0003AA2C File Offset: 0x00038C2C
		private int InternalShowExhibit(ExhibitTooltipSource source)
		{
			this.HideTooltip(this._currentId);
			Exhibit exhibit = source.Exhibit;
			GameObject gameObject = Utils.CreateGameObject(base.transform, "ExhibitTooltip: " + exhibit.Name);
			RectTransform rectTransform = (RectTransform)gameObject.transform;
			this._currentTooltip.Add(gameObject);
			this._currentId++;
			EntityTooltipWidget entityTooltipWidget = Object.Instantiate<EntityTooltipWidget>(this.entityTooltipTemplate, rectTransform);
			entityTooltipWidget.SetExhibit(exhibit);
			Vector2 vector = entityTooltipWidget.RectTransform.sizeDelta;
			if (GameMaster.IsLargeTooltips)
			{
				entityTooltipWidget.RectTransform.localScale = new Vector3(1.5f, 1.5f, 1f);
				vector *= 1.5f;
			}
			float num;
			float num2;
			vector.Deconstruct(out num, out num2);
			float num3 = num;
			float num4 = num2;
			Card singleReferenceCard = TooltipsLayer.GetSingleReferenceCard(exhibit.EnumerateRelativeCards(), exhibit.DebugName);
			Vector2 vector2;
			if (singleReferenceCard != null)
			{
				RectTransform rectTransform2 = this.CreateReferenceCardWidget(rectTransform, singleReferenceCard);
				(rectTransform2.sizeDelta * 0.85f).Deconstruct(out num2, out num);
				float num5 = num2;
				float num6 = num;
				float num7 = num3 + 10f + num5;
				float num8 = Math.Max(num4, num6);
				vector2 = new Vector2(num7, num8);
				rectTransform.sizeDelta = new Vector2(num7, num8);
				rectTransform2.localPosition = new Vector3(-(num7 - num5) / 2f, (num8 - num6) / 2f);
				entityTooltipWidget.RectTransform.localPosition = new Vector3((num7 - num3) / 2f, (num8 - num4) / 2f);
			}
			else
			{
				vector2 = new Vector2(num3, num4);
				rectTransform.sizeDelta = vector2;
			}
			TooltipPositioner tooltipPositioner = gameObject.AddComponent<TooltipPositioner>();
			tooltipPositioner.TargetTransform = source.TargetRectTransform;
			tooltipPositioner.TooltipPositions = source.TooltipPositions;
			tooltipPositioner.TooltipSize = vector2;
			tooltipPositioner.Gap = source.Gap;
			return this._currentId;
		}

		// Token: 0x06000B4B RID: 2891 RVA: 0x0003AC02 File Offset: 0x00038E02
		public static int ShowUltimateSkill(UltimateSkillTooltipSource source)
		{
			return UiManager.GetPanel<TooltipsLayer>().InternalShowUltimateSkill(source);
		}

		// Token: 0x06000B4C RID: 2892 RVA: 0x0003AC10 File Offset: 0x00038E10
		private int InternalShowUltimateSkill(UltimateSkillTooltipSource source)
		{
			this.HideTooltip(this._currentId);
			UltimateSkill skill = source.Skill;
			GameObject gameObject = Utils.CreateGameObject(base.transform, "UltimateTooltip: " + source.Skill.Title);
			RectTransform rectTransform = (RectTransform)gameObject.transform;
			this._currentTooltip.Add(gameObject);
			this._currentId++;
			EntityTooltipWidget entityTooltipWidget = Object.Instantiate<EntityTooltipWidget>(this.entityTooltipTemplate, rectTransform);
			entityTooltipWidget.SetUltimateSkill(skill);
			Vector2 vector = entityTooltipWidget.RectTransform.sizeDelta;
			if (GameMaster.IsLargeTooltips)
			{
				entityTooltipWidget.RectTransform.localScale = new Vector3(1.5f, 1.5f, 1f);
				vector *= 1.5f;
			}
			float num;
			float num2;
			vector.Deconstruct(out num, out num2);
			float num3 = num;
			float num4 = num2;
			Card singleReferenceCard = TooltipsLayer.GetSingleReferenceCard(skill.EnumerateRelativeCards(), skill.DebugName);
			Vector2 vector2;
			if (singleReferenceCard != null)
			{
				RectTransform rectTransform2 = this.CreateReferenceCardWidget(rectTransform, singleReferenceCard);
				(rectTransform2.sizeDelta * 0.85f).Deconstruct(out num2, out num);
				float num5 = num2;
				float num6 = num;
				float num7 = num3 + 10f + num5;
				float num8 = Math.Max(num4, num6);
				vector2 = new Vector2(num7, num8);
				rectTransform.sizeDelta = new Vector2(num7, num8);
				rectTransform2.localPosition = new Vector3(-(num7 - num5) / 2f, (num8 - num6) / 2f);
				entityTooltipWidget.RectTransform.localPosition = new Vector3((num7 - num3) / 2f, (num8 - num4) / 2f);
			}
			else
			{
				vector2 = new Vector2(num3, num4);
				rectTransform.sizeDelta = vector2;
			}
			TooltipPositioner tooltipPositioner = gameObject.AddComponent<TooltipPositioner>();
			tooltipPositioner.TargetTransform = source.TargetRectTransform;
			tooltipPositioner.TooltipPositions = source.TooltipPositions;
			tooltipPositioner.TooltipSize = vector2;
			return this._currentId;
		}

		// Token: 0x06000B4D RID: 2893 RVA: 0x0003ADDF File Offset: 0x00038FDF
		public static int ShowStatus(StatusTooltipSource source)
		{
			return UiManager.GetPanel<TooltipsLayer>().InternalShowStatus(source);
		}

		// Token: 0x06000B4E RID: 2894 RVA: 0x0003ADEC File Offset: 0x00038FEC
		private int InternalShowStatus(StatusTooltipSource source)
		{
			this.HideTooltip(this._currentId);
			StatusEffect statusEffect = source.StatusEffect;
			EntityTooltipWidget entityTooltipWidget = Object.Instantiate<EntityTooltipWidget>(this.entityTooltipTemplate, base.transform);
			GameObject gameObject = entityTooltipWidget.gameObject;
			gameObject.name = "StatusEffectTooltip: " + source.StatusEffect.Name;
			this._currentTooltip.Add(gameObject);
			this._currentId++;
			entityTooltipWidget.SetStatusEffect(statusEffect);
			Vector2 vector = entityTooltipWidget.RectTransform.sizeDelta;
			if (GameMaster.IsLargeTooltips)
			{
				Vector3 vector2 = new Vector3(1.5f, 1.5f, 1f);
				entityTooltipWidget.RectTransform.localScale = vector2;
				vector *= vector2;
			}
			TooltipPositioner tooltipPositioner = gameObject.AddComponent<TooltipPositioner>();
			tooltipPositioner.TargetTransform = source.TargetRectTransform;
			tooltipPositioner.TooltipPositions = source.TooltipPositions;
			tooltipPositioner.TooltipSize = vector;
			return this._currentId;
		}

		// Token: 0x06000B4F RID: 2895 RVA: 0x0003AECF File Offset: 0x000390CF
		public static int ShowDoll(DollTooltipSource source)
		{
			return UiManager.GetPanel<TooltipsLayer>().InternalShowDoll(source);
		}

		// Token: 0x06000B50 RID: 2896 RVA: 0x0003AEDC File Offset: 0x000390DC
		private int InternalShowDoll(DollTooltipSource source)
		{
			this.HideTooltip(this._currentId);
			Doll doll = source.Doll;
			EntityTooltipWidget entityTooltipWidget = Object.Instantiate<EntityTooltipWidget>(this.entityTooltipTemplate, base.transform);
			GameObject gameObject = entityTooltipWidget.gameObject;
			gameObject.name = "DollTooltip: " + doll.Name;
			this._currentTooltip.Add(gameObject);
			this._currentId++;
			entityTooltipWidget.SetDoll(doll);
			Vector2 vector = entityTooltipWidget.RectTransform.sizeDelta;
			if (GameMaster.IsLargeTooltips)
			{
				Vector3 vector2 = new Vector3(1.5f, 1.5f, 1f);
				entityTooltipWidget.RectTransform.localScale = vector2;
				vector *= vector2;
			}
			TooltipPositioner tooltipPositioner = gameObject.AddComponent<TooltipPositioner>();
			tooltipPositioner.TargetTransform = source.TargetRectTransform;
			tooltipPositioner.TooltipPositions = source.TooltipPositions;
			tooltipPositioner.TooltipSize = vector;
			return this._currentId;
		}

		// Token: 0x06000B51 RID: 2897 RVA: 0x0003AFBA File Offset: 0x000391BA
		public static int ShowAchievement(AchievementTooltipSource source)
		{
			return UiManager.GetPanel<TooltipsLayer>().InternalShowAchievement(source);
		}

		// Token: 0x06000B52 RID: 2898 RVA: 0x0003AFC8 File Offset: 0x000391C8
		private int InternalShowAchievement(AchievementTooltipSource source)
		{
			this.HideTooltip(this._currentId);
			EntityTooltipWidget entityTooltipWidget = Object.Instantiate<EntityTooltipWidget>(this.entityTooltipTemplate, base.transform);
			GameObject gameObject = entityTooltipWidget.gameObject;
			gameObject.name = "AchievementTooltip: " + source.Id;
			this._currentTooltip.Add(gameObject);
			this._currentId++;
			entityTooltipWidget.SetAchievement(source.Title, source.Description);
			Vector2 vector = entityTooltipWidget.RectTransform.sizeDelta;
			if (GameMaster.IsLargeTooltips)
			{
				Vector3 vector2 = new Vector3(1.5f, 1.5f, 1f);
				entityTooltipWidget.RectTransform.localScale = vector2;
				vector *= vector2;
			}
			TooltipPositioner tooltipPositioner = gameObject.AddComponent<TooltipPositioner>();
			tooltipPositioner.TargetTransform = source.TargetRectTransform;
			tooltipPositioner.TooltipPositions = source.TooltipPositions;
			tooltipPositioner.TooltipSize = vector;
			tooltipPositioner.Gap = source.Gap;
			return this._currentId;
		}

		// Token: 0x06000B53 RID: 2899 RVA: 0x0003B0B4 File Offset: 0x000392B4
		public static void Hide(int id)
		{
			UiManager.GetPanel<TooltipsLayer>().HideTooltip(id);
		}

		// Token: 0x06000B54 RID: 2900 RVA: 0x0003B0C4 File Offset: 0x000392C4
		private void HideTooltip(int id)
		{
			if (id < this._currentId)
			{
				return;
			}
			foreach (GameObject gameObject in this._currentTooltip)
			{
				Object.Destroy(gameObject);
			}
			this._currentTooltip.Clear();
		}

		// Token: 0x06000B55 RID: 2901 RVA: 0x0003B12C File Offset: 0x0003932C
		public void Refresh()
		{
			List<TooltipWidget> list = new List<TooltipWidget>();
			foreach (GameObject gameObject in this._currentTooltip)
			{
				TooltipWidget component = gameObject.GetComponent<TooltipWidget>();
				if (component != null)
				{
					list.Add(component);
				}
			}
			foreach (TooltipWidget tooltipWidget in list)
			{
				if (tooltipWidget.Source != null)
				{
					tooltipWidget.Source.Refresh();
				}
			}
		}

		// Token: 0x040008D9 RID: 2265
		private const float RelativeCardRatio = 0.85f;

		// Token: 0x040008DA RID: 2266
		private const float RelativeCardGap = 10f;

		// Token: 0x040008DB RID: 2267
		[SerializeField]
		private TooltipWidget tooltipTemplate;

		// Token: 0x040008DC RID: 2268
		[SerializeField]
		private EntityTooltipWidget entityTooltipTemplate;

		// Token: 0x040008DD RID: 2269
		[SerializeField]
		private CardWidget cardTemplate;

		// Token: 0x040008DE RID: 2270
		private int _currentId;

		// Token: 0x040008DF RID: 2271
		private readonly List<GameObject> _currentTooltip = new List<GameObject>();

		// Token: 0x040008E0 RID: 2272
		private RectTransform _rectTransform;
	}
}
