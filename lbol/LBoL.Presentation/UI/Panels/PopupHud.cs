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
	// Token: 0x020000A8 RID: 168
	public class PopupHud : UiPanel
	{
		// Token: 0x17000175 RID: 373
		// (get) Token: 0x06000946 RID: 2374 RVA: 0x0002F95D File Offset: 0x0002DB5D
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Base;
			}
		}

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x06000947 RID: 2375 RVA: 0x0002F960 File Offset: 0x0002DB60
		// (set) Token: 0x06000948 RID: 2376 RVA: 0x0002F967 File Offset: 0x0002DB67
		public static PopupHud Instance { get; private set; }

		// Token: 0x06000949 RID: 2377 RVA: 0x0002F96F File Offset: 0x0002DB6F
		public void Awake()
		{
			PopupHud.Instance = this;
			this.damagePopup.gameObject.SetActive(false);
		}

		// Token: 0x0600094A RID: 2378 RVA: 0x0002F988 File Offset: 0x0002DB88
		public void DamagePopupFromScene(DamageInfo damage, Vector3 worldPosition, bool sourceIsPlayer = false)
		{
			base.StartCoroutine(this.DamagePopupRunner(damage, worldPosition, sourceIsPlayer));
		}

		// Token: 0x0600094B RID: 2379 RVA: 0x0002F99A File Offset: 0x0002DB9A
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

		// Token: 0x0600094C RID: 2380 RVA: 0x0002F9BE File Offset: 0x0002DBBE
		public void HealPopupFromScene(int healAmount, Vector3 worldPosition)
		{
			this.PopupFromScene(healAmount, PopupHud.HealColor, worldPosition);
		}

		// Token: 0x0600094D RID: 2381 RVA: 0x0002F9D0 File Offset: 0x0002DBD0
		private void PopupFromScene(int popValue, Color color, Vector3 worldPosition)
		{
			DamagePopup damagePopup = Object.Instantiate<DamagePopup>(this.damagePopup, base.transform);
			damagePopup.transform.localPosition = CameraController.ScenePositionToLocalPositionInRectTransform(worldPosition, (RectTransform)base.transform);
			damagePopup.tmp.text = popValue.ToString();
			damagePopup.tmp.color = color;
			damagePopup.Show();
		}

		// Token: 0x040006D8 RID: 1752
		[SerializeField]
		private DamagePopup damagePopup;

		// Token: 0x040006DA RID: 1754
		private const float Interval = 0.1f;

		// Token: 0x040006DB RID: 1755
		private static readonly Color BlockColor = new Color32(108, 216, 209, byte.MaxValue);

		// Token: 0x040006DC RID: 1756
		private static readonly Color ShieldColor = new Color32(76, 182, 242, byte.MaxValue);

		// Token: 0x040006DD RID: 1757
		private static readonly Color PlayerHitColor = new Color32(byte.MaxValue, 77, 77, byte.MaxValue);

		// Token: 0x040006DE RID: 1758
		private static readonly Color EnemyHitColor = Color.white;

		// Token: 0x040006DF RID: 1759
		private static readonly Color HealColor = new Color32(153, 245, 125, byte.MaxValue);

		// Token: 0x02000295 RID: 661
		private class PopSource
		{
			// Token: 0x0600163F RID: 5695 RVA: 0x00064263 File Offset: 0x00062463
			public PopSource(int popValue, Color popColor)
			{
				this.PopValue = popValue;
				this.PopColor = popColor;
			}

			// Token: 0x1700045C RID: 1116
			// (get) Token: 0x06001640 RID: 5696 RVA: 0x00064279 File Offset: 0x00062479
			public int PopValue { get; }

			// Token: 0x1700045D RID: 1117
			// (get) Token: 0x06001641 RID: 5697 RVA: 0x00064281 File Offset: 0x00062481
			public Color PopColor { get; }
		}
	}
}
