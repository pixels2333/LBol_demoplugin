using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.SaveData;
using LBoL.Presentation.Effect;
using LBoL.Presentation.I10N;
using LBoL.Presentation.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	[DisallowMultipleComponent]
	public sealed class CardWidget : MonoBehaviour, ICardTooltipSource
	{
		private static string GetSimpleIllustrator(string id)
		{
			if (id.EndsWith("(Sketch)"))
			{
				return id.Substring(0, id.Length - 8);
			}
			if (id.EndsWith("(Community)"))
			{
				return id.Substring(0, id.Length - 11);
			}
			return id;
		}
		private string CurrentIllustrator
		{
			get
			{
				return this._currentIllustrator;
			}
			set
			{
				this._currentIllustrator = value;
				this.illustrator.text = CardWidget.GetSimpleIllustrator(value);
			}
		}
		public unsafe static void LoadSprites()
		{
			IntPtr intPtr = stackalloc byte[(UIntPtr)12];
			*intPtr = 0;
			*(intPtr + (IntPtr)sizeof(Rarity)) = 1;
			*(intPtr + (IntPtr)2 * (IntPtr)sizeof(Rarity)) = 2;
			Span<Rarity> span = new Span<Rarity>(intPtr, 3);
			IntPtr intPtr2 = stackalloc byte[(UIntPtr)28];
			*intPtr2 = 0;
			*(intPtr2 + (IntPtr)sizeof(ManaColor)) = 1;
			*(intPtr2 + (IntPtr)2 * (IntPtr)sizeof(ManaColor)) = 2;
			*(intPtr2 + (IntPtr)3 * (IntPtr)sizeof(ManaColor)) = 3;
			*(intPtr2 + (IntPtr)4 * (IntPtr)sizeof(ManaColor)) = 4;
			*(intPtr2 + (IntPtr)5 * (IntPtr)sizeof(ManaColor)) = 5;
			*(intPtr2 + (IntPtr)6 * (IntPtr)sizeof(ManaColor)) = 6;
			Span<ManaColor> span2 = new Span<ManaColor>(intPtr2, 7);
			CardWidget._bgTextureTable = new Dictionary<ValueTuple<ManaColor, Rarity, bool>, Texture2D>();
			CardWidget._rainbowBgTextureTable = new Dictionary<ValueTuple<Rarity, bool>, Texture2D>();
			CardWidget._toolBgTextureTable = new Dictionary<ValueTuple<Rarity, bool>, Texture2D>();
			CardWidget._misfortuneBgTextureTable = new Dictionary<ValueTuple<Rarity, bool>, Texture2D>();
			CardWidget._ownerSpriteTable = new Dictionary<string, Sprite>();
			CardWidget._typeSpriteTable = new Dictionary<CardType, Sprite>();
			Span<Rarity> span3 = span;
			for (int i = 0; i < span3.Length; i++)
			{
				Rarity rarity = *span3[i];
				Span<ManaColor> span4 = span2;
				for (int j = 0; j < span4.Length; j++)
				{
					ManaColor manaColor = *span4[j];
					string text;
					string text2;
					if (manaColor == ManaColor.Colorless)
					{
						text = "Sprite/Card/Bg/" + rarity.ToString() + ManaColor.Any.ToString();
						text2 = "Sprite/Card/Bg/Clone/" + rarity.ToString() + ManaColor.Any.ToString();
					}
					else
					{
						text = "Sprite/Card/Bg/" + rarity.ToString() + manaColor.ToString();
						text2 = "Sprite/Card/Bg/Clone/" + rarity.ToString() + manaColor.ToString();
					}
					Texture2D texture2D = Resources.Load<Texture2D>(text);
					if (texture2D)
					{
						CardWidget._bgTextureTable.Add(new ValueTuple<ManaColor, Rarity, bool>(manaColor, rarity, false), texture2D);
					}
					else
					{
						Debug.LogError("Cannot load card bg sprite '" + text + "'");
					}
					texture2D = Resources.Load<Texture2D>(text2);
					if (texture2D)
					{
						CardWidget._bgTextureTable.Add(new ValueTuple<ManaColor, Rarity, bool>(manaColor, rarity, true), texture2D);
					}
					else
					{
						Debug.LogError("Cannot load card bg sprite '" + text2 + "'");
					}
				}
				string text3 = "Sprite/Card/Bg/" + rarity.ToString() + "Rainbow";
				Texture2D texture2D2 = Resources.Load<Texture2D>(text3);
				if (texture2D2)
				{
					CardWidget._rainbowBgTextureTable.Add(new ValueTuple<Rarity, bool>(rarity, false), texture2D2);
				}
				else
				{
					Debug.LogError("Cannot load card bg sprite '" + text3 + "'");
				}
				text3 = "Sprite/Card/Bg/Clone/" + rarity.ToString() + "Rainbow";
				texture2D2 = Resources.Load<Texture2D>(text3);
				if (texture2D2)
				{
					CardWidget._rainbowBgTextureTable.Add(new ValueTuple<Rarity, bool>(rarity, true), texture2D2);
				}
				else
				{
					Debug.LogError("Cannot load card bg sprite '" + text3 + "'");
				}
				string text4 = "Sprite/Card/Bg/" + rarity.ToString() + "Tool";
				Texture2D texture2D3 = Resources.Load<Texture2D>(text4);
				if (texture2D3)
				{
					CardWidget._toolBgTextureTable.Add(new ValueTuple<Rarity, bool>(rarity, false), texture2D3);
				}
				else
				{
					Debug.LogError("Cannot load card bg sprite '" + text4 + "'");
				}
				text4 = "Sprite/Card/Bg/Clone/" + rarity.ToString() + "Tool";
				texture2D3 = Resources.Load<Texture2D>(text4);
				if (texture2D3)
				{
					CardWidget._toolBgTextureTable.Add(new ValueTuple<Rarity, bool>(rarity, true), texture2D3);
				}
				else
				{
					Debug.LogError("Cannot load card bg sprite '" + text4 + "'");
				}
				Texture2D texture2D4 = Resources.Load<Texture2D>("Sprite/Card/Bg/" + rarity.ToString() + "Misfortune");
				if (texture2D4)
				{
					CardWidget._misfortuneBgTextureTable.Add(new ValueTuple<Rarity, bool>(rarity, false), texture2D4);
				}
				else
				{
					Debug.LogError("Cannot load card bg sprite '" + text4 + "'");
				}
				texture2D4 = Resources.Load<Texture2D>("Sprite/Card/Bg/Clone/" + rarity.ToString() + "Misfortune");
				if (texture2D4)
				{
					CardWidget._misfortuneBgTextureTable.Add(new ValueTuple<Rarity, bool>(rarity, true), texture2D4);
				}
				else
				{
					Debug.LogError("Cannot load card bg sprite '" + text4 + "'");
				}
			}
			foreach (Sprite sprite in Resources.LoadAll<Sprite>("Sprite/Card/Owner/"))
			{
				CardWidget._ownerSpriteTable.Add(sprite.name, sprite);
			}
			IntPtr intPtr3 = stackalloc byte[(UIntPtr)32];
			*intPtr3 = 1;
			*(intPtr3 + (IntPtr)sizeof(CardType)) = 2;
			*(intPtr3 + (IntPtr)2 * (IntPtr)sizeof(CardType)) = 3;
			*(intPtr3 + (IntPtr)3 * (IntPtr)sizeof(CardType)) = 4;
			*(intPtr3 + (IntPtr)4 * (IntPtr)sizeof(CardType)) = 5;
			*(intPtr3 + (IntPtr)5 * (IntPtr)sizeof(CardType)) = 6;
			*(intPtr3 + (IntPtr)6 * (IntPtr)sizeof(CardType)) = 7;
			*(intPtr3 + (IntPtr)7 * (IntPtr)sizeof(CardType)) = 8;
			Span<CardType> span5 = new Span<CardType>(intPtr3, 8);
			for (int i = 0; i < span5.Length; i++)
			{
				CardType cardType = *span5[i];
				string text5 = "Sprite/Card/Type/" + cardType.ToString();
				Sprite sprite2 = Resources.Load<Sprite>(text5);
				if (sprite2)
				{
					CardWidget._typeSpriteTable.Add(cardType, sprite2);
				}
				else
				{
					Debug.LogError("Cannot load card type sprite '" + text5 + "'");
				}
			}
		}
		public void RefreshBgTextureForLoopOrder()
		{
			Card card = this._card;
			if (card != null && card.CardType != CardType.Tool && card.CardType != CardType.Misfortune)
			{
				IReadOnlyCollection<ManaColor> readOnlyCollection;
				if (!card.IsPurified)
				{
					readOnlyCollection = card.Config.Colors;
				}
				else
				{
					IReadOnlyList<ManaColor> readOnlyList = new List<ManaColor>();
					readOnlyCollection = readOnlyList;
				}
				if (readOnlyCollection.Count == 2)
				{
					this.SetBgTexture(this._card);
					this.costWidget.SetCost(this._card, this._card.Zone == CardZone.Hand);
				}
			}
		}
		[return: TupleElementNames(new string[] { "main", "sub" })]
		public static ValueTuple<Texture2D, Texture2D> GetBgTexture(Card card)
		{
			if (card.CardType == CardType.Tool)
			{
				Texture2D texture2D;
				if (CardWidget._toolBgTextureTable.TryGetValue(new ValueTuple<Rarity, bool>(card.Config.Rarity, card.UseTransparentTexture), ref texture2D))
				{
					return new ValueTuple<Texture2D, Texture2D>(texture2D, null);
				}
				Debug.LogError(string.Format("Cannot find BG sprite for '{0}'-Tool", card.Config.Rarity));
				return new ValueTuple<Texture2D, Texture2D>(null, null);
			}
			else if (card.CardType == CardType.Misfortune)
			{
				Texture2D texture2D2;
				if (CardWidget._misfortuneBgTextureTable.TryGetValue(new ValueTuple<Rarity, bool>(card.Config.Rarity, card.UseTransparentTexture), ref texture2D2))
				{
					return new ValueTuple<Texture2D, Texture2D>(texture2D2, null);
				}
				Debug.LogError(string.Format("Cannot find BG sprite for '{0}'-Tool", card.Config.Rarity));
				return new ValueTuple<Texture2D, Texture2D>(null, null);
			}
			else
			{
				Rarity rarity = card.Config.Rarity;
				IReadOnlyList<ManaColor> readOnlyList;
				if (!card.IsPurified)
				{
					readOnlyList = card.Config.Colors;
				}
				else
				{
					IReadOnlyList<ManaColor> readOnlyList2 = new List<ManaColor>();
					readOnlyList = readOnlyList2;
				}
				IReadOnlyList<ManaColor> readOnlyList3 = readOnlyList;
				if (readOnlyList3.Count >= 3)
				{
					Texture2D texture2D3;
					if (CardWidget._rainbowBgTextureTable.TryGetValue(new ValueTuple<Rarity, bool>(card.Config.Rarity, card.UseTransparentTexture), ref texture2D3))
					{
						return new ValueTuple<Texture2D, Texture2D>(texture2D3, null);
					}
					Debug.LogError(string.Format("Cannot find BG texture for '{0}'-Rainbow", card.Config.Rarity));
					return new ValueTuple<Texture2D, Texture2D>(null, null);
				}
				else
				{
					if (readOnlyList3.Count == 2)
					{
						ManaColor manaColor;
						ManaColor manaColor2;
						if (GameMaster.IsLoopOrder)
						{
							ManaColors.GetLoopOrder(readOnlyList3[0], readOnlyList3[1], out manaColor, out manaColor2);
						}
						else
						{
							manaColor = readOnlyList3[0];
							manaColor2 = readOnlyList3[1];
						}
						Texture2D texture2D4;
						if (!CardWidget._bgTextureTable.TryGetValue(new ValueTuple<ManaColor, Rarity, bool>(manaColor, rarity, card.UseTransparentTexture), ref texture2D4))
						{
							Debug.LogError(string.Format("Cannot find BG texture for '{0}-{1}'", manaColor, rarity));
						}
						Texture2D texture2D5;
						if (!CardWidget._bgTextureTable.TryGetValue(new ValueTuple<ManaColor, Rarity, bool>(manaColor2, rarity, card.UseTransparentTexture), ref texture2D5))
						{
							Debug.LogError(string.Format("Cannot find BG texture for '{0}-{1}'", manaColor2, rarity));
						}
						return new ValueTuple<Texture2D, Texture2D>(texture2D4, texture2D5);
					}
					ManaColor manaColor3 = ((readOnlyList3.Count > 0) ? readOnlyList3[0] : ManaColor.Any);
					Texture2D texture2D6;
					if (!CardWidget._bgTextureTable.TryGetValue(new ValueTuple<ManaColor, Rarity, bool>(manaColor3, rarity, card.UseTransparentTexture), ref texture2D6))
					{
						Debug.LogError(string.Format("Cannot find BG texture for '{0}-{1}'", readOnlyList3[0], rarity));
					}
					return new ValueTuple<Texture2D, Texture2D>(texture2D6, null);
				}
			}
		}
		private static Sprite GetOwnerSprite(Card card)
		{
			string owner = card.Config.Owner;
			if (string.IsNullOrWhiteSpace(owner))
			{
				return null;
			}
			Sprite sprite;
			if (CardWidget._ownerSpriteTable.TryGetValue(owner, ref sprite))
			{
				return sprite;
			}
			Debug.LogError("Cannot find owner sprite for '" + owner + "'");
			return null;
		}
		private static Sprite GetTypeSprite(Card card)
		{
			Sprite sprite;
			if (CardWidget._typeSpriteTable.TryGetValue(card.Config.Type, ref sprite))
			{
				return sprite;
			}
			Debug.LogError(string.Format("Cannot find type sprite for '{0}'", card.Config.Type));
			return null;
		}
		public bool NotReveal { get; set; }
		public bool NotReadyInMuseum { get; set; }
		public RectTransform RectTransform
		{
			get
			{
				return this.rectTransform;
			}
		}
		public CanvasGroup CanvasGroup
		{
			get
			{
				return this.canvasGroup;
			}
		}
		public void SetIdVisible(bool visible)
		{
			this.cardIdText.gameObject.SetActive(visible);
		}
		private void Awake()
		{
			this._rawPath = Utils.GetScenePath(base.transform);
			this._awaken = true;
			if (this._lazySetCard)
			{
				this.LazySetCard();
			}
			this.deckIndex.gameObject.SetActive(false);
		}
		private void LateUpdate()
		{
			if (this._changed)
			{
				this._changed = false;
				this.SetProperties();
			}
		}
		private void OnEnable()
		{
			L10nManager.LocaleChanged += new Action(this.OnLocaleChanged);
			GameMaster.SettingsChanged += new Action<GameSettingsSaveData>(this.OnSettingsChanged);
			if (this._card != null)
			{
				this.AddHandlers(this._card);
			}
		}
		private void OnDisable()
		{
			L10nManager.LocaleChanged -= new Action(this.OnLocaleChanged);
			GameMaster.SettingsChanged -= new Action<GameSettingsSaveData>(this.OnSettingsChanged);
			if (this._card != null)
			{
				this.RemoveHandlers(this._card);
			}
			this.HideTooltip();
			DOTween.Kill(this, false);
		}
		private void OnDestroy()
		{
			if (this._card != null)
			{
				this.RemoveHandlers(this._card);
				this._card = null;
			}
		}
		private void AddHandlers(Card card)
		{
			card.PropertyChanged += new Action(this.OnPropertyChanged);
			card.Activating += new Action(this.OnActivating);
		}
		private void RemoveHandlers(Card card)
		{
			card.PropertyChanged -= new Action(this.OnPropertyChanged);
			card.Activating -= new Action(this.OnActivating);
		}
		private void OnLocaleChanged()
		{
			if (this._card != null)
			{
				this._changed = true;
			}
		}
		private void OnSettingsChanged(GameSettingsSaveData settings)
		{
			if (!this)
			{
				Debug.LogError("OnSettingsChanged invoked on destroyed: " + this._rawPath);
				return;
			}
			if (this._card != null)
			{
				if (this.illustrator.gameObject)
				{
					this.illustrator.gameObject.SetActive(settings.ShowIllustrator);
				}
				this.RefreshCardImage();
				return;
			}
			if (this.illustrator.gameObject)
			{
				this.illustrator.gameObject.SetActive(false);
			}
		}
		public Card Card
		{
			get
			{
				return this._card;
			}
			set
			{
				if (this._card != null && base.isActiveAndEnabled)
				{
					this.RemoveHandlers(this._card);
				}
				this._card = value;
				if (value != null)
				{
					if (this._awaken)
					{
						this.LazySetCard();
					}
					else
					{
						this._lazySetCard = true;
					}
					this._changed = true;
					if (base.isActiveAndEnabled)
					{
						this.AddHandlers(value);
					}
				}
			}
		}
		private void LazySetCard()
		{
			if (this._card == null)
			{
				return;
			}
			this.cardIdText.text = this._card.GetType().Name;
			this.cardIdText.gameObject.SetActive(GameMaster.ShowCardId);
			this.otherInfo.text = ((this._card.Config.DebugLevel > 0) ? "Debug" : null);
			this.otherInfo.gameObject.SetActive(this._card.Config.DebugLevel > 0);
			this.RefreshCardImage();
			this.illustrator.gameObject.SetActive(GameMaster.ShowIllustrator);
			this._dontShowMana = this._card.IsForbidden;
			this.costWidget.gameObject.SetActive(!this._dontShowMana);
			this.SetBgTexture(this._card);
			Sprite ownerSprite = CardWidget.GetOwnerSprite(this._card);
			this.cardOwnerIcon.sprite = ownerSprite;
			this.cardOwnerIcon.gameObject.SetActive(ownerSprite);
			Sprite typeSprite = CardWidget.GetTypeSprite(this._card);
			this.cardTypeIcon.sprite = typeSprite;
			this.cardTypeIcon.gameObject.SetActive(typeSprite);
			this.baseLoyaltyObj.SetActive(this._card.CardType == CardType.Friend);
			this.MarginAsFriend = this._card.CardType == CardType.Friend && !this.NotReveal;
			this._shouldChangeImageWhenUpgrade = !this._card.Config.UpgradeImageId.IsNullOrEmpty();
			if (this._shouldChangeImageWhenUpgrade)
			{
				this._changedImageWhenUpgrade = this._card.IsUpgraded;
			}
		}
		public bool MarginAsFriend
		{
			get
			{
				return this._marginAsFriend;
			}
			set
			{
				if (this._marginAsFriend != value)
				{
					this._marginAsFriend = value;
					this.descriptionText.margin = (this._marginAsFriend ? Vector4.zero : new Vector4(77f, 0f, 0f, 0f));
				}
			}
		}
		public void SetCardIllustrator(string id)
		{
			this._tempIllustratorId = id;
			this._changed = true;
		}
		private void RefreshCardImage()
		{
			string text = GameMaster.GetPreferredCardIllustrator(this._card);
			if (this._tempIllustratorId != null)
			{
				text = this._tempIllustratorId;
			}
			if (text != null && text != "")
			{
				this.CurrentIllustrator = text;
				this.unfinished.gameObject.SetActive(false);
			}
			else if (this._card.Config.Illustrator.IsNullOrEmpty())
			{
				this.CurrentIllustrator = "";
				this.unfinished.gameObject.SetActive(false);
			}
			else
			{
				this.CurrentIllustrator = ((!this._card.Config.Illustrator.IsNullOrEmpty()) ? this._card.Config.Illustrator : "");
				this.unfinished.gameObject.SetActive(this._card.Config.Unfinished);
			}
			string text2;
			if (!this._card.Config.UpgradeImageId.IsNullOrEmpty() && this._card.IsUpgraded)
			{
				text2 = this._card.Config.UpgradeImageId;
			}
			else
			{
				text2 = (this._card.Config.ImageId.IsNullOrEmpty() ? this._card.Id : this._card.Config.ImageId);
			}
			Texture texture = ResourcesHelper.TryGetCardImage(text2 + text);
			this.cardImage.texture = (texture ? texture : this.defaultCardImage);
		}
		private void SetBgTexture(Card card)
		{
			ValueTuple<Texture2D, Texture2D> bgTexture = CardWidget.GetBgTexture(card);
			Texture2D item = bgTexture.Item1;
			Texture2D item2 = bgTexture.Item2;
			this.cardMainBg.texture = item;
			this.cardSubBg.texture = item2;
			this.cardSubBg.gameObject.SetActive(item2);
		}
		public bool ShowManaHand
		{
			get
			{
				return this._showManaHand;
			}
			set
			{
				if (this._showManaHand != value)
				{
					this._showManaHand = value;
					RectTransform rectTransform = this.costWidget.rectTransform;
					RectTransform rectTransform2 = (this._showManaHand ? this.costHandRect : this.costRegularRect);
					rectTransform.SetParent(rectTransform2);
					rectTransform.anchoredPosition = Vector2.zero;
					rectTransform.sizeDelta = rectTransform2.sizeDelta;
				}
			}
		}
		public bool CostMoreLeft
		{
			get
			{
				return this._costMoreLeft;
			}
			set
			{
				if (this._costMoreLeft == value)
				{
					return;
				}
				this._costMoreLeft = value;
				this.costHandRect.DOComplete(false);
				this.costHandRect.DOLocalMoveX(value ? (-150f) : 150f, 0.1f, false).SetRelative(true).SetUpdate(true)
					.SetTarget(this.costHandRect);
			}
		}
		public void SetCostToLeftInstant(bool left = true)
		{
			this._costMoreLeft = left;
			this.costHandRect.DOComplete(false);
			if (left)
			{
				this.costHandRect.localPosition += new Vector3(-150f, 0f, 0f);
				return;
			}
			this.costHandRect.localPosition += new Vector3(150f, 0f, 0f);
		}
		public void OnActivating()
		{
			if (!this)
			{
				Debug.Log(string.Concat(new string[]
				{
					"Activating ",
					this._card.DebugName,
					" error: Widget ",
					this._rawPath,
					" already destroyed."
				}));
				return;
			}
			this.PlayActivatingEdge();
			DOTween.Sequence().Append(base.transform.DOScale(1.2f, 0.2f)).AppendInterval(0.3f)
				.Append(base.transform.DOScale(1f, 0.2f))
				.SetUpdate(true)
				.SetLink(base.gameObject);
		}
		private void OnPropertyChanged()
		{
			this._changed = true;
		}
		public void RefreshStatus()
		{
			this._changed = true;
		}
		private void SetProperties()
		{
			if (this._card == null)
			{
				return;
			}
			this.nameText.text = (this._card.IsUpgraded ? ((this._card.UpgradeCounter > 0) ? (this._card.Name + "+" + this._card.UpgradeCounter.Value.ToString()) : (this._card.Name + "+")) : this._card.Name);
			this.typeText.text = ("CardType." + this._card.Config.Type.ToString()).Localize(true);
			this.descriptionText.text = this._card.Description;
			this.nameText.color = (this._card.IsUpgraded ? GlobalConfig.UpgradedGreen : Color.white);
			this.costWidget.SetCost(this._card, this._card.Zone == CardZone.Hand);
			if (this._card.CardType == CardType.Friend)
			{
				this.baseLoyaltyObj.SetActive(!this._card.Summoned);
				if (!this._card.Summoned)
				{
					this.baseLoyalty.text = this._card.Loyalty.ToString();
				}
			}
			if (this.NotReadyInMuseum)
			{
				this.descriptionText.text = "Museum.CardNotReady".Localize(true);
			}
			else if (this.NotReveal)
			{
				this.descriptionText.text = "???";
				this.nameText.text = "Museum.UnRevealCard".Localize(true);
			}
			this.SetBgTexture(this._card);
			if (this._shouldChangeImageWhenUpgrade && this._card.IsUpgraded && !this._changedImageWhenUpgrade)
			{
				this.RefreshCardImage();
				this._changedImageWhenUpgrade = true;
			}
			this.MarginAsFriend = this._card.CardType == CardType.Friend && !this.NotReveal;
			this.descriptionText.ForceMeshUpdate(false, false);
			int lineCount = this.descriptionText.textInfo.lineCount;
			if (this.NotReveal)
			{
				this.descriptionText.alignment = TextAlignmentOptions.Center;
				return;
			}
			this.descriptionText.alignment = ((lineCount > 1 || this._card.CardType == CardType.Friend) ? TextAlignmentOptions.Left : TextAlignmentOptions.Center);
		}
		public CardWidget.EdgeStatus Edge { get; set; }
		public void SetCardEdge(CardWidget.EdgeStatus status)
		{
			this.Edge = status;
			switch (status)
			{
			case CardWidget.EdgeStatus.None:
				this.SetEdgeAfford(false);
				this.SetEdgeAffordKicker(false);
				this.SetEdgeHigh(false);
				this.SetEdgeHighKicker(false);
				return;
			case CardWidget.EdgeStatus.Afford:
				this.SetEdgeAfford(true);
				this.SetEdgeAffordKicker(false);
				this.SetEdgeHigh(false);
				this.SetEdgeHighKicker(false);
				return;
			case CardWidget.EdgeStatus.AffordKicker:
				this.SetEdgeAfford(false);
				this.SetEdgeAffordKicker(true);
				this.SetEdgeHigh(false);
				this.SetEdgeHighKicker(false);
				return;
			case CardWidget.EdgeStatus.High:
				this.SetEdgeAfford(false);
				this.SetEdgeAffordKicker(false);
				this.SetEdgeHigh(true);
				this.SetEdgeHighKicker(false);
				return;
			case CardWidget.EdgeStatus.HighKicker:
				this.SetEdgeAfford(false);
				this.SetEdgeAffordKicker(false);
				this.SetEdgeHigh(false);
				this.SetEdgeHighKicker(true);
				return;
			default:
				throw new ArgumentOutOfRangeException("status", status, null);
			}
		}
		private void SetEdgeAfford(bool b)
		{
			if (b && !this._edgeAfford)
			{
				this._edgeAfford = Object.Instantiate<GameObject>(this.edgeAfford, this.effectFront);
				float num = Random.Range(0f, 1f);
				this._edgeAfford.GetComponent<Image>().color = new Color(num, 0.25f, 1f, 0.33f);
			}
			if (this._edgeAfford)
			{
				this._edgeAfford.SetActive(b);
			}
		}
		private void SetEdgeAffordKicker(bool b)
		{
			if (b && !this._edgeAffordKicker)
			{
				this._edgeAffordKicker = Object.Instantiate<GameObject>(this.edgeAffordKicker, this.effectFront);
				float num = Random.Range(0f, 1f);
				this._edgeAffordKicker.GetComponent<Image>().color = new Color(num, 0.6f, 1f, 0.5f);
			}
			if (this._edgeAffordKicker)
			{
				this._edgeAffordKicker.SetActive(b);
			}
		}
		private void SetEdgeHigh(bool b)
		{
			if (b && !this._edgeHigh)
			{
				this._edgeHigh = Object.Instantiate<GameObject>(this.edgeHigh, this.effectFront);
			}
			if (this._edgeHigh)
			{
				this._edgeHigh.SetActive(b);
			}
		}
		private void SetEdgeHighKicker(bool b)
		{
			if (b && !this._edgeHighKicker)
			{
				this._edgeHighKicker = Object.Instantiate<GameObject>(this.edgeHighKicker, this.effectFront);
			}
			if (this._edgeHighKicker)
			{
				this._edgeHighKicker.SetActive(b);
			}
		}
		private void PlayActivatingEdge()
		{
			if (!this._edgeActivatingOnce)
			{
				this._edgeActivatingOnce = Object.Instantiate<GameObject>(this.edgeActivating, this.effectFront);
			}
			this._edgeActivatingOnce.GetComponentInChildren<ParticleSystem>().Play(true);
		}
		public void PlayFriendEffect()
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.friendEffect, this.effectFront);
			gameObject.GetComponentInChildren<ParticleSystem>().Play(true);
			Object.Destroy(gameObject, 2f);
		}
		public void PlayTransformEffect()
		{
			base.transform.DOLocalRotate(new Vector3(0f, 360f, 0f), 0.5f, RotateMode.FastBeyond360);
			GameObject gameObject = Object.Instantiate<GameObject>(this.transformEffect, this.effectFront);
			gameObject.GetComponentInChildren<ParticleSystem>().Play(true);
			Object.Destroy(gameObject, 3f);
		}
		public void SetEndingNotify()
		{
			if (!this._edgeEndNotify)
			{
				this._edgeEndNotify = Object.Instantiate<GameObject>(this.edgeEndNotify, this.effectBack);
			}
			this._edgeEndNotify.GetComponentInChildren<ParticleSystem>().Stop();
			this._edgeEndNotify.GetComponentInChildren<ParticleSystem>().Play();
		}
		public ExileCoverEffect Exile()
		{
			AudioManager.PlayUi("CardExile", false);
			ExileCoverEffect exileCoverEffect = Object.Instantiate<ExileCoverEffect>(this.cardExile, this.exileLayer);
			exileCoverEffect.CloseObjects.Add(this.root.gameObject);
			exileCoverEffect.RootObject = base.gameObject;
			exileCoverEffect.Exile();
			return exileCoverEffect;
		}
		public RemoveCoverEffect Remove()
		{
			AudioManager.PlayUi("CardExile", false);
			RemoveCoverEffect removeCoverEffect = Object.Instantiate<RemoveCoverEffect>(this.cardRemove, this.exileLayer);
			removeCoverEffect.CloseObjects.Add(this.root.gameObject);
			removeCoverEffect.RootObject = base.gameObject;
			removeCoverEffect.Remove();
			return removeCoverEffect;
		}
		public void Summon()
		{
			this.ShowManaHand = true;
			this.PlayFriendEffect();
			AudioManager.PlayUi("SummonFriend", false);
		}
		public bool ShowSticker
		{
			get
			{
				return this._showSticker;
			}
			set
			{
				this._showSticker = value;
				this.sticker.gameObject.SetActive(value);
			}
		}
		public TooltipPosition[] TooltipPositions { get; set; } = CardWidget.DefaultTooltipPositions;
		private int TooltipId { get; set; }
		public bool TooltipEnabled { get; set; } = true;
		public void ShowTooltip()
		{
			if (this.Card != null && this.TooltipEnabled)
			{
				this.TooltipId = TooltipsLayer.ShowCard(this, false);
			}
		}
		public void HideTooltip()
		{
			if (this.TooltipId != 0)
			{
				TooltipsLayer.Hide(this.TooltipId);
				this.TooltipId = 0;
			}
		}
		public void ShowDeckIndex(int index, bool top = false, bool bottom = false)
		{
			string text = index.ToString();
			if (top)
			{
				if (bottom)
				{
					text += "Game.TopBottomSuffix".Localize(true);
				}
				else
				{
					text += "Game.TopSuffix".Localize(true);
				}
			}
			else if (bottom)
			{
				text += "Game.BottomSuffix".Localize(true);
			}
			this.deckIndex.text = text;
			this.deckIndex.gameObject.SetActive(true);
		}
		public void HideDeckIndex()
		{
			this.deckIndex.gameObject.SetActive(false);
		}
		private static Dictionary<ValueTuple<ManaColor, Rarity, bool>, Texture2D> _bgTextureTable;
		private static Dictionary<ValueTuple<Rarity, bool>, Texture2D> _rainbowBgTextureTable;
		private static Dictionary<ValueTuple<Rarity, bool>, Texture2D> _toolBgTextureTable;
		private static Dictionary<ValueTuple<Rarity, bool>, Texture2D> _misfortuneBgTextureTable;
		private static Dictionary<string, Sprite> _ownerSpriteTable;
		private static Dictionary<CardType, Sprite> _typeSpriteTable;
		private const string BgTexturePath = "Sprite/Card/Bg/";
		private const string BgCloneTexturePath = "Sprite/Card/Bg/Clone/";
		private string _currentIllustrator;
		private bool _awaken;
		private bool _lazySetCard;
		private Card _card;
		private GameObject _cardActive;
		private string _tempIllustratorId;
		private bool _changed;
		private string _rawPath;
		private bool _marginAsFriend;
		private bool _dontShowMana;
		private bool _showManaHand;
		private const float DistanceX = 150f;
		private const float Duration = 0.1f;
		private bool _costMoreLeft;
		private bool _shouldChangeImageWhenUpgrade;
		private bool _changedImageWhenUpgrade;
		private GameObject _edgeAfford;
		private GameObject _edgeAffordKicker;
		private GameObject _edgeHigh;
		private GameObject _edgeHighKicker;
		private GameObject _edgeEndNotify;
		private GameObject _edgeActivatingOnce;
		private GameObject _edgeActivatingLoop;
		private bool _showSticker;
		private static readonly TooltipPosition[] DefaultTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Min)
		};
		[Header("组件引用")]
		[SerializeField]
		private TextMeshProUGUI nameText;
		[SerializeField]
		private TextMeshProUGUI typeText;
		[SerializeField]
		private TextMeshProUGUI descriptionText;
		[SerializeField]
		private CostWidget costWidget;
		[SerializeField]
		private RectTransform costRegularRect;
		[SerializeField]
		private RectTransform costHandRect;
		[SerializeField]
		private RawImage cardImage;
		[SerializeField]
		private RawImage cardMainBg;
		[SerializeField]
		private RawImage cardSubBg;
		[SerializeField]
		private Image cardTypeIcon;
		[SerializeField]
		private Image cardOwnerIcon;
		[SerializeField]
		private Transform effectBack;
		[SerializeField]
		private Transform effectFront;
		[SerializeField]
		private Image cardBack;
		[SerializeField]
		private Transform root;
		[SerializeField]
		private Transform exileLayer;
		[SerializeField]
		private RectTransform rectTransform;
		[SerializeField]
		private CanvasGroup canvasGroup;
		[SerializeField]
		private GameObject baseLoyaltyObj;
		[SerializeField]
		private TextMeshProUGUI baseLoyalty;
		[SerializeField]
		private Image sticker;
		[Header("SubInfos")]
		[SerializeField]
		private TextMeshProUGUI cardIdText;
		[SerializeField]
		private TextMeshProUGUI otherInfo;
		[SerializeField]
		private TextMeshProUGUI unfinished;
		[SerializeField]
		private TextMeshProUGUI illustrator;
		[SerializeField]
		private TextMeshProUGUI deckIndex;
		[Header("资源引用")]
		[SerializeField]
		private GameObject edgeAfford;
		[SerializeField]
		private GameObject edgeAffordKicker;
		[SerializeField]
		private GameObject edgeHigh;
		[SerializeField]
		private GameObject edgeHighKicker;
		[SerializeField]
		private GameObject edgeEndNotify;
		[SerializeField]
		private GameObject edgeActivating;
		[SerializeField]
		private GameObject friendEffect;
		[SerializeField]
		private GameObject transformEffect;
		[SerializeField]
		private Texture defaultCardImage;
		public ExileCoverEffect cardExile;
		public RemoveCoverEffect cardRemove;
		public enum EdgeStatus
		{
			None,
			Afford,
			AffordKicker,
			High,
			HighKicker
		}
	}
}
