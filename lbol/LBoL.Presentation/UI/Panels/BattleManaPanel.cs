using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x02000088 RID: 136
	public class BattleManaPanel : UiPanel
	{
		// Token: 0x060006C4 RID: 1732 RVA: 0x0001DF5C File Offset: 0x0001C15C
		private static void FillAnyCost(ref int any, ref ManaGroup available, in ManaGroup handRemainCost, out ManaGroup payment)
		{
			payment = ManaGroup.Empty;
			int colorless = available.Colorless;
			if (colorless >= any)
			{
				available.Colorless -= any;
				payment.Colorless += any;
				any = 0;
				return;
			}
			available.Colorless = 0;
			payment.Colorless += colorless;
			any -= colorless;
			List<ManaColor> list = new List<ManaColor>();
			using (IEnumerator<ManaColor> enumerator = ManaColors.WUBRG.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ManaColor manaColor = enumerator.Current;
					ManaGroup manaGroup = handRemainCost;
					if (manaGroup[manaColor] == 0)
					{
						list.Add(manaColor);
					}
				}
				goto IL_0130;
			}
			IL_00A1:
			ManaColor manaColor2 = list.MaxByOrDefault(new Func<ManaColor, int>(available.GetValue));
			int num = ((manaColor2 != ManaColor.Any) ? available[manaColor2] : 0);
			if (num == 0)
			{
				manaColor2 = ManaColors.WUBRG.MaxBy(new Func<ManaColor, int>(available.GetValue));
				num = available[manaColor2];
			}
			if (num == 0)
			{
				goto IL_0138;
			}
			available[manaColor2] = num - 1;
			ManaColor manaColor3 = manaColor2;
			int num2 = payment[manaColor3] + 1;
			payment[manaColor3] = num2;
			any--;
			IL_0130:
			if (any > 0)
			{
				goto IL_00A1;
			}
			IL_0138:
			if (any == 0)
			{
				return;
			}
			int philosophy = available.Philosophy;
			if (philosophy >= any)
			{
				available.Philosophy -= any;
				payment.Philosophy += any;
				any = 0;
				return;
			}
			available.Philosophy = 0;
			payment.Philosophy += philosophy;
			any -= philosophy;
		}

		// Token: 0x060006C5 RID: 1733 RVA: 0x0001E100 File Offset: 0x0001C300
		private static void FillHybridCost(ref int hybrid, List<ManaColor> hybridColors, ref ManaGroup available, in ManaGroup handRemainCost, out ManaGroup payment)
		{
			payment = ManaGroup.Empty;
			List<ManaColor> list = new List<ManaColor>();
			using (List<ManaColor>.Enumerator enumerator = hybridColors.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ManaColor manaColor = enumerator.Current;
					ManaGroup manaGroup = handRemainCost;
					if (manaGroup[manaColor] == 0)
					{
						list.Add(manaColor);
					}
				}
				goto IL_00E4;
			}
			IL_0058:
			ManaColor manaColor2 = list.MaxByOrDefault(new Func<ManaColor, int>(available.GetValue));
			int num = ((manaColor2 != ManaColor.Any) ? available[manaColor2] : 0);
			if (num == 0)
			{
				manaColor2 = hybridColors.MaxBy(new Func<ManaColor, int>(available.GetValue));
				num = available[manaColor2];
			}
			if (num == 0)
			{
				goto IL_00EC;
			}
			available[manaColor2] = num - 1;
			ManaColor manaColor3 = manaColor2;
			int num2 = payment[manaColor3] + 1;
			payment[manaColor3] = num2;
			hybrid--;
			IL_00E4:
			if (hybrid > 0)
			{
				goto IL_0058;
			}
			IL_00EC:
			if (hybrid == 0)
			{
				return;
			}
			int philosophy = available.Philosophy;
			if (philosophy >= hybrid)
			{
				available.Philosophy -= hybrid;
				payment.Philosophy += hybrid;
				hybrid = 0;
				return;
			}
			available.Philosophy = 0;
			payment.Philosophy += philosophy;
			hybrid -= philosophy;
		}

		// Token: 0x060006C6 RID: 1734 RVA: 0x0001E25C File Offset: 0x0001C45C
		internal static bool CalculateAutoPayment(ManaGroup unpooledMana, ManaGroup pooledMana, ManaGroup handRemainCost, ManaGroup cost, ManaGroup? kickerCost, bool preferKicker, [MaybeNull] IXCostFilter xCostFilter, out bool kickerPaying, out ManaGroup unpooledPayment, out ManaGroup pooledPayment)
		{
			BattleManaPanel.<>c__DisplayClass2_0 CS$<>8__locals1 = new BattleManaPanel.<>c__DisplayClass2_0();
			CS$<>8__locals1.pooledMana = pooledMana;
			CS$<>8__locals1.unpooledMana = unpooledMana;
			CS$<>8__locals1.tCost = cost;
			kickerPaying = false;
			if (kickerCost != null)
			{
				if (preferKicker)
				{
					if ((CS$<>8__locals1.unpooledMana + CS$<>8__locals1.pooledMana).CanAfford(kickerCost.Value))
					{
						CS$<>8__locals1.tCost = kickerCost.Value;
						kickerPaying = true;
					}
				}
				else if (CS$<>8__locals1.pooledMana.CanAfford(kickerCost.Value))
				{
					CS$<>8__locals1.tCost = kickerCost.Value;
					kickerPaying = true;
				}
			}
			unpooledPayment = ManaGroup.Empty;
			pooledPayment = ManaGroup.Empty;
			int any = CS$<>8__locals1.tCost.Any;
			int hybrid = CS$<>8__locals1.tCost.Hybrid;
			ManaColor manaColor2;
			foreach (ManaColor manaColor in ManaColors.WUBRGC)
			{
				int num = CS$<>8__locals1.tCost[manaColor];
				if (num != 0)
				{
					int num2 = CS$<>8__locals1.unpooledMana[manaColor] + CS$<>8__locals1.pooledMana[manaColor];
					int num3 = num - num2;
					if (num3 > 0 && CS$<>8__locals1.pooledMana.Philosophy > 0)
					{
						int num4 = Math.Min(num3, CS$<>8__locals1.pooledMana.Philosophy);
						ref ManaGroup ptr = ref CS$<>8__locals1.tCost;
						manaColor2 = manaColor;
						ptr[manaColor2] -= num4;
						BattleManaPanel.<>c__DisplayClass2_0 CS$<>8__locals2 = CS$<>8__locals1;
						CS$<>8__locals2.pooledMana.Philosophy = CS$<>8__locals2.pooledMana.Philosophy - num4;
						pooledPayment.Philosophy += num4;
						num3 -= num4;
					}
					if (num3 > 0 && CS$<>8__locals1.unpooledMana.Philosophy > 0)
					{
						int num5 = Math.Min(num3, CS$<>8__locals1.unpooledMana.Philosophy);
						ref ManaGroup ptr = ref CS$<>8__locals1.tCost;
						manaColor2 = manaColor;
						ptr[manaColor2] -= num5;
						BattleManaPanel.<>c__DisplayClass2_0 CS$<>8__locals3 = CS$<>8__locals1;
						CS$<>8__locals3.unpooledMana.Philosophy = CS$<>8__locals3.unpooledMana.Philosophy - num5;
						unpooledPayment.Philosophy += num5;
						num3 -= num5;
					}
					if (num3 > 0)
					{
						return false;
					}
				}
			}
			using (IEnumerator<ManaColor> enumerator = ManaColors.WUBRGC.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ManaColor manaColor3 = enumerator.Current;
					int num6 = CS$<>8__locals1.tCost[manaColor3];
					int num7 = CS$<>8__locals1.pooledMana[manaColor3];
					if (num6 > 0 && num7 > 0)
					{
						int num8 = Math.Min(num6, num7);
						ref ManaGroup ptr = ref CS$<>8__locals1.tCost;
						manaColor2 = manaColor3;
						ptr[manaColor2] -= num8;
						ptr = ref CS$<>8__locals1.pooledMana;
						manaColor2 = manaColor3;
						ptr[manaColor2] -= num8;
						ptr = ref pooledPayment;
						manaColor2 = manaColor3;
						ptr[manaColor2] += num8;
					}
				}
				goto IL_0381;
			}
			IL_02E1:
			IEnumerable<ManaColor> wubrgc = ManaColors.WUBRGC;
			Func<ManaColor, bool> func;
			if ((func = CS$<>8__locals1.<>9__0) == null)
			{
				func = (CS$<>8__locals1.<>9__0 = (ManaColor c) => CS$<>8__locals1.tCost[c] > 0);
			}
			IEnumerable<ManaColor> enumerable = Enumerable.Where<ManaColor>(wubrgc, func);
			Func<ManaColor, int> func2;
			if ((func2 = CS$<>8__locals1.<>9__1) == null)
			{
				func2 = (CS$<>8__locals1.<>9__1 = (ManaColor c) => CS$<>8__locals1.pooledMana[c] + CS$<>8__locals1.unpooledMana[c]);
			}
			ManaColor manaColor4 = enumerable.MinByOrDefault(func2);
			if (manaColor4 == ManaColor.Any)
			{
				goto IL_0392;
			}
			BattleManaPanel.<>c__DisplayClass2_0 CS$<>8__locals4 = CS$<>8__locals1;
			int num9 = CS$<>8__locals4.pooledMana.Philosophy - 1;
			CS$<>8__locals4.pooledMana.Philosophy = num9;
			num9 = pooledPayment.Philosophy + 1;
			pooledPayment.Philosophy = num9;
			BattleManaPanel.<>c__DisplayClass2_0 CS$<>8__locals5 = CS$<>8__locals1;
			manaColor2 = manaColor4;
			num9 = CS$<>8__locals5.tCost[manaColor2] - 1;
			CS$<>8__locals5.tCost[manaColor2] = num9;
			IL_0381:
			if (CS$<>8__locals1.pooledMana.Philosophy > 0)
			{
				goto IL_02E1;
			}
			IL_0392:
			if (hybrid > 0)
			{
				List<ManaColor> list = ManaGroup.HybridColors[CS$<>8__locals1.tCost.HybridColor];
				ManaGroup manaGroup;
				BattleManaPanel.FillHybridCost(ref hybrid, list, ref CS$<>8__locals1.pooledMana, in handRemainCost, out manaGroup);
				pooledPayment += manaGroup;
			}
			if (any > 0)
			{
				ManaGroup manaGroup2;
				BattleManaPanel.FillAnyCost(ref any, ref CS$<>8__locals1.pooledMana, in handRemainCost, out manaGroup2);
				pooledPayment += manaGroup2;
			}
			foreach (ManaColor manaColor5 in ManaColors.WUBRGC)
			{
				int num10 = CS$<>8__locals1.tCost[manaColor5];
				int num11 = CS$<>8__locals1.unpooledMana[manaColor5];
				if (num10 > 0 && num11 > 0)
				{
					int num12 = Math.Min(num10, num11);
					ref ManaGroup ptr = ref CS$<>8__locals1.tCost;
					manaColor2 = manaColor5;
					ptr[manaColor2] -= num12;
					ptr = ref CS$<>8__locals1.unpooledMana;
					manaColor2 = manaColor5;
					ptr[manaColor2] -= num12;
					ptr = ref unpooledPayment;
					manaColor2 = manaColor5;
					ptr[manaColor2] += num12;
				}
			}
			if (hybrid > 0)
			{
				List<ManaColor> list2 = ManaGroup.HybridColors[CS$<>8__locals1.tCost.HybridColor];
				ManaGroup manaGroup3;
				BattleManaPanel.FillHybridCost(ref hybrid, list2, ref CS$<>8__locals1.unpooledMana, in handRemainCost, out manaGroup3);
				unpooledPayment += manaGroup3;
			}
			if (any > 0)
			{
				ManaGroup manaGroup4;
				BattleManaPanel.FillAnyCost(ref any, ref CS$<>8__locals1.unpooledMana, in handRemainCost, out manaGroup4);
				unpooledPayment += manaGroup4;
			}
			if (xCostFilter != null)
			{
				ManaGroup xcostFromPooled = xCostFilter.GetXCostFromPooled(CS$<>8__locals1.pooledMana);
				CS$<>8__locals1.pooledMana -= xcostFromPooled;
				pooledPayment += xcostFromPooled;
			}
			return any == 0;
		}

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x060006C7 RID: 1735 RVA: 0x0001E82C File Offset: 0x0001CA2C
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Base;
			}
		}

		// Token: 0x17000129 RID: 297
		// (get) Token: 0x060006C8 RID: 1736 RVA: 0x0001E82F File Offset: 0x0001CA2F
		public float TransitionDuration
		{
			get
			{
				return this.transitionDuration;
			}
		}

		// Token: 0x1700012A RID: 298
		// (get) Token: 0x060006C9 RID: 1737 RVA: 0x0001E837 File Offset: 0x0001CA37
		public ManaGroup AvailableMana
		{
			get
			{
				return this._pooledCollection.TotalMana + this._unpooledCollection.TotalMana;
			}
		}

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x060006CA RID: 1738 RVA: 0x0001E854 File Offset: 0x0001CA54
		public ManaGroup PooledMana
		{
			get
			{
				return this._pooledCollection.TotalMana;
			}
		}

		// Token: 0x060006CB RID: 1739 RVA: 0x0001E861 File Offset: 0x0001CA61
		private Vector3 GetTargetCenterWorldPosition(BattleManaWidget widget)
		{
			return this.manaRoot.TransformPoint(widget.TargetPosition + this._widgetSize / 2f);
		}

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x060006CC RID: 1740 RVA: 0x0001E88E File Offset: 0x0001CA8E
		// (set) Token: 0x060006CD RID: 1741 RVA: 0x0001E896 File Offset: 0x0001CA96
		public bool ShowKickerPrefer
		{
			get
			{
				return this._showKickerPrefer;
			}
			set
			{
				if (this._showKickerPrefer == value)
				{
					return;
				}
				this._showKickerPrefer = value;
				this.kickerPreferRoot.SetActive(value);
			}
		}

		// Token: 0x060006CE RID: 1742 RVA: 0x0001E8B8 File Offset: 0x0001CAB8
		public void Awake()
		{
			this._widgetSize = this.battleManaTemplate.GetComponent<RectTransform>().sizeDelta;
			this.InitCollections();
			Dictionary<string, Sprite> dictionary = Enumerable.ToDictionary<Sprite, string>(Resources.LoadAll<Sprite>("Sprite Assets/ManaSprite"), (Sprite sprite) => sprite.name);
			ManaColor[] colors = BattleManaPanel.Colors;
			for (int i = 0; i < colors.Length; i++)
			{
				ManaColor color = colors[i];
				char c = color.ToShortName();
				Sprite unpooled = dictionary[c.ToString() + "M"];
				Sprite unpooledHighlight = dictionary[c.ToString() + "MA"];
				Sprite pooled = dictionary[c.ToString() + "P"];
				Sprite pooledHighlight = dictionary[c.ToString() + "PA"];
				this._manaWidgetPoolTable.Add(color, new ObjectPool<BattleManaWidget>(delegate
				{
					BattleManaWidget battleManaWidget = Object.Instantiate<BattleManaWidget>(this.battleManaTemplate, this.manaRoot);
					battleManaWidget.Init(color, unpooled, unpooledHighlight, pooled, pooledHighlight);
					return battleManaWidget;
				}, delegate(BattleManaWidget widget)
				{
					widget.Parent = this;
					widget.gameObject.SetActive(true);
				}, delegate(BattleManaWidget widget)
				{
					widget.Parent = null;
					widget.ResetStatus();
					widget.gameObject.SetActive(false);
				}, delegate(BattleManaWidget widget)
				{
					Object.Destroy(widget.gameObject);
				}, false, 16, 32));
			}
			this.manaRoot.DestroyChildren();
			this.clearPooledButton.onClick.AddListener(new UnityAction(this.UnpoolAllMana));
			this.pooledGroup.gameObject.SetActive(false);
			this.pooledGroup.alpha = 0f;
			this.kickerPreferRoot.SetActive(false);
			this.kickerPreferSwitch.SetValueWithoutNotifier(this.PreferKicker, true);
			this.kickerPreferSwitch.AddListener(delegate(bool isOn)
			{
				this.PreferKicker = isOn;
			});
			SimpleTooltipSource.CreateWithTooltipKey(this.tooltipZone, "BattleMana").WithPosition(TooltipDirection.Right, TooltipAlignment.Min);
		}

		// Token: 0x060006CF RID: 1743 RVA: 0x0001EACC File Offset: 0x0001CCCC
		private void OnDestroy()
		{
			foreach (KeyValuePair<ManaColor, IObjectPool<BattleManaWidget>> keyValuePair in this._manaWidgetPoolTable)
			{
				keyValuePair.Value.Clear();
			}
		}

		// Token: 0x060006D0 RID: 1744 RVA: 0x0001EB24 File Offset: 0x0001CD24
		private void UnpoolAllMana()
		{
			AudioManager.PlayUi("ManaInactive", false);
			if (this._currentHighlightingCost == null || this._currentHighlightingCost.IsPseudoEmpty)
			{
				List<BattleManaWidget> list = this.CreateManaWidgetList();
				ManaGroup normalMana = this._pooledCollection.NormalMana;
				this._pooledCollection.RemoveNormalMana(normalMana, list);
				this._unpooledCollection.AddNormalMana(normalMana, list, null);
				this.ReleaseManaWidgetList(list);
			}
			else
			{
				Debug.LogWarning(string.Format("[BattleManaPanel] Unpool all mana while highlighting [{0}] is not empty.", this._currentHighlightingCost));
				this._unpooledCollection.ResetAllManas(this._unpooledCollection.TotalMana + this._pooledCollection.TotalMana, true);
				this._pooledCollection.ResetAllManas(ManaGroup.Empty, true);
				this.UpdateHighlightManaForCard();
			}
			this.SetPoolBgActive(false);
		}

		// Token: 0x060006D1 RID: 1745 RVA: 0x0001EBE4 File Offset: 0x0001CDE4
		public bool TryPoolSingle(ManaColor color)
		{
			if (this._unpooledCollection.TotalMana[color] <= 0)
			{
				return false;
			}
			ManaGroup manaGroup = ManaGroup.Single(color);
			this._unpooledCollection.ResetAllManas(this._unpooledCollection.TotalMana - manaGroup, true);
			this._pooledCollection.ResetAllManas(this._pooledCollection.TotalMana + manaGroup, true);
			this.SetPoolBgActive(true);
			this.UpdateHighlightManaForCard();
			return true;
		}

		// Token: 0x060006D2 RID: 1746 RVA: 0x0001EC5C File Offset: 0x0001CE5C
		public void OnBattleManaClicked(BattleManaWidget widget, bool rightClick)
		{
			if (rightClick)
			{
				if (widget.IsPooled)
				{
					if (this._pooledCollection.ContainsNormalWidget(widget))
					{
						int num = this._pooledCollection.NormalMana[widget.ManaColor];
						ManaGroup manaGroup = ManaGroup.FromColor(widget.ManaColor, num);
						List<BattleManaWidget> list = this.CreateManaWidgetList();
						this._pooledCollection.RemoveNormalMana(manaGroup, list);
						this._unpooledCollection.AddNormalMana(manaGroup, list, null);
						this.ReleaseManaWidgetList(list);
					}
				}
				else if (this._unpooledCollection.ContainsNormalWidget(widget))
				{
					int num2 = this._unpooledCollection.NormalMana[widget.ManaColor];
					ManaGroup manaGroup2 = ManaGroup.FromColor(widget.ManaColor, num2);
					List<BattleManaWidget> list2 = this.CreateManaWidgetList();
					this._unpooledCollection.RemoveNormalMana(manaGroup2, list2);
					this._pooledCollection.AddNormalMana(manaGroup2, list2, null);
					this.ReleaseManaWidgetList(list2);
				}
			}
			else if (widget.IsPooled)
			{
				if (this._pooledCollection.ContainsNormalWidget(widget))
				{
					widget = this._pooledCollection.ExtractSingleNormalMana(widget);
					this._unpooledCollection.AddSingleNormalMana(widget);
				}
			}
			else if (this._unpooledCollection.ContainsNormalWidget(widget))
			{
				widget = this._unpooledCollection.ExtractSingleNormalMana(widget);
				this._pooledCollection.AddSingleNormalMana(widget);
			}
			this.SetPoolBgActive(this._pooledCollection.TotalMana.Amount > 0);
		}

		// Token: 0x060006D3 RID: 1747 RVA: 0x0001EDC0 File Offset: 0x0001CFC0
		private void UpdateHighlightManaForCard()
		{
			BattleManaPanel.CardCostSnapshot currentHighlightingCost = this._currentHighlightingCost;
			if (currentHighlightingCost != null)
			{
				ManaGroup handRemainCostExcept = UiManager.GetPanel<PlayBoard>().GetHandRemainCostExcept(currentHighlightingCost.Card);
				if (currentHighlightingCost.IsXCost)
				{
					bool flag;
					ManaGroup manaGroup;
					ManaGroup manaGroup2;
					if (!BattleManaPanel.CalculateAutoPayment(this._unpooledCollection.TotalMana, this._pooledCollection.TotalMana, handRemainCostExcept, currentHighlightingCost.Cost, currentHighlightingCost.KickerTotalCost, this.PreferKicker, currentHighlightingCost.Card, out flag, out manaGroup, out manaGroup2))
					{
						currentHighlightingCost.KickerPaying = flag;
						this._unpooledCollection.ClearHighlight();
						this._pooledCollection.ClearHighlight();
						currentHighlightingCost.Card.PendingManaUsage = new ManaGroup?(ManaGroup.Empty);
						return;
					}
					currentHighlightingCost.KickerPaying = flag;
					this._unpooledCollection.SetHighlightMana(manaGroup);
					this._pooledCollection.SetHighlightMana(manaGroup2);
					currentHighlightingCost.Card.PendingManaUsage = new ManaGroup?(manaGroup + manaGroup2);
				}
				else
				{
					bool flag2;
					ManaGroup manaGroup3;
					ManaGroup manaGroup4;
					if (!BattleManaPanel.CalculateAutoPayment(this._unpooledCollection.TotalMana, this._pooledCollection.TotalMana, handRemainCostExcept, currentHighlightingCost.Cost, currentHighlightingCost.KickerTotalCost, this.PreferKicker, null, out flag2, out manaGroup3, out manaGroup4))
					{
						currentHighlightingCost.KickerPaying = flag2;
						this._unpooledCollection.ClearHighlight();
						this._pooledCollection.ClearHighlight();
						currentHighlightingCost.Card.PendingManaUsage = new ManaGroup?(ManaGroup.Empty);
						return;
					}
					currentHighlightingCost.KickerPaying = flag2;
					this._unpooledCollection.SetHighlightMana(manaGroup3);
					this._pooledCollection.SetHighlightMana(manaGroup4);
					currentHighlightingCost.Card.PendingManaUsage = new ManaGroup?(manaGroup3 + manaGroup4);
				}
			}
			else
			{
				this._unpooledCollection.ClearHighlight();
				this._pooledCollection.ClearHighlight();
			}
			this.UpdateAmountText();
		}

		// Token: 0x060006D4 RID: 1748 RVA: 0x0001EF66 File Offset: 0x0001D166
		public void SetCostHighlightForCard(Card card)
		{
			this._currentHighlightingCost = new BattleManaPanel.CardCostSnapshot(card);
			this.UpdateHighlightManaForCard();
		}

		// Token: 0x060006D5 RID: 1749 RVA: 0x0001EF7A File Offset: 0x0001D17A
		public void ClearHighlightMana()
		{
			this._currentHighlightingCost = null;
			this.UpdateHighlightManaForCard();
		}

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x060006D6 RID: 1750 RVA: 0x0001EF89 File Offset: 0x0001D189
		public ManaGroup HighlightingMana
		{
			get
			{
				return this._unpooledCollection.HighlightMana + this._pooledCollection.HighlightMana;
			}
		}

		// Token: 0x060006D7 RID: 1751 RVA: 0x0001EFA8 File Offset: 0x0001D1A8
		protected override void OnEnterBattle()
		{
			this.ResetAllManas(base.Battle.BattleMana, ManaGroup.Empty, true);
			base.Battle.ActionViewer.Register<ConsumeManaAction>(new BattleActionViewer<ConsumeManaAction>(this.ViewConsumeMana), null);
			base.Battle.ActionViewer.Register<GainManaAction>(new BattleActionViewer<GainManaAction>(this.ViewGainMana), null);
			base.Battle.ActionViewer.Register<LoseManaAction>(new BattleActionViewer<LoseManaAction>(this.ViewLoseMana), null);
			base.Battle.ActionViewer.Register<ConvertManaAction>(new BattleActionViewer<ConvertManaAction>(this.ViewConvertMana), null);
			if (Enumerable.Any<Card>(base.GameRun.BaseDeck, (Card card) => card.HasKicker))
			{
				this.ShowKickerPrefer = true;
			}
			base.Show();
		}

		// Token: 0x060006D8 RID: 1752 RVA: 0x0001F080 File Offset: 0x0001D280
		protected override void OnLeaveBattle()
		{
			this._currentHighlightingCost = null;
			this.ResetAllManas(base.Battle.BattleMana, ManaGroup.Empty, true);
			base.Battle.ActionViewer.Unregister<ConsumeManaAction>(new BattleActionViewer<ConsumeManaAction>(this.ViewConsumeMana));
			base.Battle.ActionViewer.Unregister<GainManaAction>(new BattleActionViewer<GainManaAction>(this.ViewGainMana));
			base.Battle.ActionViewer.Unregister<LoseManaAction>(new BattleActionViewer<LoseManaAction>(this.ViewLoseMana));
			base.Battle.ActionViewer.Unregister<ConvertManaAction>(new BattleActionViewer<ConvertManaAction>(this.ViewConvertMana));
			this.ShowKickerPrefer = false;
			base.Hide();
		}

		// Token: 0x060006D9 RID: 1753 RVA: 0x0001F128 File Offset: 0x0001D328
		public void CheckKickerPrefer(IEnumerable<Card> cards)
		{
			if (this.ShowKickerPrefer || base.Battle == null)
			{
				return;
			}
			if (Enumerable.Any<Card>(cards, (Card card) => card.HasKicker))
			{
				this.ShowKickerPrefer = true;
			}
		}

		// Token: 0x060006DA RID: 1754 RVA: 0x0001F174 File Offset: 0x0001D374
		protected override void OnHided()
		{
			this.ResetAllManas(ManaGroup.Empty, ManaGroup.Empty, false);
		}

		// Token: 0x060006DB RID: 1755 RVA: 0x0001F188 File Offset: 0x0001D388
		private void UpdateAmountText()
		{
			ManaGroup availableMana = this.AvailableMana;
			ManaGroup manaGroup = this._unpooledCollection.HighlightMana + this._pooledCollection.HighlightMana;
			int num;
			Color color2;
			if (!manaGroup.IsEmpty)
			{
				int amount = (availableMana - manaGroup).Amount;
				Color color = Color.yellow;
				num = amount;
				color2 = color;
			}
			else
			{
				int amount2 = availableMana.Amount;
				Color color = Color.white;
				num = amount2;
				color2 = color;
			}
			this.amountText.color = color2;
			this.amountText.text = ((num < 99) ? num.ToString() : "99");
		}

		// Token: 0x060006DC RID: 1756 RVA: 0x0001F220 File Offset: 0x0001D420
		private void SetPoolBgActive(bool active)
		{
			if (this._poolBgActive == active)
			{
				return;
			}
			this._poolBgActive = active;
			this.pooledGroup.DOKill(false);
			if (active)
			{
				this.pooledGroup.gameObject.SetActive(true);
				this.pooledGroup.DOFade(1f, this.transitionDuration).SetUpdate(true);
				return;
			}
			this.pooledGroup.DOFade(0f, this.transitionDuration).SetUpdate(true).OnComplete(delegate
			{
				this.pooledGroup.gameObject.SetActive(false);
			});
		}

		// Token: 0x060006DD RID: 1757 RVA: 0x0001F2AC File Offset: 0x0001D4AC
		private bool TryRemoveWidgetsFromNormal(ManaGroup value, List<BattleManaWidget> result)
		{
			ManaGroup manaGroup = value.Intersect(this._unpooledCollection.NormalMana);
			ManaGroup manaGroup2 = value - manaGroup;
			if (manaGroup2.IsInvalid || !this._unpooledCollection.NormalMana.CanAffordExact(manaGroup) || !this._pooledCollection.NormalMana.CanAffordExact(manaGroup2))
			{
				return false;
			}
			this._unpooledCollection.RemoveNormalMana(manaGroup, result);
			this._pooledCollection.RemoveNormalMana(manaGroup2, result);
			return true;
		}

		// Token: 0x060006DE RID: 1758 RVA: 0x0001F32C File Offset: 0x0001D52C
		private bool RemoveWidgetsByMana(ManaGroup value, out List<BattleManaWidget> result, out Exception exception)
		{
			exception = null;
			result = this.CreateManaWidgetList();
			try
			{
				if (this.TryRemoveWidgetsFromNormal(value, result))
				{
					return true;
				}
				if (this._currentHighlightingCost != null)
				{
					this._unpooledCollection.ClearHighlight();
					this._pooledCollection.ClearHighlight();
					if (this.TryRemoveWidgetsFromNormal(value, result))
					{
						if (this._currentHighlightingCost != null)
						{
							this.UpdateHighlightManaForCard();
						}
						return true;
					}
				}
				if (this._consumingDeque.Count > 0)
				{
					UiManager.GetPanel<PlayBoard>().CancelTargetSelecting(true);
					UiManager.GetPanel<PlayBoard>().RewindRequests();
					if (this.TryRemoveWidgetsFromNormal(value, result))
					{
						return true;
					}
				}
				throw new InvalidOperationException("Cannot afford losing mana");
			}
			catch (Exception ex)
			{
				this.ReleaseManaWidgetList(result);
				exception = ex;
			}
			return false;
		}

		// Token: 0x060006DF RID: 1759 RVA: 0x0001F3F0 File Offset: 0x0001D5F0
		private IEnumerator ViewConsumeMana(ConsumeManaAction action)
		{
			float num = 0f;
			PlayBoard panel = UiManager.GetPanel<PlayBoard>();
			if (this._consumingDeque.Count > 0)
			{
				BattleManaPanel.ConsumingManaWidgets consumingManaWidgets = this._consumingDeque[0];
				this._consumingDeque.RemoveAt(0);
				if (consumingManaWidgets.Value.TotalMana == action.Args.Value)
				{
					using (IEnumerator<BattleManaWidget> enumerator = Enumerable.Concat<BattleManaWidget>(consumingManaWidgets.UnpooledWidgets, consumingManaWidgets.PooledWidgets).GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							BattleManaWidget battleManaWidget = enumerator.Current;
							this.ConsumeAndReleaseManaWidget(battleManaWidget);
							Vector3 targetCenterWorldPosition = this.GetTargetCenterWorldPosition(battleManaWidget);
							Card card = action.Args.ActionSource as Card;
							if (card != null)
							{
								HandCard handCard = panel.CardUi.FindHandWidget(card);
								if (handCard != null && handCard)
								{
									num = Math.Max(panel.PlayManaConsumeEffect(battleManaWidget.ManaColor, targetCenterWorldPosition, new Vector3?(handCard.TargetWorldPosition)), num);
								}
							}
						}
						goto IL_018A;
					}
				}
				Debug.LogError(string.Format("Queued mana ({0}) not match consuming {1}, resetting all.", consumingManaWidgets.Value, action.Args.Value));
				this.ResetAllManas(base.Battle.BattleMana, ManaGroup.Empty, true);
			}
			else
			{
				Debug.LogError("Cannot dequeue consuming mana, resetting all.");
				this.ResetAllManas(base.Battle.BattleMana, ManaGroup.Empty, true);
			}
			IL_018A:
			this.SetPoolBgActive(!this._pooledCollection.TotalMana.IsEmpty);
			if (num > 0f)
			{
				yield return new WaitForSeconds(num);
			}
			yield break;
		}

		// Token: 0x060006E0 RID: 1760 RVA: 0x0001F406 File Offset: 0x0001D606
		private IEnumerator ViewGainMana(GainManaAction action)
		{
			ManaGroup value = action.Args.Value;
			List<BattleManaWidget> list = this.CreateManaWidgetList();
			this._unpooledCollection.AddNormalMana(value, null, list);
			float num = 0f;
			PlayBoard panel = UiManager.GetPanel<PlayBoard>();
			if (action.Args.Cause == ActionCause.TurnStart)
			{
				List<BaseManaWidget> notLockedBaseManaWidgets = UiManager.GetPanel<SystemBoard>().GetNotLockedBaseManaWidgets();
				SortedDictionary<ManaColor, List<BaseManaWidget>> sortedDictionary = new SortedDictionary<ManaColor, List<BaseManaWidget>>();
				List<BaseManaWidget> extraTurnManaWidgets = UiManager.GetPanel<SystemBoard>().GetExtraTurnManaWidgets();
				if (value == base.Battle.TurnMana)
				{
					foreach (ManaColor manaColor in value.EnumerateColors())
					{
						List<BaseManaWidget> list2 = Enumerable.ToList<BaseManaWidget>(notLockedBaseManaWidgets.EnumerateEqualRange(manaColor, (BaseManaWidget w) => w.ManaColor));
						sortedDictionary.Add(manaColor, list2);
					}
				}
				ManaColor[] colors = BattleManaPanel.Colors;
				for (int i = 0; i < colors.Length; i++)
				{
					ManaColor color = colors[i];
					if (value[color] != 0)
					{
						List<BaseManaWidget> list4;
						List<BaseManaWidget> list3 = (sortedDictionary.TryGetValue(color, ref list4) ? list4 : new List<BaseManaWidget>());
						Range range = list.EqualRange(color, (BattleManaWidget w) => w.ManaColor);
						BaseManaWidget baseManaWidget = Enumerable.FirstOrDefault<BaseManaWidget>(extraTurnManaWidgets, (BaseManaWidget widget) => widget.ManaColor == color);
						int num2 = ((baseManaWidget != null) ? baseManaWidget.BattleManaAmount : 0);
						if (range.Count() == 1)
						{
							List<BattleManaWidget> list5 = list;
							BattleManaWidget battleManaWidget = list5[range.Start.GetOffset(list5.Count)];
							foreach (BaseManaWidget baseManaWidget2 in list3)
							{
								num = Math.Max(panel.PlayManaGainEffect(color, new Vector3?(baseManaWidget2.CenterWorldPosition), this.GetTargetCenterWorldPosition(battleManaWidget)), num);
							}
							if (baseManaWidget != null)
							{
								num = Math.Max(panel.PlayManaGainEffect(color, new Vector3?(baseManaWidget.CenterWorldPosition), this.GetTargetCenterWorldPosition(battleManaWidget)), num);
							}
						}
						else if (list3.Count + num2 == range.Count())
						{
							foreach (ValueTuple<BaseManaWidget, int> valueTuple in list3.Zip(range.AsEnumerable()))
							{
								BaseManaWidget item = valueTuple.Item1;
								int item2 = valueTuple.Item2;
								BattleManaWidget battleManaWidget2 = list[item2];
								num = Math.Max(panel.PlayManaGainEffect(color, new Vector3?(item.CenterWorldPosition), this.GetTargetCenterWorldPosition(battleManaWidget2)), num);
							}
							if (baseManaWidget != null)
							{
								for (int j = range.Start.Value + list3.Count; j < range.End.Value; j++)
								{
									num = Math.Max(panel.PlayManaGainEffect(color, new Vector3?(baseManaWidget.CenterWorldPosition), this.GetTargetCenterWorldPosition(list[j])), num);
								}
							}
						}
						else
						{
							Debug.LogWarning(string.Format("[BattleManaPanel] Base widget count for color {0} not equal to Battle widget count {1}", color.ToShortName(), range));
						}
					}
				}
			}
			else
			{
				Vector3? vector = panel.FindActionSourceWorldPosition(action.Source);
				foreach (BattleManaWidget battleManaWidget3 in list)
				{
					num = Math.Max(panel.PlayManaGainEffect(battleManaWidget3.ManaColor, vector, this.GetTargetCenterWorldPosition(battleManaWidget3)), num);
				}
			}
			this.ReleaseManaWidgetList(list);
			this.UpdateHighlightManaForCard();
			this.RefreshCardsEdge();
			this.UpdateAmountText();
			if (num > 0f)
			{
				yield return new WaitForSeconds(num);
			}
			yield break;
		}

		// Token: 0x060006E1 RID: 1761 RVA: 0x0001F41C File Offset: 0x0001D61C
		private IEnumerator ViewLoseMana(LoseManaAction action)
		{
			ManaGroup value = action.Args.Value;
			ManaGroup value2 = action.Args.Value;
			List<BattleManaWidget> result;
			Exception ex;
			if (!this.RemoveWidgetsByMana(value2, out result, out ex))
			{
				this.ClearHighlightMana();
				this.ResetAllManas(base.Battle.BattleMana, ManaGroup.Empty, true);
				yield break;
			}
			PlayBoard panel = UiManager.GetPanel<PlayBoard>();
			panel.ReverifyCard();
			this.RefreshCardsEdge();
			this.UpdateAmountText();
			float maxDuration = 0f;
			if (action.Args.Cause == ActionCause.TurnEnd)
			{
				foreach (BattleManaWidget battleManaWidget in result)
				{
					float num = panel.PlayManaLoseEffect(battleManaWidget.ManaColor, this.GetTargetCenterWorldPosition(battleManaWidget), default(Vector3?));
					maxDuration = Math.Max(num, maxDuration);
					this.DisappearAndReleaseManaWidget(battleManaWidget);
				}
				yield return new WaitForSeconds(0.2f);
			}
			else
			{
				Vector3? vector = panel.FindActionSourceWorldPosition(action.Source);
				foreach (BattleManaWidget battleManaWidget2 in result)
				{
					float num2 = panel.PlayManaLoseEffect(battleManaWidget2.ManaColor, this.GetTargetCenterWorldPosition(battleManaWidget2), vector);
					maxDuration = Math.Max(num2, maxDuration);
					this.DisappearAndReleaseManaWidget(battleManaWidget2);
				}
			}
			this.ReleaseManaWidgetList(result);
			this.SetPoolBgActive(!this._pooledCollection.TotalMana.IsEmpty);
			if (maxDuration > 0f)
			{
				yield return new WaitForSeconds(maxDuration);
			}
			yield break;
		}

		// Token: 0x060006E2 RID: 1762 RVA: 0x0001F432 File Offset: 0x0001D632
		private IEnumerator ViewConvertMana(ConvertManaAction action)
		{
			AudioManager.PlayUi("ManaConvert", false);
			List<BattleManaWidget> list;
			Exception ex;
			if (!this.RemoveWidgetsByMana(action.Args.Input, out list, out ex))
			{
				this.ResetAllManas(action.Battle.BattleMana, ManaGroup.Empty, true);
				this.RefreshCardsEdge();
			}
			else
			{
				ManaGroup output = action.Args.Output;
				List<BattleManaWidget> list2 = this.CreateManaWidgetList();
				this._unpooledCollection.AddNormalMana(output, null, list2);
				this.UpdateHighlightManaForCard();
				PlayBoard panel = UiManager.GetPanel<PlayBoard>();
				panel.ReverifyCard();
				float num = 0f;
				Vector3? vector = panel.FindActionSourceWorldPosition(action.Source);
				foreach (BattleManaWidget battleManaWidget in list)
				{
					num = Math.Max(panel.PlayManaLoseEffect(battleManaWidget.ManaColor, this.GetTargetCenterWorldPosition(battleManaWidget), vector), num);
					this.DisappearAndReleaseManaWidget(battleManaWidget);
				}
				foreach (BattleManaWidget battleManaWidget2 in list2)
				{
					num = Math.Max(panel.PlayManaGainEffect(battleManaWidget2.ManaColor, vector, this.GetTargetCenterWorldPosition(battleManaWidget2)), num);
				}
				this.ReleaseManaWidgetList(list);
				this.ReleaseManaWidgetList(list2);
				this.RefreshCardsEdge();
				yield return new WaitForSeconds(num);
			}
			yield break;
		}

		// Token: 0x060006E3 RID: 1763 RVA: 0x0001F448 File Offset: 0x0001D648
		private void RefreshCardsEdge()
		{
			UiManager.GetPanel<PlayBoard>().CardUi.RefreshAllCardsEdge();
		}

		// Token: 0x060006E4 RID: 1764 RVA: 0x0001F45C File Offset: 0x0001D65C
		public bool TryGetConfirmUseMana(Card card, out bool kicker, out ConsumingMana result)
		{
			if (this._currentHighlightingCost == null)
			{
				Debug.LogWarning("[BattleManaPanel] TryGetConfirmUseMana invoked while current highlighting cost is null");
				kicker = false;
				result = null;
				return false;
			}
			if (card != this._currentHighlightingCost.Card)
			{
				Debug.LogWarning("[BattleManaPanel] Trying to get confirm mana for " + card.DebugName + " while highlighting " + this._currentHighlightingCost.Card.DebugName);
				kicker = false;
				result = null;
				return false;
			}
			if (card.IsXCost && card.HasKicker && this._currentHighlightingCost.Cost.CanAfford(card.KickerTotalCost))
			{
				kicker = true;
				result = new ConsumingMana(this._unpooledCollection.HighlightMana, this._pooledCollection.HighlightMana);
				return true;
			}
			if (card.IsXCost && this._currentHighlightingCost.Cost.CanAfford(card.Cost))
			{
				kicker = false;
				result = new ConsumingMana(this._unpooledCollection.HighlightMana, this._pooledCollection.HighlightMana);
				return true;
			}
			if (card.HasKicker && this._currentHighlightingCost.KickerPaying && card.KickerTotalCost == this._currentHighlightingCost.KickerTotalCost)
			{
				kicker = true;
				result = new ConsumingMana(this._unpooledCollection.HighlightMana, this._pooledCollection.HighlightMana);
				return true;
			}
			if (card.Cost == this._currentHighlightingCost.Cost)
			{
				kicker = false;
				result = new ConsumingMana(this._unpooledCollection.HighlightMana, this._pooledCollection.HighlightMana);
				return true;
			}
			kicker = false;
			result = null;
			return false;
		}

		// Token: 0x060006E5 RID: 1765 RVA: 0x0001F5FC File Offset: 0x0001D7FC
		public void Prepay(ConsumingMana mana)
		{
			ConsumingMana consumingMana = new ConsumingMana(this._unpooledCollection.HighlightMana, this._pooledCollection.HighlightMana);
			if (consumingMana.Unpooled != mana.Unpooled || consumingMana.Pooled != mana.Pooled)
			{
				throw new InvalidOperationException("Cost not match with Highlight.");
			}
			BattleManaPanel.ConsumingManaWidgets consumingManaWidgets = new BattleManaPanel.ConsumingManaWidgets(consumingMana, this._unpooledCollection.Prepay(), this._pooledCollection.Prepay());
			this._consumingDeque.Add(consumingManaWidgets);
			this._currentHighlightingCost = null;
			this.UpdateHighlightManaForCard();
			this.RefreshCardsEdge();
			this.UpdateAmountText();
		}

		// Token: 0x060006E6 RID: 1766 RVA: 0x0001F698 File Offset: 0x0001D898
		private void InternalRefund(BattleManaPanel.ConsumingManaWidgets consuming)
		{
			this._unpooledCollection.AddNormalMana(consuming.Value.Unpooled, consuming.UnpooledWidgets, null);
			this._unpooledCollection.AddNormalMana(consuming.Value.Pooled, consuming.PooledWidgets, null);
			this.ReleaseManaWidgetList(consuming.UnpooledWidgets);
			this.ReleaseManaWidgetList(consuming.PooledWidgets);
			this.UpdateHighlightManaForCard();
		}

		// Token: 0x060006E7 RID: 1767 RVA: 0x0001F700 File Offset: 0x0001D900
		public void RefundFront(ManaGroup? checkMana = null)
		{
			Debug.Log(string.Format("RefundFront({0}), queue = [{1}]", checkMana, string.Join<BattleManaPanel.ConsumingManaWidgets>(", ", this._consumingDeque)));
			if (this._consumingDeque.Count > 0)
			{
				BattleManaPanel.ConsumingManaWidgets consumingManaWidgets = this._consumingDeque[0];
				if (checkMana != null)
				{
					ManaGroup valueOrDefault = checkMana.GetValueOrDefault();
					if (valueOrDefault != consumingManaWidgets.Value.TotalMana)
					{
						Debug.LogError(string.Format("Refunding front mana {0} not equal to queue front {1}", valueOrDefault, consumingManaWidgets.Value));
						this.ResetAllManas(base.Battle.BattleMana, ManaGroup.Empty, true);
						goto IL_00CE;
					}
				}
				this._consumingDeque.RemoveAt(0);
				this.InternalRefund(consumingManaWidgets);
			}
			else
			{
				Debug.LogError("Refund mana while consuming queue is empty");
				this.ResetAllManas(base.Battle.BattleMana, ManaGroup.Empty, true);
			}
			IL_00CE:
			this.RefreshCardsEdge();
		}

		// Token: 0x060006E8 RID: 1768 RVA: 0x0001F7E4 File Offset: 0x0001D9E4
		public void RefundBack(ConsumingMana consumingMana)
		{
			Debug.Log(string.Format("RefundBack({0}), queue = [{1}]", consumingMana, string.Join<BattleManaPanel.ConsumingManaWidgets>(", ", this._consumingDeque)));
			if (this._consumingDeque.Count > 0)
			{
				List<BattleManaPanel.ConsumingManaWidgets> consumingDeque = this._consumingDeque;
				BattleManaPanel.ConsumingManaWidgets consumingManaWidgets = consumingDeque[consumingDeque.Count - 1];
				if (consumingMana.Unpooled != consumingManaWidgets.Value.Unpooled || consumingMana.Pooled != consumingManaWidgets.Value.Pooled)
				{
					Debug.LogError(string.Format("Refunding back mana {0} not equal to queue back {1}", consumingMana, consumingManaWidgets.Value));
					this.ResetAllManas(base.Battle.BattleMana, ManaGroup.Empty, true);
				}
				else
				{
					this._consumingDeque.RemoveAt(this._consumingDeque.Count - 1);
					this.InternalRefund(consumingManaWidgets);
				}
			}
			else
			{
				Debug.LogWarning("Refund mana while consuming queue is empty");
				this.ResetAllManas(base.Battle.BattleMana, ManaGroup.Empty, true);
			}
			this.RefreshCardsEdge();
		}

		// Token: 0x1700012E RID: 302
		// (get) Token: 0x060006E9 RID: 1769 RVA: 0x0001F8DD File Offset: 0x0001DADD
		// (set) Token: 0x060006EA RID: 1770 RVA: 0x0001F8E5 File Offset: 0x0001DAE5
		public bool PreferKicker { get; set; }

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x060006EB RID: 1771 RVA: 0x0001F8F0 File Offset: 0x0001DAF0
		public bool CurrentKickerHighlighting
		{
			get
			{
				BattleManaPanel.CardCostSnapshot currentHighlightingCost = this._currentHighlightingCost;
				return currentHighlightingCost != null && currentHighlightingCost.KickerPaying;
			}
		}

		// Token: 0x060006EC RID: 1772 RVA: 0x0001F910 File Offset: 0x0001DB10
		private void InitCollections()
		{
			this._unpooledCollection = new BattleManaPanel.ManaCollection(this, false, new Vector2(this.unpooledX, this.unpooledY), new Vector2(0f, this.unpooledDeltaY), new Vector2(0f, this.highlightDeltaY), new Vector2(0f, this.pendingUseDeltaY));
			this._pooledCollection = new BattleManaPanel.ManaCollection(this, true, new Vector2(this.pooledX, this.pooledY), new Vector2(0f, this.pooledDeltaY), new Vector2(0f, this.highlightDeltaY), new Vector2(0f, this.pendingUseDeltaY));
		}

		// Token: 0x060006ED RID: 1773 RVA: 0x0001F9B9 File Offset: 0x0001DBB9
		private BattleManaWidget CreateManaWidget(ManaColor color, bool pooled, int amount, BattleManaStatus status, Vector2 fromPosition, Vector2? toPosition = null)
		{
			BattleManaWidget battleManaWidget = this.CreateInactiveManaWidget(color, pooled, amount);
			battleManaWidget.Appear(status, fromPosition, toPosition);
			return battleManaWidget;
		}

		// Token: 0x060006EE RID: 1774 RVA: 0x0001F9D0 File Offset: 0x0001DBD0
		private BattleManaWidget CreateInactiveManaWidget(ManaColor color, bool pooled, int amount)
		{
			int num = this._currentManaIndex + 1;
			this._currentManaIndex = num;
			int num2 = num;
			BattleManaWidget battleManaWidget = this._manaWidgetPoolTable[color].Get();
			battleManaWidget.gameObject.name = ((amount > 1) ? string.Format("Mana [{0}]: {1} (x{2})", num2, color.ToShortName(), amount) : string.Format("Mana [{0}]: {1}", num2, color.ToShortName()));
			battleManaWidget.Reinit(pooled);
			battleManaWidget.Amount = amount;
			battleManaWidget.Index = num2;
			return battleManaWidget;
		}

		// Token: 0x060006EF RID: 1775 RVA: 0x0001FA62 File Offset: 0x0001DC62
		private void ReleaseManaWidget(BattleManaWidget widget)
		{
			this._manaWidgetPoolTable[widget.ManaColor].Release(widget);
		}

		// Token: 0x060006F0 RID: 1776 RVA: 0x0001FA7C File Offset: 0x0001DC7C
		private void DisappearAndReleaseManaWidget(BattleManaWidget widget)
		{
			widget.Disappear(delegate
			{
				this.ReleaseManaWidget(widget);
			});
		}

		// Token: 0x060006F1 RID: 1777 RVA: 0x0001FAB4 File Offset: 0x0001DCB4
		private void DisappearAndReleaseManaWidget(BattleManaWidget widget, Vector2 toPosition)
		{
			widget.Disappear(toPosition, delegate
			{
				this.ReleaseManaWidget(widget);
			});
		}

		// Token: 0x060006F2 RID: 1778 RVA: 0x0001FAF0 File Offset: 0x0001DCF0
		private void DisappearAndReleaseManaWidget(BattleManaWidget widget, Vector2 toPosition, bool pooled)
		{
			widget.Disappear(toPosition, pooled, delegate
			{
				this.ReleaseManaWidget(widget);
			});
		}

		// Token: 0x060006F3 RID: 1779 RVA: 0x0001FB2C File Offset: 0x0001DD2C
		private void ConsumeAndReleaseManaWidget(BattleManaWidget widget)
		{
			widget.Consume(delegate
			{
				this.ReleaseManaWidget(widget);
			});
		}

		// Token: 0x060006F4 RID: 1780 RVA: 0x0001FB64 File Offset: 0x0001DD64
		private List<BattleManaWidget> CreateManaWidgetList()
		{
			return this._manaListPool.Get();
		}

		// Token: 0x060006F5 RID: 1781 RVA: 0x0001FB71 File Offset: 0x0001DD71
		private void ReleaseManaWidgetList(List<BattleManaWidget> list)
		{
			this._manaListPool.Release(list);
		}

		// Token: 0x060006F6 RID: 1782 RVA: 0x0001FB80 File Offset: 0x0001DD80
		private void ClearWidgetList(List<BattleManaWidget> list, bool transition)
		{
			foreach (BattleManaWidget battleManaWidget in list)
			{
				if (transition)
				{
					this.DisappearAndReleaseManaWidget(battleManaWidget);
				}
				else
				{
					this.ReleaseManaWidget(battleManaWidget);
				}
			}
			list.Clear();
		}

		// Token: 0x060006F7 RID: 1783 RVA: 0x0001FBE0 File Offset: 0x0001DDE0
		private void ResetAllManas(ManaGroup unpooled, ManaGroup pooled, bool transition = true)
		{
			this._unpooledCollection.ResetAllManas(unpooled, transition);
			this._pooledCollection.ResetAllManas(pooled, transition);
			foreach (BattleManaPanel.ConsumingManaWidgets consumingManaWidgets in this._consumingDeque)
			{
				foreach (BattleManaWidget battleManaWidget in consumingManaWidgets.PooledWidgets)
				{
					this.DisappearAndReleaseManaWidget(battleManaWidget);
				}
				foreach (BattleManaWidget battleManaWidget2 in consumingManaWidgets.UnpooledWidgets)
				{
					this.DisappearAndReleaseManaWidget(battleManaWidget2);
				}
				this.ReleaseManaWidgetList(consumingManaWidgets.PooledWidgets);
				this.ReleaseManaWidgetList(consumingManaWidgets.UnpooledWidgets);
			}
			this._consumingDeque.Clear();
			this.UpdateHighlightManaForCard();
			this.UpdateAmountText();
			this.SetPoolBgActive(!this._pooledCollection.TotalMana.IsEmpty);
		}

		// Token: 0x04000460 RID: 1120
		[SerializeField]
		private BattleManaWidget battleManaTemplate;

		// Token: 0x04000461 RID: 1121
		[SerializeField]
		private Transform manaRoot;

		// Token: 0x04000462 RID: 1122
		[SerializeField]
		private TextMeshProUGUI amountText;

		// Token: 0x04000463 RID: 1123
		[SerializeField]
		private CanvasGroup pooledGroup;

		// Token: 0x04000464 RID: 1124
		[SerializeField]
		private Button clearPooledButton;

		// Token: 0x04000465 RID: 1125
		[SerializeField]
		private GameObject tooltipZone;

		// Token: 0x04000466 RID: 1126
		[SerializeField]
		private GameObject kickerPreferRoot;

		// Token: 0x04000467 RID: 1127
		[SerializeField]
		private SwitchWidget kickerPreferSwitch;

		// Token: 0x04000468 RID: 1128
		[Header("UI adjustment")]
		[SerializeField]
		private float unpooledX;

		// Token: 0x04000469 RID: 1129
		[SerializeField]
		private float unpooledY;

		// Token: 0x0400046A RID: 1130
		[SerializeField]
		private float unpooledDeltaY = 110f;

		// Token: 0x0400046B RID: 1131
		[SerializeField]
		private float pooledX = 126f;

		// Token: 0x0400046C RID: 1132
		[SerializeField]
		private float pooledY;

		// Token: 0x0400046D RID: 1133
		[SerializeField]
		private float pooledDeltaY = 100f;

		// Token: 0x0400046E RID: 1134
		[SerializeField]
		private float highlightDeltaY = 50f;

		// Token: 0x0400046F RID: 1135
		[SerializeField]
		private float pendingUseDeltaY = 50f;

		// Token: 0x04000470 RID: 1136
		[SerializeField]
		private float transitionDuration = 0.2f;

		// Token: 0x04000471 RID: 1137
		private static readonly ManaColor[] Colors = Enumerable.ToArray<ManaColor>(ManaColors.WUBRGCP);

		// Token: 0x04000472 RID: 1138
		private const int FoldingThreshold = 10;

		// Token: 0x04000473 RID: 1139
		private RectTransform _rect;

		// Token: 0x04000474 RID: 1140
		private readonly List<BattleManaPanel.ConsumingManaWidgets> _consumingDeque = new List<BattleManaPanel.ConsumingManaWidgets>();

		// Token: 0x04000475 RID: 1141
		private readonly Dictionary<ManaColor, IObjectPool<BattleManaWidget>> _manaWidgetPoolTable = new Dictionary<ManaColor, IObjectPool<BattleManaWidget>>();

		// Token: 0x04000476 RID: 1142
		private readonly IObjectPool<List<BattleManaWidget>> _manaListPool = new ObjectPool<List<BattleManaWidget>>(() => new List<BattleManaWidget>(), null, delegate(List<BattleManaWidget> list)
		{
			list.Clear();
		}, null, false, 16, 10000);

		// Token: 0x04000477 RID: 1143
		private Vector3 _widgetSize;

		// Token: 0x04000478 RID: 1144
		private BattleManaPanel.CardCostSnapshot _currentHighlightingCost;

		// Token: 0x04000479 RID: 1145
		private bool _showKickerPrefer;

		// Token: 0x0400047A RID: 1146
		private bool _poolBgActive;

		// Token: 0x0400047C RID: 1148
		private BattleManaPanel.ManaCollection _unpooledCollection;

		// Token: 0x0400047D RID: 1149
		private BattleManaPanel.ManaCollection _pooledCollection;

		// Token: 0x0400047E RID: 1150
		private int _currentManaIndex;

		// Token: 0x020001F4 RID: 500
		private sealed class ConsumingManaWidgets
		{
			// Token: 0x170003E9 RID: 1001
			// (get) Token: 0x0600139A RID: 5018 RVA: 0x0005A596 File Offset: 0x00058796
			public ConsumingMana Value { get; }

			// Token: 0x170003EA RID: 1002
			// (get) Token: 0x0600139B RID: 5019 RVA: 0x0005A59E File Offset: 0x0005879E
			public List<BattleManaWidget> UnpooledWidgets { get; }

			// Token: 0x170003EB RID: 1003
			// (get) Token: 0x0600139C RID: 5020 RVA: 0x0005A5A6 File Offset: 0x000587A6
			public List<BattleManaWidget> PooledWidgets { get; }

			// Token: 0x0600139D RID: 5021 RVA: 0x0005A5B0 File Offset: 0x000587B0
			public ConsumingManaWidgets(ConsumingMana value, List<BattleManaWidget> unpooled, List<BattleManaWidget> pooled)
			{
				this.Value = value;
				this.UnpooledWidgets = unpooled;
				this.PooledWidgets = pooled;
			}

			// Token: 0x0600139E RID: 5022 RVA: 0x0005A5E0 File Offset: 0x000587E0
			public override string ToString()
			{
				ManaGroup manaGroup = BattleManaPanel.ConsumingManaWidgets.<ToString>g__MergeWidgetsMana|10_0(this.UnpooledWidgets);
				ManaGroup manaGroup2 = BattleManaPanel.ConsumingManaWidgets.<ToString>g__MergeWidgetsMana|10_0(this.PooledWidgets);
				ManaGroup manaGroup3 = manaGroup;
				ManaGroup manaGroup4 = manaGroup2;
				if (manaGroup3 != this.Value.Unpooled || manaGroup4 != this.Value.Pooled)
				{
					return string.Format("{0}(actually {1}+{2})", this.Value, manaGroup3, manaGroup4);
				}
				return this.Value.ToString();
			}

			// Token: 0x0600139F RID: 5023 RVA: 0x0005A658 File Offset: 0x00058858
			[CompilerGenerated]
			internal static ManaGroup <ToString>g__MergeWidgetsMana|10_0(IEnumerable<BattleManaWidget> widgets)
			{
				ManaGroup empty = ManaGroup.Empty;
				foreach (BattleManaWidget battleManaWidget in widgets)
				{
					ref ManaGroup ptr = ref empty;
					ManaColor manaColor = battleManaWidget.ManaColor;
					ptr[manaColor] += battleManaWidget.Amount;
				}
				return empty;
			}
		}

		// Token: 0x020001F5 RID: 501
		private sealed class CardCostSnapshot
		{
			// Token: 0x170003EC RID: 1004
			// (get) Token: 0x060013A0 RID: 5024 RVA: 0x0005A6C4 File Offset: 0x000588C4
			public Card Card { get; }

			// Token: 0x170003ED RID: 1005
			// (get) Token: 0x060013A1 RID: 5025 RVA: 0x0005A6CC File Offset: 0x000588CC
			public bool IsXCost { get; }

			// Token: 0x170003EE RID: 1006
			// (get) Token: 0x060013A2 RID: 5026 RVA: 0x0005A6D4 File Offset: 0x000588D4
			public ManaGroup Cost { get; }

			// Token: 0x170003EF RID: 1007
			// (get) Token: 0x060013A3 RID: 5027 RVA: 0x0005A6DC File Offset: 0x000588DC
			public ManaGroup? KickerTotalCost { get; }

			// Token: 0x170003F0 RID: 1008
			// (get) Token: 0x060013A4 RID: 5028 RVA: 0x0005A6E4 File Offset: 0x000588E4
			// (set) Token: 0x060013A5 RID: 5029 RVA: 0x0005A6EC File Offset: 0x000588EC
			public bool KickerPaying { get; set; }

			// Token: 0x060013A6 RID: 5030 RVA: 0x0005A6F8 File Offset: 0x000588F8
			public CardCostSnapshot(Card card)
			{
				this.Card = card;
				if (card.IsXCost)
				{
					this.IsXCost = true;
					this.Cost = card.XCostRequiredMana;
				}
				else
				{
					this.Cost = card.Cost;
				}
				if (card.HasKicker)
				{
					this.KickerTotalCost = new ManaGroup?(card.KickerTotalCost);
					return;
				}
				this.KickerTotalCost = default(ManaGroup?);
			}

			// Token: 0x170003F1 RID: 1009
			// (get) Token: 0x060013A7 RID: 5031 RVA: 0x0005A764 File Offset: 0x00058964
			public bool IsPseudoEmpty
			{
				get
				{
					return !this.IsXCost && this.Cost.IsEmpty;
				}
			}

			// Token: 0x060013A8 RID: 5032 RVA: 0x0005A78C File Offset: 0x0005898C
			public override string ToString()
			{
				if (!this.IsXCost)
				{
					return "CardCost: " + this.Cost.ToString();
				}
				return "CardCost: X";
			}
		}

		// Token: 0x020001F6 RID: 502
		private sealed class ManaCollection
		{
			// Token: 0x060013A9 RID: 5033 RVA: 0x0005A7C8 File Offset: 0x000589C8
			public ManaCollection(BattleManaPanel parent, bool pooled, Vector2 origin, Vector2 delta, Vector2 highlightDelta, Vector2 pendingUseDelta)
			{
				this._parent = parent;
				this._isPooled = pooled;
				this._origin = origin;
				this._deltaPosition = delta;
				this._highlightDeltaPosition = highlightDelta;
				this._pendingUseDeltaPosition = pendingUseDelta;
				this._normalWidgets = parent.CreateManaWidgetList();
				this._highlightWidgets = parent.CreateManaWidgetList();
			}

			// Token: 0x170003F2 RID: 1010
			// (get) Token: 0x060013AA RID: 5034 RVA: 0x0005A820 File Offset: 0x00058A20
			public ManaGroup TotalMana
			{
				get
				{
					return this._totalMana;
				}
			}

			// Token: 0x170003F3 RID: 1011
			// (get) Token: 0x060013AB RID: 5035 RVA: 0x0005A828 File Offset: 0x00058A28
			public ManaGroup HighlightMana
			{
				get
				{
					return this._highlightMana;
				}
			}

			// Token: 0x170003F4 RID: 1012
			// (get) Token: 0x060013AC RID: 5036 RVA: 0x0005A830 File Offset: 0x00058A30
			public ManaGroup NormalMana
			{
				get
				{
					return this._totalMana - this._highlightMana;
				}
			}

			// Token: 0x060013AD RID: 5037 RVA: 0x0005A844 File Offset: 0x00058A44
			[Conditional("UNITY_EDITOR")]
			private void ConsistancyTest([CallerMemberName] string callderName = null)
			{
				ManaGroup empty = ManaGroup.Empty;
				foreach (BattleManaWidget battleManaWidget in this._normalWidgets)
				{
					ref ManaGroup ptr = ref empty;
					ManaColor manaColor = battleManaWidget.ManaColor;
					ptr[manaColor] += battleManaWidget.Amount;
				}
				if (empty != this._totalMana - this._highlightMana)
				{
					Debug.LogError(string.Format("{0}: normal mana mismatch: {1} vs {2}", callderName, empty, this._totalMana - this._highlightMana));
				}
				ManaGroup empty2 = ManaGroup.Empty;
				foreach (BattleManaWidget battleManaWidget2 in this._highlightWidgets)
				{
					ref ManaGroup ptr = ref empty2;
					ManaColor manaColor = battleManaWidget2.ManaColor;
					ptr[manaColor] += battleManaWidget2.Amount;
				}
				if (empty2 != this._highlightMana)
				{
					Debug.LogError(string.Format("{0}: highlight mana mismatch: {1} vs {2}", callderName, empty2, this._highlightMana));
				}
			}

			// Token: 0x060013AE RID: 5038 RVA: 0x0005A998 File Offset: 0x00058B98
			public bool ContainsNormalWidget(BattleManaWidget widget)
			{
				return this._normalWidgets.Contains(widget);
			}

			// Token: 0x060013AF RID: 5039 RVA: 0x0005A9A8 File Offset: 0x00058BA8
			public void ResetAllManas(ManaGroup total, bool transition = true)
			{
				this._isFolded = total.Amount > 10;
				this._totalMana = total;
				this._highlightMana = ManaGroup.Empty;
				this._parent.ClearWidgetList(this._normalWidgets, transition);
				this._parent.ClearWidgetList(this._highlightWidgets, transition);
				Vector2 vector = this._origin;
				foreach (ManaColor manaColor in BattleManaPanel.Colors)
				{
					int num = total[manaColor];
					if (num > 0)
					{
						if (this._isFolded)
						{
							BattleManaWidget battleManaWidget = this._parent.CreateManaWidget(manaColor, this._isPooled, num, BattleManaStatus.Active, vector, default(Vector2?));
							this._normalWidgets.Add(battleManaWidget);
							vector += this._deltaPosition;
						}
						else
						{
							for (int j = 0; j < num; j++)
							{
								BattleManaWidget battleManaWidget2 = this._parent.CreateManaWidget(manaColor, this._isPooled, 1, BattleManaStatus.Active, vector, default(Vector2?));
								this._normalWidgets.Add(battleManaWidget2);
								vector += this._deltaPosition;
							}
						}
					}
				}
			}

			// Token: 0x060013B0 RID: 5040 RVA: 0x0005AAC8 File Offset: 0x00058CC8
			public void SetHighlightMana(ManaGroup highlight)
			{
				if (highlight == this._highlightMana)
				{
					return;
				}
				if (!this._highlightMana.IsEmpty)
				{
					this.ClearHighlight();
				}
				if (highlight.IsEmpty)
				{
					return;
				}
				this._highlightMana = highlight;
				ManaGroup manaGroup = this._totalMana - highlight;
				Vector2 vector = this._origin;
				if (this._isFolded)
				{
					Vector2 vector2 = vector + this._deltaPosition * (float)manaGroup.ColorCount + this._highlightDeltaPosition;
					ManaColor[] colors = BattleManaPanel.Colors;
					for (int i = 0; i < colors.Length; i++)
					{
						ManaColor color = colors[i];
						int num = highlight[color];
						int num2 = this._normalWidgets.FindIndex((BattleManaWidget w) => w.ManaColor == color);
						if (num > 0)
						{
							BattleManaWidget battleManaWidget = this._normalWidgets[num2];
							if (battleManaWidget.Amount == num)
							{
								this._normalWidgets.RemoveAt(num2);
								battleManaWidget.MoveTo(vector2, this._isPooled, BattleManaStatus.Active);
								this._highlightWidgets.Add(battleManaWidget);
								vector2 += this._deltaPosition;
							}
							else
							{
								battleManaWidget.Amount -= num;
								battleManaWidget.MoveTo(vector, this._isPooled, BattleManaStatus.Active);
								BattleManaWidget battleManaWidget2 = this._parent.CreateManaWidget(color, this._isPooled, num, BattleManaStatus.Active, battleManaWidget.CurrentPosition, new Vector2?(vector2));
								this._highlightWidgets.Add(battleManaWidget2);
								vector += this._deltaPosition;
								vector2 += this._deltaPosition;
							}
						}
						else if (num2 >= 0)
						{
							this._normalWidgets[num2].MoveTo(vector, this._isPooled, BattleManaStatus.Active);
							vector += this._deltaPosition;
						}
					}
					return;
				}
				Vector2 vector3 = vector + this._deltaPosition * (float)manaGroup.Amount + this._highlightDeltaPosition;
				List<BattleManaWidget> list = this._parent.CreateManaWidgetList();
				foreach (BattleManaWidget battleManaWidget3 in this._normalWidgets)
				{
					ManaColor manaColor = battleManaWidget3.ManaColor;
					if (highlight[manaColor] > 0)
					{
						ManaColor manaColor2 = manaColor;
						int i = highlight[manaColor2] - 1;
						highlight[manaColor2] = i;
						battleManaWidget3.MoveTo(vector3, this._isPooled, BattleManaStatus.Active);
						this._highlightWidgets.Add(battleManaWidget3);
						vector3 += this._deltaPosition;
					}
					else
					{
						battleManaWidget3.MoveTo(vector, this._isPooled, BattleManaStatus.Active);
						list.Add(battleManaWidget3);
						vector += this._deltaPosition;
					}
				}
				this._parent.ReleaseManaWidgetList(this._normalWidgets);
				this._normalWidgets = list;
			}

			// Token: 0x060013B1 RID: 5041 RVA: 0x0005ADB8 File Offset: 0x00058FB8
			public void ClearHighlight()
			{
				if (this._highlightMana.IsEmpty)
				{
					return;
				}
				this._highlightMana = ManaGroup.Empty;
				Vector2 vector = this._origin;
				List<BattleManaWidget> list = this._parent.CreateManaWidgetList();
				if (this._isFolded)
				{
					ManaColor[] array = BattleManaPanel.Colors;
					int i = 0;
					while (i < array.Length)
					{
						ManaColor color = array[i];
						int num = this._normalWidgets.FindIndex((BattleManaWidget w) => w.ManaColor == color);
						int num2 = this._highlightWidgets.FindIndex((BattleManaWidget w) => w.ManaColor == color);
						if (num >= 0 && num2 >= 0)
						{
							BattleManaWidget battleManaWidget = this._normalWidgets[num];
							battleManaWidget.MoveTo(vector, this._isPooled, BattleManaStatus.Active);
							list.Add(battleManaWidget);
							BattleManaWidget battleManaWidget2 = this._highlightWidgets[num2];
							battleManaWidget.Amount += battleManaWidget2.Amount;
							this._parent.DisappearAndReleaseManaWidget(battleManaWidget2, vector);
							goto IL_0145;
						}
						if (num >= 0)
						{
							BattleManaWidget battleManaWidget3 = this._normalWidgets[num];
							battleManaWidget3.MoveTo(vector, this._isPooled, BattleManaStatus.Active);
							list.Add(battleManaWidget3);
							goto IL_0145;
						}
						if (num2 >= 0)
						{
							BattleManaWidget battleManaWidget4 = this._highlightWidgets[num2];
							battleManaWidget4.MoveTo(vector, this._isPooled, BattleManaStatus.Active);
							list.Add(battleManaWidget4);
							goto IL_0145;
						}
						IL_0152:
						i++;
						continue;
						IL_0145:
						vector += this._deltaPosition;
						goto IL_0152;
					}
				}
				else
				{
					foreach (ManaColor manaColor in BattleManaPanel.Colors)
					{
						foreach (BattleManaWidget battleManaWidget5 in this._normalWidgets.EnumerateEqualRange(manaColor, (BattleManaWidget w) => w.ManaColor))
						{
							battleManaWidget5.MoveTo(vector, this._isPooled, BattleManaStatus.Active);
							list.Add(battleManaWidget5);
							vector += this._deltaPosition;
						}
						foreach (BattleManaWidget battleManaWidget6 in this._highlightWidgets.EnumerateEqualRange(manaColor, (BattleManaWidget w) => w.ManaColor))
						{
							battleManaWidget6.MoveTo(vector, this._isPooled, BattleManaStatus.Active);
							list.Add(battleManaWidget6);
							vector += this._deltaPosition;
						}
					}
				}
				this._parent.ReleaseManaWidgetList(this._normalWidgets);
				this._normalWidgets = list;
				this._highlightWidgets.Clear();
			}

			// Token: 0x060013B2 RID: 5042 RVA: 0x0005B07C File Offset: 0x0005927C
			private void Fold()
			{
				if (this._isFolded)
				{
					Debug.LogError("Try to fold while already folded.");
					return;
				}
				ManaGroup manaGroup = this._totalMana - this._highlightMana;
				Vector2 vector = this._origin;
				if (manaGroup.Amount > 0)
				{
					List<BattleManaWidget> list = this._parent.CreateManaWidgetList();
					foreach (ManaColor manaColor in BattleManaPanel.Colors)
					{
						foreach (BattleManaWidget battleManaWidget in this._normalWidgets.EnumerateEqualRange(manaColor, (BattleManaWidget w) => w.ManaColor))
						{
							this._parent.DisappearAndReleaseManaWidget(battleManaWidget, vector);
						}
						int num = manaGroup[manaColor];
						if (num > 0)
						{
							list.Add(this._parent.CreateManaWidget(manaColor, this._isPooled, num, BattleManaStatus.Active, vector, default(Vector2?)));
							vector += this._deltaPosition;
						}
					}
					this._parent.ReleaseManaWidgetList(this._normalWidgets);
					this._normalWidgets = list;
				}
				if (this._highlightWidgets.Count > 0)
				{
					vector += this._highlightDeltaPosition;
					List<BattleManaWidget> list2 = this._parent.CreateManaWidgetList();
					foreach (ManaColor manaColor2 in BattleManaPanel.Colors)
					{
						int num2 = this._highlightMana[manaColor2];
						if (num2 > 0)
						{
							foreach (BattleManaWidget battleManaWidget2 in this._highlightWidgets.EnumerateEqualRange(manaColor2, (BattleManaWidget w) => w.ManaColor))
							{
								this._parent.DisappearAndReleaseManaWidget(battleManaWidget2, vector);
							}
							list2.Add(this._parent.CreateManaWidget(manaColor2, this._isPooled, num2, BattleManaStatus.Active, vector, default(Vector2?)));
							vector += this._deltaPosition;
						}
					}
					this._parent.ReleaseManaWidgetList(this._highlightWidgets);
					this._highlightWidgets = list2;
				}
				this._isFolded = true;
			}

			// Token: 0x060013B3 RID: 5043 RVA: 0x0005B2EC File Offset: 0x000594EC
			private void Unfold()
			{
				if (!this._isFolded)
				{
					Debug.LogError("Try to unfold while already unfolded.");
					return;
				}
				Vector2 vector = this._origin;
				if (this._normalWidgets.Count > 0)
				{
					List<BattleManaWidget> list = this._parent.CreateManaWidgetList();
					foreach (BattleManaWidget battleManaWidget in this._normalWidgets)
					{
						int amount = battleManaWidget.Amount;
						for (int i = 0; i < amount; i++)
						{
							list.Add(this._parent.CreateManaWidget(battleManaWidget.ManaColor, this._isPooled, 1, BattleManaStatus.Active, battleManaWidget.CurrentPosition, new Vector2?(vector)));
							vector += this._deltaPosition;
						}
						this._parent.DisappearAndReleaseManaWidget(battleManaWidget);
					}
					this._parent.ReleaseManaWidgetList(this._normalWidgets);
					this._normalWidgets = list;
				}
				if (this._highlightWidgets.Count > 0)
				{
					vector += this._highlightDeltaPosition;
					List<BattleManaWidget> list2 = this._parent.CreateManaWidgetList();
					foreach (BattleManaWidget battleManaWidget2 in this._highlightWidgets)
					{
						int amount2 = battleManaWidget2.Amount;
						for (int j = 0; j < amount2; j++)
						{
							list2.Add(this._parent.CreateManaWidget(battleManaWidget2.ManaColor, this._isPooled, 1, BattleManaStatus.Active, battleManaWidget2.CurrentPosition, new Vector2?(vector)));
							vector += this._deltaPosition;
						}
						this._parent.DisappearAndReleaseManaWidget(battleManaWidget2);
					}
					this._parent.ReleaseManaWidgetList(this._highlightWidgets);
					this._highlightWidgets = list2;
				}
				this._isFolded = false;
			}

			// Token: 0x060013B4 RID: 5044 RVA: 0x0005B4D0 File Offset: 0x000596D0
			private void AdjustAll()
			{
				Vector2 vector = this._origin;
				foreach (BattleManaWidget battleManaWidget in this._normalWidgets)
				{
					battleManaWidget.MoveTo(vector, this._isPooled, BattleManaStatus.Active);
					vector += this._deltaPosition;
				}
				vector += this._highlightDeltaPosition;
				foreach (BattleManaWidget battleManaWidget2 in this._highlightWidgets)
				{
					battleManaWidget2.MoveTo(vector, this._isFolded, BattleManaStatus.Active);
					vector += this._deltaPosition;
				}
			}

			// Token: 0x060013B5 RID: 5045 RVA: 0x0005B5A0 File Offset: 0x000597A0
			public void AddSingleNormalMana(BattleManaWidget widget)
			{
				ManaColor color = widget.ManaColor;
				ManaColor color2 = color;
				int num = this._totalMana[color2] + 1;
				this._totalMana[color2] = num;
				ManaGroup manaGroup = this._totalMana - this._highlightMana;
				if (this._totalMana.Amount <= 10)
				{
					int num2 = this._normalWidgets.UpperBound(color, (BattleManaWidget w) => w.ManaColor);
					this._normalWidgets.Insert(num2, widget);
					this.AdjustAll();
					return;
				}
				if (!this._isFolded)
				{
					this.Fold();
					int num3 = this._normalWidgets.FindIndex((BattleManaWidget w) => w.ManaColor == color);
					BattleManaWidget battleManaWidget = this._normalWidgets[num3];
					battleManaWidget.Amount = manaGroup[color];
					this._parent.DisappearAndReleaseManaWidget(widget, battleManaWidget.TargetPosition, this._isPooled);
					return;
				}
				int num4 = this._normalWidgets.UpperBound(color, (BattleManaWidget w) => w.ManaColor);
				if (num4 > 0 && this._normalWidgets[num4 - 1].ManaColor == color)
				{
					BattleManaWidget battleManaWidget2 = this._normalWidgets[num4 - 1];
					BattleManaWidget battleManaWidget3 = battleManaWidget2;
					num = battleManaWidget3.Amount + 1;
					battleManaWidget3.Amount = num;
					this._parent.DisappearAndReleaseManaWidget(widget, battleManaWidget2.TargetPosition, this._isPooled);
					return;
				}
				this._normalWidgets.Insert(num4, widget);
				this.AdjustAll();
			}

			// Token: 0x060013B6 RID: 5046 RVA: 0x0005B754 File Offset: 0x00059954
			public void AddNormalMana(ManaGroup adding, [MaybeNull] IReadOnlyList<BattleManaWidget> sourceWidgets, [MaybeNull] IList<BattleManaWidget> result)
			{
				Vector2 vector = this._origin;
				this._totalMana += adding;
				ManaGroup manaGroup = this._totalMana - this._highlightMana;
				if (this._totalMana.Amount > 10)
				{
					if (!this._isFolded)
					{
						this.Fold();
					}
					else
					{
						foreach (BattleManaWidget battleManaWidget in this._normalWidgets)
						{
							battleManaWidget.Amount = manaGroup[battleManaWidget.ManaColor];
						}
					}
					foreach (ManaColor manaColor in BattleManaPanel.Colors)
					{
						int num = this._normalWidgets.UpperBound(manaColor, (BattleManaWidget w) => w.ManaColor);
						BattleManaWidget battleManaWidget2 = ((num > 0 && this._normalWidgets[num - 1].ManaColor == manaColor) ? this._normalWidgets[num - 1] : null);
						int num2 = adding[manaColor];
						if (num2 > 0)
						{
							if (battleManaWidget2 != null)
							{
								battleManaWidget2.MoveTo(vector, this._isPooled, BattleManaStatus.Active);
								if (result != null)
								{
									result.Add(battleManaWidget2);
								}
							}
							else
							{
								BattleManaWidget battleManaWidget3 = this._parent.CreateManaWidget(manaColor, this._isPooled, num2, BattleManaStatus.Active, vector, default(Vector2?));
								this._normalWidgets.Insert(num, battleManaWidget3);
								if (result != null)
								{
									result.Add(battleManaWidget3);
								}
							}
							if (sourceWidgets != null)
							{
								foreach (BattleManaWidget battleManaWidget4 in sourceWidgets)
								{
									if (battleManaWidget4.ManaColor == manaColor)
									{
										this._parent.DisappearAndReleaseManaWidget(battleManaWidget4, vector);
									}
								}
							}
							vector += this._deltaPosition;
						}
						else if (battleManaWidget2 != null)
						{
							battleManaWidget2.MoveTo(vector, this._isPooled, BattleManaStatus.Active);
							vector += this._deltaPosition;
						}
					}
				}
				else
				{
					List<BattleManaWidget> list = this._parent.CreateManaWidgetList();
					foreach (ManaColor manaColor2 in BattleManaPanel.Colors)
					{
						using (BasicTypeExtensions.RangeEnumerator rangeEnumerator = this._normalWidgets.EqualRange(manaColor2, (BattleManaWidget w) => w.ManaColor).GetEnumerator())
						{
							while (rangeEnumerator.MoveNext())
							{
								int num3 = rangeEnumerator.Current;
								BattleManaWidget battleManaWidget5 = this._normalWidgets[num3];
								battleManaWidget5.MoveTo(vector, this._isPooled, BattleManaStatus.Active);
								list.Add(battleManaWidget5);
								vector += this._deltaPosition;
							}
						}
						int num4 = adding[manaColor2];
						if (num4 > 0)
						{
							if (sourceWidgets != null)
							{
								Range range = sourceWidgets.EqualRange(manaColor2, (BattleManaWidget w) => w.ManaColor);
								range.Count();
								using (BasicTypeExtensions.RangeEnumerator rangeEnumerator = range.GetEnumerator())
								{
									while (rangeEnumerator.MoveNext())
									{
										int num5 = rangeEnumerator.Current;
										BattleManaWidget battleManaWidget6 = sourceWidgets[num5];
										battleManaWidget6.MoveTo(vector, this._isPooled, BattleManaStatus.Active);
										list.Add(battleManaWidget6);
										if (result != null)
										{
											result.Add(battleManaWidget6);
										}
										vector += this._deltaPosition;
									}
									goto IL_03AD;
								}
							}
							for (int j = 0; j < num4; j++)
							{
								BattleManaWidget battleManaWidget7 = this._parent.CreateManaWidget(manaColor2, this._isPooled, 1, BattleManaStatus.Active, vector, default(Vector2?));
								list.Add(battleManaWidget7);
								if (result != null)
								{
									result.Add(battleManaWidget7);
								}
								vector += this._deltaPosition;
							}
						}
						IL_03AD:;
					}
					this._parent.ReleaseManaWidgetList(this._normalWidgets);
					this._normalWidgets = list;
				}
				foreach (BattleManaWidget battleManaWidget8 in this._highlightWidgets)
				{
					battleManaWidget8.MoveTo(vector, this._isPooled, BattleManaStatus.Active);
					vector += this._deltaPosition;
				}
			}

			// Token: 0x060013B7 RID: 5047 RVA: 0x0005BBC0 File Offset: 0x00059DC0
			public BattleManaWidget ExtractSingleNormalMana(BattleManaWidget widget)
			{
				if (!this._normalWidgets.Contains(widget))
				{
					throw new InvalidOperationException("Cannot extra widget that is not in normal widgets");
				}
				ManaColor manaColor = widget.ManaColor;
				int num;
				if (this._isFolded)
				{
					bool flag = this._totalMana.Amount - 1 <= 10;
					if (widget.Amount > 1)
					{
						BattleManaWidget battleManaWidget = widget;
						num = battleManaWidget.Amount - 1;
						battleManaWidget.Amount = num;
						widget = this._parent.CreateInactiveManaWidget(manaColor, this._isPooled, 1);
						widget.InstantAppear(BattleManaStatus.Active, widget.CurrentPosition);
						if (flag)
						{
							this.Unfold();
						}
					}
					else
					{
						this._normalWidgets.Remove(widget);
						if (flag)
						{
							this.Unfold();
						}
						else
						{
							this.AdjustAll();
						}
					}
				}
				else
				{
					this._normalWidgets.Remove(widget);
					this.AdjustAll();
				}
				ManaColor manaColor2 = manaColor;
				num = this._totalMana[manaColor2] - 1;
				this._totalMana[manaColor2] = num;
				return widget;
			}

			// Token: 0x060013B8 RID: 5048 RVA: 0x0005BCA0 File Offset: 0x00059EA0
			public void RemoveNormalMana(ManaGroup removing, [MaybeNull] IList<BattleManaWidget> result)
			{
				ManaGroup normalMana = this.NormalMana;
				if (!normalMana.CanAfford(removing))
				{
					throw new InvalidOperationException(string.Format("Cannot remove {0} from {1}", removing, normalMana));
				}
				if (!this._isFolded)
				{
					List<BattleManaWidget> list = this._parent.CreateManaWidgetList();
					foreach (ManaColor manaColor in BattleManaPanel.Colors)
					{
						int num = removing[manaColor];
						int num2 = normalMana[manaColor] - num;
						Range range = this._normalWidgets.EqualRange(manaColor, (BattleManaWidget w) => w.ManaColor);
						int value = range.Start.Value;
						int value2 = range.End.Value;
						int num3 = value + num2;
						for (int j = value; j < num3; j++)
						{
							BattleManaWidget battleManaWidget = this._normalWidgets[j];
							list.Add(battleManaWidget);
						}
						for (int k = num3; k < value2; k++)
						{
							BattleManaWidget battleManaWidget2 = this._normalWidgets[k];
							if (result != null)
							{
								result.Add(battleManaWidget2);
							}
							else
							{
								this._parent.DisappearAndReleaseManaWidget(battleManaWidget2);
							}
						}
					}
					this._parent.ReleaseManaWidgetList(this._normalWidgets);
					this._normalWidgets = list;
					this._totalMana -= removing;
					this.AdjustAll();
					return;
				}
				ManaColor[] array = BattleManaPanel.Colors;
				for (int i = 0; i < array.Length; i++)
				{
					ManaColor color = array[i];
					int num4 = removing[color];
					if (num4 > 0)
					{
						int num5 = this._normalWidgets.FindIndex((BattleManaWidget w) => w.ManaColor == color);
						BattleManaWidget battleManaWidget3 = this._normalWidgets[num5];
						if (battleManaWidget3.Amount > num4)
						{
							battleManaWidget3.Amount -= num4;
							if (result != null)
							{
								for (int l = 0; l < num4; l++)
								{
									BattleManaWidget battleManaWidget4 = this._parent.CreateInactiveManaWidget(color, this._isPooled, 1);
									battleManaWidget4.InstantAppear(BattleManaStatus.Active, battleManaWidget3.CurrentPosition);
									result.Add(battleManaWidget4);
								}
							}
						}
						else
						{
							this._normalWidgets.RemoveAt(num5);
							if (result != null)
							{
								if (battleManaWidget3.Amount == 1)
								{
									result.Add(battleManaWidget3);
								}
								else
								{
									for (int m = 0; m < num4; m++)
									{
										BattleManaWidget battleManaWidget5 = this._parent.CreateInactiveManaWidget(color, this._isPooled, 1);
										battleManaWidget5.InstantAppear(BattleManaStatus.Active, battleManaWidget3.CurrentPosition);
										result.Add(battleManaWidget5);
									}
									this._parent.DisappearAndReleaseManaWidget(battleManaWidget3);
								}
							}
							else
							{
								this._parent.DisappearAndReleaseManaWidget(battleManaWidget3);
							}
						}
					}
				}
				this._totalMana -= removing;
				if (this._totalMana.Amount <= 10)
				{
					this.Unfold();
					return;
				}
				this.AdjustAll();
			}

			// Token: 0x060013B9 RID: 5049 RVA: 0x0005BF98 File Offset: 0x0005A198
			public List<BattleManaWidget> Prepay()
			{
				List<BattleManaWidget> highlightWidgets = this._highlightWidgets;
				this._totalMana -= this._highlightMana;
				this._highlightMana = ManaGroup.Empty;
				this._highlightWidgets = this._parent.CreateManaWidgetList();
				if (this._isFolded && this._totalMana.Amount <= 10)
				{
					this.Unfold();
				}
				Vector2 vector = this._origin + (float)this._normalWidgets.Count * this._deltaPosition + this._highlightDeltaPosition + this._pendingUseDeltaPosition;
				foreach (BattleManaWidget battleManaWidget in highlightWidgets)
				{
					battleManaWidget.MoveTo(vector, battleManaWidget.IsPooled, BattleManaStatus.PendingUse);
					vector += this._deltaPosition;
				}
				return highlightWidgets;
			}

			// Token: 0x04000F6B RID: 3947
			private readonly BattleManaPanel _parent;

			// Token: 0x04000F6C RID: 3948
			private readonly Vector2 _origin;

			// Token: 0x04000F6D RID: 3949
			private readonly Vector2 _deltaPosition;

			// Token: 0x04000F6E RID: 3950
			private readonly Vector2 _highlightDeltaPosition;

			// Token: 0x04000F6F RID: 3951
			private readonly Vector2 _pendingUseDeltaPosition;

			// Token: 0x04000F70 RID: 3952
			private readonly bool _isPooled;

			// Token: 0x04000F71 RID: 3953
			private List<BattleManaWidget> _normalWidgets;

			// Token: 0x04000F72 RID: 3954
			private List<BattleManaWidget> _highlightWidgets;

			// Token: 0x04000F73 RID: 3955
			private ManaGroup _totalMana;

			// Token: 0x04000F74 RID: 3956
			private ManaGroup _highlightMana;

			// Token: 0x04000F75 RID: 3957
			private bool _isFolded;
		}
	}
}
