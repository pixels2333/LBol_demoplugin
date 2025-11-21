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
	// Token: 0x02000044 RID: 68
	[DisallowMultipleComponent]
	public sealed class CardWidget : MonoBehaviour, ICardTooltipSource
	{
		// Token: 0x0600042F RID: 1071 RVA: 0x00010C3E File Offset: 0x0000EE3E
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

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x06000430 RID: 1072 RVA: 0x00010C7C File Offset: 0x0000EE7C
		// (set) Token: 0x06000431 RID: 1073 RVA: 0x00010C84 File Offset: 0x0000EE84
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

		// Token: 0x06000432 RID: 1074 RVA: 0x00010CA0 File Offset: 0x0000EEA0
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

		// Token: 0x06000433 RID: 1075 RVA: 0x00011208 File Offset: 0x0000F408
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

		// Token: 0x06000434 RID: 1076 RVA: 0x00011280 File Offset: 0x0000F480
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

		// Token: 0x06000435 RID: 1077 RVA: 0x000114E0 File Offset: 0x0000F6E0
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

		// Token: 0x06000436 RID: 1078 RVA: 0x0001152C File Offset: 0x0000F72C
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

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x06000437 RID: 1079 RVA: 0x00011574 File Offset: 0x0000F774
		// (set) Token: 0x06000438 RID: 1080 RVA: 0x0001157C File Offset: 0x0000F77C
		public bool NotReveal { get; set; }

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x06000439 RID: 1081 RVA: 0x00011585 File Offset: 0x0000F785
		// (set) Token: 0x0600043A RID: 1082 RVA: 0x0001158D File Offset: 0x0000F78D
		public bool NotReadyInMuseum { get; set; }

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x0600043B RID: 1083 RVA: 0x00011596 File Offset: 0x0000F796
		public RectTransform RectTransform
		{
			get
			{
				return this.rectTransform;
			}
		}

		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x0600043C RID: 1084 RVA: 0x0001159E File Offset: 0x0000F79E
		public CanvasGroup CanvasGroup
		{
			get
			{
				return this.canvasGroup;
			}
		}

		// Token: 0x0600043D RID: 1085 RVA: 0x000115A6 File Offset: 0x0000F7A6
		public void SetIdVisible(bool visible)
		{
			this.cardIdText.gameObject.SetActive(visible);
		}

		// Token: 0x0600043E RID: 1086 RVA: 0x000115B9 File Offset: 0x0000F7B9
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

		// Token: 0x0600043F RID: 1087 RVA: 0x000115F2 File Offset: 0x0000F7F2
		private void LateUpdate()
		{
			if (this._changed)
			{
				this._changed = false;
				this.SetProperties();
			}
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x00011609 File Offset: 0x0000F809
		private void OnEnable()
		{
			L10nManager.LocaleChanged += new Action(this.OnLocaleChanged);
			GameMaster.SettingsChanged += new Action<GameSettingsSaveData>(this.OnSettingsChanged);
			if (this._card != null)
			{
				this.AddHandlers(this._card);
			}
		}

		// Token: 0x06000441 RID: 1089 RVA: 0x00011644 File Offset: 0x0000F844
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

		// Token: 0x06000442 RID: 1090 RVA: 0x00011695 File Offset: 0x0000F895
		private void OnDestroy()
		{
			if (this._card != null)
			{
				this.RemoveHandlers(this._card);
				this._card = null;
			}
		}

		// Token: 0x06000443 RID: 1091 RVA: 0x000116B2 File Offset: 0x0000F8B2
		private void AddHandlers(Card card)
		{
			card.PropertyChanged += new Action(this.OnPropertyChanged);
			card.Activating += new Action(this.OnActivating);
		}

		// Token: 0x06000444 RID: 1092 RVA: 0x000116D8 File Offset: 0x0000F8D8
		private void RemoveHandlers(Card card)
		{
			card.PropertyChanged -= new Action(this.OnPropertyChanged);
			card.Activating -= new Action(this.OnActivating);
		}

		// Token: 0x06000445 RID: 1093 RVA: 0x000116FE File Offset: 0x0000F8FE
		private void OnLocaleChanged()
		{
			if (this._card != null)
			{
				this._changed = true;
			}
		}

		// Token: 0x06000446 RID: 1094 RVA: 0x00011710 File Offset: 0x0000F910
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

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x06000447 RID: 1095 RVA: 0x00011795 File Offset: 0x0000F995
		// (set) Token: 0x06000448 RID: 1096 RVA: 0x000117A0 File Offset: 0x0000F9A0
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

		// Token: 0x06000449 RID: 1097 RVA: 0x00011800 File Offset: 0x0000FA00
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

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x0600044A RID: 1098 RVA: 0x000119AC File Offset: 0x0000FBAC
		// (set) Token: 0x0600044B RID: 1099 RVA: 0x000119B4 File Offset: 0x0000FBB4
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

		// Token: 0x0600044C RID: 1100 RVA: 0x00011A04 File Offset: 0x0000FC04
		public void SetCardIllustrator(string id)
		{
			this._tempIllustratorId = id;
			this._changed = true;
		}

		// Token: 0x0600044D RID: 1101 RVA: 0x00011A14 File Offset: 0x0000FC14
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

		// Token: 0x0600044E RID: 1102 RVA: 0x00011B88 File Offset: 0x0000FD88
		private void SetBgTexture(Card card)
		{
			ValueTuple<Texture2D, Texture2D> bgTexture = CardWidget.GetBgTexture(card);
			Texture2D item = bgTexture.Item1;
			Texture2D item2 = bgTexture.Item2;
			this.cardMainBg.texture = item;
			this.cardSubBg.texture = item2;
			this.cardSubBg.gameObject.SetActive(item2);
		}

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x0600044F RID: 1103 RVA: 0x00011BD6 File Offset: 0x0000FDD6
		// (set) Token: 0x06000450 RID: 1104 RVA: 0x00011BE0 File Offset: 0x0000FDE0
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

		// Token: 0x170000BA RID: 186
		// (get) Token: 0x06000451 RID: 1105 RVA: 0x00011C3C File Offset: 0x0000FE3C
		// (set) Token: 0x06000452 RID: 1106 RVA: 0x00011C44 File Offset: 0x0000FE44
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

		// Token: 0x06000453 RID: 1107 RVA: 0x00011CA8 File Offset: 0x0000FEA8
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

		// Token: 0x06000454 RID: 1108 RVA: 0x00011D24 File Offset: 0x0000FF24
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

		// Token: 0x06000455 RID: 1109 RVA: 0x00011DD4 File Offset: 0x0000FFD4
		private void OnPropertyChanged()
		{
			this._changed = true;
		}

		// Token: 0x06000456 RID: 1110 RVA: 0x00011DDD File Offset: 0x0000FFDD
		public void RefreshStatus()
		{
			this._changed = true;
		}

		// Token: 0x06000457 RID: 1111 RVA: 0x00011DE8 File Offset: 0x0000FFE8
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

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x06000458 RID: 1112 RVA: 0x00012071 File Offset: 0x00010271
		// (set) Token: 0x06000459 RID: 1113 RVA: 0x00012079 File Offset: 0x00010279
		public CardWidget.EdgeStatus Edge { get; set; }

		// Token: 0x0600045A RID: 1114 RVA: 0x00012084 File Offset: 0x00010284
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

		// Token: 0x0600045B RID: 1115 RVA: 0x0001215C File Offset: 0x0001035C
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

		// Token: 0x0600045C RID: 1116 RVA: 0x000121E0 File Offset: 0x000103E0
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

		// Token: 0x0600045D RID: 1117 RVA: 0x00012264 File Offset: 0x00010464
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

		// Token: 0x0600045E RID: 1118 RVA: 0x000122B4 File Offset: 0x000104B4
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

		// Token: 0x0600045F RID: 1119 RVA: 0x00012301 File Offset: 0x00010501
		private void PlayActivatingEdge()
		{
			if (!this._edgeActivatingOnce)
			{
				this._edgeActivatingOnce = Object.Instantiate<GameObject>(this.edgeActivating, this.effectFront);
			}
			this._edgeActivatingOnce.GetComponentInChildren<ParticleSystem>().Play(true);
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x00012338 File Offset: 0x00010538
		public void PlayFriendEffect()
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.friendEffect, this.effectFront);
			gameObject.GetComponentInChildren<ParticleSystem>().Play(true);
			Object.Destroy(gameObject, 2f);
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x00012364 File Offset: 0x00010564
		public void PlayTransformEffect()
		{
			base.transform.DOLocalRotate(new Vector3(0f, 360f, 0f), 0.5f, RotateMode.FastBeyond360);
			GameObject gameObject = Object.Instantiate<GameObject>(this.transformEffect, this.effectFront);
			gameObject.GetComponentInChildren<ParticleSystem>().Play(true);
			Object.Destroy(gameObject, 3f);
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x000123C0 File Offset: 0x000105C0
		public void SetEndingNotify()
		{
			if (!this._edgeEndNotify)
			{
				this._edgeEndNotify = Object.Instantiate<GameObject>(this.edgeEndNotify, this.effectBack);
			}
			this._edgeEndNotify.GetComponentInChildren<ParticleSystem>().Stop();
			this._edgeEndNotify.GetComponentInChildren<ParticleSystem>().Play();
		}

		// Token: 0x06000463 RID: 1123 RVA: 0x00012414 File Offset: 0x00010614
		public ExileCoverEffect Exile()
		{
			AudioManager.PlayUi("CardExile", false);
			ExileCoverEffect exileCoverEffect = Object.Instantiate<ExileCoverEffect>(this.cardExile, this.exileLayer);
			exileCoverEffect.CloseObjects.Add(this.root.gameObject);
			exileCoverEffect.RootObject = base.gameObject;
			exileCoverEffect.Exile();
			return exileCoverEffect;
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x00012468 File Offset: 0x00010668
		public RemoveCoverEffect Remove()
		{
			AudioManager.PlayUi("CardExile", false);
			RemoveCoverEffect removeCoverEffect = Object.Instantiate<RemoveCoverEffect>(this.cardRemove, this.exileLayer);
			removeCoverEffect.CloseObjects.Add(this.root.gameObject);
			removeCoverEffect.RootObject = base.gameObject;
			removeCoverEffect.Remove();
			return removeCoverEffect;
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x000124B9 File Offset: 0x000106B9
		public void Summon()
		{
			this.ShowManaHand = true;
			this.PlayFriendEffect();
			AudioManager.PlayUi("SummonFriend", false);
		}

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x06000466 RID: 1126 RVA: 0x000124D3 File Offset: 0x000106D3
		// (set) Token: 0x06000467 RID: 1127 RVA: 0x000124DB File Offset: 0x000106DB
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

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x06000468 RID: 1128 RVA: 0x000124F5 File Offset: 0x000106F5
		// (set) Token: 0x06000469 RID: 1129 RVA: 0x000124FD File Offset: 0x000106FD
		public TooltipPosition[] TooltipPositions { get; set; } = CardWidget.DefaultTooltipPositions;

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x0600046A RID: 1130 RVA: 0x00012506 File Offset: 0x00010706
		// (set) Token: 0x0600046B RID: 1131 RVA: 0x0001250E File Offset: 0x0001070E
		private int TooltipId { get; set; }

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x0600046C RID: 1132 RVA: 0x00012517 File Offset: 0x00010717
		// (set) Token: 0x0600046D RID: 1133 RVA: 0x0001251F File Offset: 0x0001071F
		public bool TooltipEnabled { get; set; } = true;

		// Token: 0x0600046E RID: 1134 RVA: 0x00012528 File Offset: 0x00010728
		public void ShowTooltip()
		{
			if (this.Card != null && this.TooltipEnabled)
			{
				this.TooltipId = TooltipsLayer.ShowCard(this, false);
			}
		}

		// Token: 0x0600046F RID: 1135 RVA: 0x00012547 File Offset: 0x00010747
		public void HideTooltip()
		{
			if (this.TooltipId != 0)
			{
				TooltipsLayer.Hide(this.TooltipId);
				this.TooltipId = 0;
			}
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x00012564 File Offset: 0x00010764
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

		// Token: 0x06000471 RID: 1137 RVA: 0x000125D9 File Offset: 0x000107D9
		public void HideDeckIndex()
		{
			this.deckIndex.gameObject.SetActive(false);
		}

		// Token: 0x040001FD RID: 509
		private static Dictionary<ValueTuple<ManaColor, Rarity, bool>, Texture2D> _bgTextureTable;

		// Token: 0x040001FE RID: 510
		private static Dictionary<ValueTuple<Rarity, bool>, Texture2D> _rainbowBgTextureTable;

		// Token: 0x040001FF RID: 511
		private static Dictionary<ValueTuple<Rarity, bool>, Texture2D> _toolBgTextureTable;

		// Token: 0x04000200 RID: 512
		private static Dictionary<ValueTuple<Rarity, bool>, Texture2D> _misfortuneBgTextureTable;

		// Token: 0x04000201 RID: 513
		private static Dictionary<string, Sprite> _ownerSpriteTable;

		// Token: 0x04000202 RID: 514
		private static Dictionary<CardType, Sprite> _typeSpriteTable;

		// Token: 0x04000203 RID: 515
		private const string BgTexturePath = "Sprite/Card/Bg/";

		// Token: 0x04000204 RID: 516
		private const string BgCloneTexturePath = "Sprite/Card/Bg/Clone/";

		// Token: 0x04000205 RID: 517
		private string _currentIllustrator;

		// Token: 0x04000206 RID: 518
		private bool _awaken;

		// Token: 0x04000207 RID: 519
		private bool _lazySetCard;

		// Token: 0x04000208 RID: 520
		private Card _card;

		// Token: 0x04000209 RID: 521
		private GameObject _cardActive;

		// Token: 0x0400020A RID: 522
		private string _tempIllustratorId;

		// Token: 0x0400020B RID: 523
		private bool _changed;

		// Token: 0x0400020E RID: 526
		private string _rawPath;

		// Token: 0x0400020F RID: 527
		private bool _marginAsFriend;

		// Token: 0x04000210 RID: 528
		private bool _dontShowMana;

		// Token: 0x04000211 RID: 529
		private bool _showManaHand;

		// Token: 0x04000212 RID: 530
		private const float DistanceX = 150f;

		// Token: 0x04000213 RID: 531
		private const float Duration = 0.1f;

		// Token: 0x04000214 RID: 532
		private bool _costMoreLeft;

		// Token: 0x04000215 RID: 533
		private bool _shouldChangeImageWhenUpgrade;

		// Token: 0x04000216 RID: 534
		private bool _changedImageWhenUpgrade;

		// Token: 0x04000218 RID: 536
		private GameObject _edgeAfford;

		// Token: 0x04000219 RID: 537
		private GameObject _edgeAffordKicker;

		// Token: 0x0400021A RID: 538
		private GameObject _edgeHigh;

		// Token: 0x0400021B RID: 539
		private GameObject _edgeHighKicker;

		// Token: 0x0400021C RID: 540
		private GameObject _edgeEndNotify;

		// Token: 0x0400021D RID: 541
		private GameObject _edgeActivatingOnce;

		// Token: 0x0400021E RID: 542
		private GameObject _edgeActivatingLoop;

		// Token: 0x0400021F RID: 543
		private bool _showSticker;

		// Token: 0x04000220 RID: 544
		private static readonly TooltipPosition[] DefaultTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Min)
		};

		// Token: 0x04000224 RID: 548
		[Header("组件引用")]
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x04000225 RID: 549
		[SerializeField]
		private TextMeshProUGUI typeText;

		// Token: 0x04000226 RID: 550
		[SerializeField]
		private TextMeshProUGUI descriptionText;

		// Token: 0x04000227 RID: 551
		[SerializeField]
		private CostWidget costWidget;

		// Token: 0x04000228 RID: 552
		[SerializeField]
		private RectTransform costRegularRect;

		// Token: 0x04000229 RID: 553
		[SerializeField]
		private RectTransform costHandRect;

		// Token: 0x0400022A RID: 554
		[SerializeField]
		private RawImage cardImage;

		// Token: 0x0400022B RID: 555
		[SerializeField]
		private RawImage cardMainBg;

		// Token: 0x0400022C RID: 556
		[SerializeField]
		private RawImage cardSubBg;

		// Token: 0x0400022D RID: 557
		[SerializeField]
		private Image cardTypeIcon;

		// Token: 0x0400022E RID: 558
		[SerializeField]
		private Image cardOwnerIcon;

		// Token: 0x0400022F RID: 559
		[SerializeField]
		private Transform effectBack;

		// Token: 0x04000230 RID: 560
		[SerializeField]
		private Transform effectFront;

		// Token: 0x04000231 RID: 561
		[SerializeField]
		private Image cardBack;

		// Token: 0x04000232 RID: 562
		[SerializeField]
		private Transform root;

		// Token: 0x04000233 RID: 563
		[SerializeField]
		private Transform exileLayer;

		// Token: 0x04000234 RID: 564
		[SerializeField]
		private RectTransform rectTransform;

		// Token: 0x04000235 RID: 565
		[SerializeField]
		private CanvasGroup canvasGroup;

		// Token: 0x04000236 RID: 566
		[SerializeField]
		private GameObject baseLoyaltyObj;

		// Token: 0x04000237 RID: 567
		[SerializeField]
		private TextMeshProUGUI baseLoyalty;

		// Token: 0x04000238 RID: 568
		[SerializeField]
		private Image sticker;

		// Token: 0x04000239 RID: 569
		[Header("SubInfos")]
		[SerializeField]
		private TextMeshProUGUI cardIdText;

		// Token: 0x0400023A RID: 570
		[SerializeField]
		private TextMeshProUGUI otherInfo;

		// Token: 0x0400023B RID: 571
		[SerializeField]
		private TextMeshProUGUI unfinished;

		// Token: 0x0400023C RID: 572
		[SerializeField]
		private TextMeshProUGUI illustrator;

		// Token: 0x0400023D RID: 573
		[SerializeField]
		private TextMeshProUGUI deckIndex;

		// Token: 0x0400023E RID: 574
		[Header("资源引用")]
		[SerializeField]
		private GameObject edgeAfford;

		// Token: 0x0400023F RID: 575
		[SerializeField]
		private GameObject edgeAffordKicker;

		// Token: 0x04000240 RID: 576
		[SerializeField]
		private GameObject edgeHigh;

		// Token: 0x04000241 RID: 577
		[SerializeField]
		private GameObject edgeHighKicker;

		// Token: 0x04000242 RID: 578
		[SerializeField]
		private GameObject edgeEndNotify;

		// Token: 0x04000243 RID: 579
		[SerializeField]
		private GameObject edgeActivating;

		// Token: 0x04000244 RID: 580
		[SerializeField]
		private GameObject friendEffect;

		// Token: 0x04000245 RID: 581
		[SerializeField]
		private GameObject transformEffect;

		// Token: 0x04000246 RID: 582
		[SerializeField]
		private Texture defaultCardImage;

		// Token: 0x04000247 RID: 583
		public ExileCoverEffect cardExile;

		// Token: 0x04000248 RID: 584
		public RemoveCoverEffect cardRemove;

		// Token: 0x020001CE RID: 462
		public enum EdgeStatus
		{
			// Token: 0x04000EF7 RID: 3831
			None,
			// Token: 0x04000EF8 RID: 3832
			Afford,
			// Token: 0x04000EF9 RID: 3833
			AffordKicker,
			// Token: 0x04000EFA RID: 3834
			High,
			// Token: 0x04000EFB RID: 3835
			HighKicker
		}
	}
}
