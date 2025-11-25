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
	public class UnitStatusWidget : MonoBehaviour
	{
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
		private void Awake()
		{
			this.statusEffectTemplate.gameObject.SetActive(false);
			this._scenePositionTier = base.GetComponent<ScenePositionTier>() ?? base.gameObject.AddComponent<ScenePositionTier>();
			this._canvasGroup = base.GetComponent<CanvasGroup>() ?? base.gameObject.AddComponent<CanvasGroup>();
		}
		private void OnDestroy()
		{
			this._canvasGroup.DOKill(false);
		}
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
		public void OnRemoveStatusEffect(StatusEffect effect)
		{
			StatusEffectWidget statusEffectWidget = this.FindStatusEffect(effect);
			if (statusEffectWidget)
			{
				this._statusEffectWidgets.Remove(statusEffectWidget);
				Object.Destroy(statusEffectWidget.gameObject);
			}
		}
		public void OnMaxHpChanged()
		{
			PlayerUnit playerUnit = this.Unit as PlayerUnit;
			if (playerUnit != null)
			{
				this.SetPlayerHpBarLength(playerUnit.MaxHp);
			}
			this.TweenHpBar();
		}
		public void OnDamageReceived(DamageInfo damageInfo)
		{
			this.TweenHpBar();
		}
		public void OnHealingReceived(int healAmount)
		{
			this.TweenHpBar();
		}
		public void OnBlockShieldChanged()
		{
			this.TweenHpBar();
		}
		public void OnForceKill()
		{
			this.TweenHpBar();
		}
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
		private void SetHpBar()
		{
			this.hpBar.SetHp(this._unit.Hp, this._unit.MaxHp);
			this.hpBar.SetShield(this._unit.Shield, this._unit.Block);
			this.hpBar.TweenHp(this._unit.Hp, this._unit.MaxHp, this._unit.Shield, this._unit.Block, true);
		}
		private void TweenHpBar()
		{
			this.hpBar.TweenHp(this._unit.Hp, this._unit.MaxHp, this._unit.Shield, this._unit.Block, false);
		}
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
		public StatusEffectWidget FindStatusEffect(StatusEffect effect)
		{
			return Enumerable.FirstOrDefault<StatusEffectWidget>(this._statusEffectWidgets, (StatusEffectWidget e) => e.StatusEffect == effect);
		}
		private void SetStatusEffects()
		{
			this.ClearStatusEffects();
			foreach (StatusEffect statusEffect in this._unit.StatusEffects)
			{
				this._statusEffectWidgets.Add(this.CreateStatusEffectWidget(statusEffect, false));
			}
		}
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
		private readonly List<StatusEffectWidget> _statusEffectWidgets = new List<StatusEffectWidget>();
		private ScenePositionTier _scenePositionTier;
		private CanvasGroup _canvasGroup;
		private Unit _unit;
		[SerializeField]
		private RectTransform rectTransform;
		[SerializeField]
		private HealthBar hpBar;
		[SerializeField]
		private StatusEffectWidget statusEffectTemplate;
		[SerializeField]
		private Transform statusEffectParent;
		private const float MinBarLength = 240f;
		private const float MaxBarLength = 640f;
		private float _barLength;
		private const float PlayerMinLength = 240f;
		private const float PlayerMaxLength = 640f;
		private const int PlayerMinLengthHp = 45;
		private const int PlayerMaxLengthHp = 120;
	}
}
