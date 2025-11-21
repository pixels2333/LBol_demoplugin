using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200007C RID: 124
	public class UnitStatusWidget : MonoBehaviour
	{
		// Token: 0x1700011C RID: 284
		// (get) Token: 0x06000666 RID: 1638 RVA: 0x0001BA76 File Offset: 0x00019C76
		// (set) Token: 0x06000667 RID: 1639 RVA: 0x0001BA7E File Offset: 0x00019C7E
		public Unit Unit
		{
			get
			{
				return this._unit;
			}
			set
			{
				this._unit = value;
				if (value != null)
				{
					this.SetHpBar();
					this.SetStatusEffects();
					return;
				}
				this.ClearStatusEffects();
			}
		}

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x06000668 RID: 1640 RVA: 0x0001BA9D File Offset: 0x00019C9D
		// (set) Token: 0x06000669 RID: 1641 RVA: 0x0001BAAC File Offset: 0x00019CAC
		public Transform TargetTransform
		{
			get
			{
				return this._scenePositionTier.TargetTransform;
			}
			set
			{
				if (this._scenePositionTier == null)
				{
					Debug.LogError("Setting TargetTransform before Awake");
					this._scenePositionTier = base.GetComponent<ScenePositionTier>() ?? base.gameObject.AddComponent<ScenePositionTier>();
				}
				this._scenePositionTier.TargetTransform = value;
			}
		}

		// Token: 0x0600066A RID: 1642 RVA: 0x0001BAF8 File Offset: 0x00019CF8
		private void Awake()
		{
			this.statusEffectTemplate.gameObject.SetActive(false);
			this._scenePositionTier = base.GetComponent<ScenePositionTier>() ?? base.gameObject.AddComponent<ScenePositionTier>();
			this._canvasGroup = base.GetComponent<CanvasGroup>() ?? base.gameObject.AddComponent<CanvasGroup>();
		}

		// Token: 0x0600066B RID: 1643 RVA: 0x0001BB4C File Offset: 0x00019D4C
		private void OnDestroy()
		{
			this._canvasGroup.DOKill(false);
		}

		// Token: 0x0600066C RID: 1644 RVA: 0x0001BB5C File Offset: 0x00019D5C
		public void OnAddStatusEffect(StatusEffect effect, StatusEffectAddResult addResult)
		{
			if (addResult == StatusEffectAddResult.Added)
			{
				StatusEffectWidget statusEffectWidget = this.CreateStatusEffectWidget(effect, true);
				statusEffectWidget.ShowAdded();
				this._statusEffectWidgets.Add(statusEffectWidget);
				return;
			}
			if (addResult == StatusEffectAddResult.Stacked)
			{
				StatusEffect statusEffect = this._unit.GetStatusEffect(effect.GetType());
				if (statusEffect != null)
				{
					StatusEffectWidget statusEffectWidget2 = this.FindStatusEffect(statusEffect);
					if (statusEffectWidget2)
					{
						statusEffectWidget2.ShowAdded();
						return;
					}
				}
			}
			else if (addResult != StatusEffectAddResult.Opposed)
			{
			}
		}

		// Token: 0x0600066D RID: 1645 RVA: 0x0001BBC4 File Offset: 0x00019DC4
		public void OnRemoveStatusEffect(StatusEffect effect)
		{
			StatusEffectWidget statusEffectWidget = this.FindStatusEffect(effect);
			if (statusEffectWidget)
			{
				this._statusEffectWidgets.Remove(statusEffectWidget);
				Object.Destroy(statusEffectWidget.gameObject);
			}
		}

		// Token: 0x0600066E RID: 1646 RVA: 0x0001BBFC File Offset: 0x00019DFC
		public void OnMaxHpChanged()
		{
			PlayerUnit playerUnit = this.Unit as PlayerUnit;
			if (playerUnit != null)
			{
				this.SetPlayerHpBarLength(playerUnit.MaxHp);
			}
			this.TweenHpBar();
		}

		// Token: 0x0600066F RID: 1647 RVA: 0x0001BC2A File Offset: 0x00019E2A
		public void OnDamageReceived(DamageInfo damageInfo)
		{
			this.TweenHpBar();
		}

		// Token: 0x06000670 RID: 1648 RVA: 0x0001BC32 File Offset: 0x00019E32
		public void OnHealingReceived(int healAmount)
		{
			this.TweenHpBar();
		}

		// Token: 0x06000671 RID: 1649 RVA: 0x0001BC3A File Offset: 0x00019E3A
		public void OnBlockShieldChanged()
		{
			this.TweenHpBar();
		}

		// Token: 0x06000672 RID: 1650 RVA: 0x0001BC42 File Offset: 0x00019E42
		public void OnForceKill()
		{
			this.TweenHpBar();
		}

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x06000673 RID: 1651 RVA: 0x0001BC4A File Offset: 0x00019E4A
		// (set) Token: 0x06000674 RID: 1652 RVA: 0x0001BC57 File Offset: 0x00019E57
		public float Alpha
		{
			get
			{
				return this._canvasGroup.alpha;
			}
			set
			{
				this._canvasGroup.alpha = value;
			}
		}

		// Token: 0x06000675 RID: 1653 RVA: 0x0001BC65 File Offset: 0x00019E65
		public void SetVisible(bool visible, bool instant = false)
		{
			this._canvasGroup.DOKill(false);
			if (instant)
			{
				this._canvasGroup.alpha = (float)(visible ? 1 : 0);
				return;
			}
			this._canvasGroup.DOFade((float)(visible ? 1 : 0), 0.2f);
		}

		// Token: 0x06000676 RID: 1654 RVA: 0x0001BCA4 File Offset: 0x00019EA4
		private void SetHpBar()
		{
			this.hpBar.SetHp(this._unit.Hp, this._unit.MaxHp);
			this.hpBar.SetShield(this._unit.Shield, this._unit.Block);
			this.hpBar.TweenHp(this._unit.Hp, this._unit.MaxHp, this._unit.Shield, this._unit.Block, true);
		}

		// Token: 0x06000677 RID: 1655 RVA: 0x0001BD2B File Offset: 0x00019F2B
		private void TweenHpBar()
		{
			this.hpBar.TweenHp(this._unit.Hp, this._unit.MaxHp, this._unit.Shield, this._unit.Block, false);
		}

		// Token: 0x06000678 RID: 1656 RVA: 0x0001BD68 File Offset: 0x00019F68
		private StatusEffectWidget CreateStatusEffectWidget(StatusEffect effect, bool setIndex)
		{
			StatusEffectWidget statusEffectWidget = Object.Instantiate<StatusEffectWidget>(this.statusEffectTemplate, this.statusEffectParent);
			statusEffectWidget.gameObject.name = "SE: " + effect.Id;
			statusEffectWidget.gameObject.SetActive(true);
			statusEffectWidget.StatusEffect = effect;
			if (setIndex)
			{
				int num = effect.Owner.StatusEffects.IndexOf(effect);
				statusEffectWidget.transform.SetSiblingIndex(num);
			}
			return statusEffectWidget;
		}

		// Token: 0x06000679 RID: 1657 RVA: 0x0001BDD8 File Offset: 0x00019FD8
		public StatusEffectWidget FindStatusEffect(StatusEffect effect)
		{
			return Enumerable.FirstOrDefault<StatusEffectWidget>(this._statusEffectWidgets, (StatusEffectWidget e) => e.StatusEffect == effect);
		}

		// Token: 0x0600067A RID: 1658 RVA: 0x0001BE0C File Offset: 0x0001A00C
		private void SetStatusEffects()
		{
			this.ClearStatusEffects();
			foreach (StatusEffect statusEffect in this._unit.StatusEffects)
			{
				this._statusEffectWidgets.Add(this.CreateStatusEffectWidget(statusEffect, false));
			}
		}

		// Token: 0x0600067B RID: 1659 RVA: 0x0001BE70 File Offset: 0x0001A070
		public void ClearStatusEffects()
		{
			foreach (StatusEffectWidget statusEffectWidget in this._statusEffectWidgets)
			{
				if (statusEffectWidget)
				{
					Object.Destroy(statusEffectWidget.gameObject);
				}
			}
			this._statusEffectWidgets.Clear();
		}

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x0600067C RID: 1660 RVA: 0x0001BEDC File Offset: 0x0001A0DC
		// (set) Token: 0x0600067D RID: 1661 RVA: 0x0001BEE4 File Offset: 0x0001A0E4
		public float BarLength
		{
			get
			{
				return this._barLength;
			}
			set
			{
				this._barLength = Mathf.Min(Mathf.Max(value, 240f), 640f);
				this.rectTransform.sizeDelta = new Vector2(this._barLength, this.rectTransform.sizeDelta.y);
			}
		}

		// Token: 0x0600067E RID: 1662 RVA: 0x0001BF34 File Offset: 0x0001A134
		public void SetPlayerHpBarLength(int maxHp)
		{
			float num;
			if (maxHp > 45)
			{
				if (maxHp < 120)
				{
					num = (float)(maxHp - 45) / 75f * 400f + 240f;
				}
				else
				{
					num = 640f;
				}
			}
			else
			{
				num = 240f;
			}
			this.BarLength = num;
		}

		// Token: 0x040003F9 RID: 1017
		private readonly List<StatusEffectWidget> _statusEffectWidgets = new List<StatusEffectWidget>();

		// Token: 0x040003FA RID: 1018
		private ScenePositionTier _scenePositionTier;

		// Token: 0x040003FB RID: 1019
		private CanvasGroup _canvasGroup;

		// Token: 0x040003FC RID: 1020
		private Unit _unit;

		// Token: 0x040003FD RID: 1021
		[SerializeField]
		private RectTransform rectTransform;

		// Token: 0x040003FE RID: 1022
		[SerializeField]
		private HealthBar hpBar;

		// Token: 0x040003FF RID: 1023
		[SerializeField]
		private StatusEffectWidget statusEffectTemplate;

		// Token: 0x04000400 RID: 1024
		[SerializeField]
		private Transform statusEffectParent;

		// Token: 0x04000401 RID: 1025
		private const float MinBarLength = 240f;

		// Token: 0x04000402 RID: 1026
		private const float MaxBarLength = 640f;

		// Token: 0x04000403 RID: 1027
		private float _barLength;

		// Token: 0x04000404 RID: 1028
		private const float PlayerMinLength = 240f;

		// Token: 0x04000405 RID: 1029
		private const float PlayerMaxLength = 640f;

		// Token: 0x04000406 RID: 1030
		private const int PlayerMinLengthHp = 45;

		// Token: 0x04000407 RID: 1031
		private const int PlayerMaxLengthHp = 120;
	}
}
