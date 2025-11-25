using System;
using System.Collections;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Presentation.UI.ExtraWidgets;
using UnityEngine;
namespace LBoL.Presentation.UI.Panels
{
	public class PopupHud : UiPanel
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Base;
			}
		}
		public static PopupHud Instance { get; private set; }
		public void Awake()
		{
			PopupHud.Instance = this;
			this.damagePopup.gameObject.SetActive(false);
		}
		public void DamagePopupFromScene(DamageInfo damage, Vector3 worldPosition, bool sourceIsPlayer = false)
		{
			base.StartCoroutine(this.DamagePopupRunner(damage, worldPosition, sourceIsPlayer));
		}
		private IEnumerator DamagePopupRunner(DamageInfo damage, Vector3 worldPosition, bool sourceIsPlayer)
		{
			List<PopupHud.PopSource> list = new List<PopupHud.PopSource>();
			if (damage.DamageBlocked > 0f)
			{
				list.Add(new PopupHud.PopSource(damage.DamageBlocked.RoundToInt(), PopupHud.BlockColor));
			}
			if (damage.DamageShielded > 0f)
			{
				list.Add(new PopupHud.PopSource(damage.DamageShielded.RoundToInt(), PopupHud.ShieldColor));
			}
			if (damage.Damage > 0f || list.Count == 0)
			{
				list.Add(new PopupHud.PopSource(damage.Damage.RoundToInt(), (damage.DamageType == DamageType.HpLose) ? PopupHud.PlayerHitColor : (sourceIsPlayer ? PopupHud.EnemyHitColor : PopupHud.PlayerHitColor)));
			}
			foreach (PopupHud.PopSource popSource in list)
			{
				this.PopupFromScene(popSource.PopValue, popSource.PopColor, worldPosition);
				yield return new WaitForSeconds(0.1f);
			}
			List<PopupHud.PopSource>.Enumerator enumerator = default(List<PopupHud.PopSource>.Enumerator);
			yield break;
			yield break;
		}
		public void HealPopupFromScene(int healAmount, Vector3 worldPosition)
		{
			this.PopupFromScene(healAmount, PopupHud.HealColor, worldPosition);
		}
		private void PopupFromScene(int popValue, Color color, Vector3 worldPosition)
		{
			DamagePopup damagePopup = Object.Instantiate<DamagePopup>(this.damagePopup, base.transform);
			damagePopup.transform.localPosition = CameraController.ScenePositionToLocalPositionInRectTransform(worldPosition, (RectTransform)base.transform);
			damagePopup.tmp.text = popValue.ToString();
			damagePopup.tmp.color = color;
			damagePopup.Show();
		}
		[SerializeField]
		private DamagePopup damagePopup;
		private const float Interval = 0.1f;
		private static readonly Color BlockColor = new Color32(108, 216, 209, byte.MaxValue);
		private static readonly Color ShieldColor = new Color32(76, 182, 242, byte.MaxValue);
		private static readonly Color PlayerHitColor = new Color32(byte.MaxValue, 77, 77, byte.MaxValue);
		private static readonly Color EnemyHitColor = Color.white;
		private static readonly Color HealColor = new Color32(153, 245, 125, byte.MaxValue);
		private class PopSource
		{
			public PopSource(int popValue, Color popColor)
			{
				this.PopValue = popValue;
				this.PopColor = popColor;
			}
			public int PopValue { get; }
			public Color PopColor { get; }
		}
	}
}
