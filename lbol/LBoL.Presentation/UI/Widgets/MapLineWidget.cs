using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Core.Stations;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000062 RID: 98
	public class MapLineWidget : MonoBehaviour
	{
		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x06000551 RID: 1361 RVA: 0x00017078 File Offset: 0x00015278
		// (set) Token: 0x06000552 RID: 1362 RVA: 0x00017080 File Offset: 0x00015280
		public MapLineWidget.LineStatus Status { get; private set; }

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x06000553 RID: 1363 RVA: 0x00017089 File Offset: 0x00015289
		// (set) Token: 0x06000554 RID: 1364 RVA: 0x00017091 File Offset: 0x00015291
		public MapNodeWidget SourceWidget { get; private set; }

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x06000555 RID: 1365 RVA: 0x0001709A File Offset: 0x0001529A
		// (set) Token: 0x06000556 RID: 1366 RVA: 0x000170A2 File Offset: 0x000152A2
		public MapNodeWidget TargetWidget { get; private set; }

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x06000557 RID: 1367 RVA: 0x000170AB File Offset: 0x000152AB
		public int SourceX
		{
			get
			{
				return this.SourceWidget.X;
			}
		}

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x06000558 RID: 1368 RVA: 0x000170B8 File Offset: 0x000152B8
		public int SourceY
		{
			get
			{
				return this.SourceWidget.Y;
			}
		}

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x06000559 RID: 1369 RVA: 0x000170C5 File Offset: 0x000152C5
		public int TargetX
		{
			get
			{
				return this.TargetWidget.X;
			}
		}

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x0600055A RID: 1370 RVA: 0x000170D2 File Offset: 0x000152D2
		public int TargetY
		{
			get
			{
				return this.TargetWidget.Y;
			}
		}

		// Token: 0x0600055B RID: 1371 RVA: 0x000170E0 File Offset: 0x000152E0
		public void Initialize(MapNodeWidget sourceWidget, MapNodeWidget targetWidget, RandomGen uiRng)
		{
			this.SourceWidget = sourceWidget;
			this.TargetWidget = targetWidget;
			int num = uiRng.NextInt(0, this.lineSpritePairs.Keys.Count - 1);
			this.lineOrigin.sprite = this.lineSpritePairs.EntryAt(num).key;
			this.linePass.sprite = this.lineSpritePairs.EntryAt(num).value;
			Vector2 grid = sourceWidget.Grid;
			Vector2 grid2 = targetWidget.Grid;
			if (sourceWidget.MapNode.StationType == StationType.Boss)
			{
				Debug.LogWarning("现在没考虑Boss节点后面还能接其他节点的情况，UI显示是不正确的");
			}
			float num3;
			if (targetWidget.MapNode.StationType == StationType.Boss)
			{
				float num2 = 180f / (grid2 - grid).magnitude;
				base.transform.localPosition = grid * (1f + num2) / 2f + grid2 * (1f - num2) / 2f;
				num3 = (grid2 - grid).magnitude - 120f - 300f;
			}
			else
			{
				base.transform.localPosition = (grid + grid2) / 2f;
				num3 = (grid2 - grid).magnitude - 240f;
			}
			base.GetComponent<RectTransform>().sizeDelta = new Vector2(num3, num3 / 10f);
			base.transform.localRotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, grid2 - grid));
			this.SetLineImage(this.Status == MapLineWidget.LineStatus.Acrossed);
			if (this.Status == MapLineWidget.LineStatus.Passed)
			{
				this.lineOrigin.color = this._passedColor;
			}
		}

		// Token: 0x0600055C RID: 1372 RVA: 0x000172A4 File Offset: 0x000154A4
		public void SetLineStatus(MapLineWidget.LineStatus status)
		{
			this.Status = status;
			switch (this.Status)
			{
			case MapLineWidget.LineStatus.NotAcrossed:
			case MapLineWidget.LineStatus.Active:
				return;
			case MapLineWidget.LineStatus.Acrossing:
				this.StartLineTween(0.5f);
				return;
			case MapLineWidget.LineStatus.Acrossed:
				this.lineOrigin.fillAmount = 0f;
				this.linePass.fillAmount = 1f;
				return;
			case MapLineWidget.LineStatus.Passed:
				this.lineOrigin.color = this._passedColor;
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x0600055D RID: 1373 RVA: 0x00017320 File Offset: 0x00015520
		private void SetLineImage(bool acrossed)
		{
			if (acrossed)
			{
				this.lineOrigin.fillAmount = 0f;
				this.linePass.fillAmount = 1f;
				return;
			}
			this.lineOrigin.fillAmount = 1f;
			this.linePass.fillAmount = 0f;
		}

		// Token: 0x0600055E RID: 1374 RVA: 0x00017374 File Offset: 0x00015574
		private void StartLineTween(float time)
		{
			this.lineOrigin.DOFillAmount(0f, time).SetUpdate(true);
			this.crossingLineOrigin.fillAmount = 0f;
			this.crossingLineOrigin.DOFillAmount(1f, time).SetUpdate(true);
			this.linePass.DOFillAmount(1f, time).SetUpdate(true);
		}

		// Token: 0x0600055F RID: 1375 RVA: 0x000173D9 File Offset: 0x000155D9
		public void SetLineCrossing()
		{
			this.crossingLineOrigin.gameObject.SetActive(true);
			this.lineOrigin.gameObject.SetActive(false);
			this.linePass.gameObject.SetActive(false);
		}

		// Token: 0x04000318 RID: 792
		[SerializeField]
		private Image lineOrigin;

		// Token: 0x04000319 RID: 793
		[SerializeField]
		private Image crossingLineOrigin;

		// Token: 0x0400031A RID: 794
		[SerializeField]
		private Image linePass;

		// Token: 0x0400031B RID: 795
		[SerializeField]
		private AssociationList<Sprite, Sprite> lineSpritePairs;

		// Token: 0x0400031D RID: 797
		private readonly Color _passedColor = new Color(1f, 1f, 1f, 0.5f);

		// Token: 0x04000320 RID: 800
		private const float DefaultMinus = 120f;

		// Token: 0x04000321 RID: 801
		private const float BossMinus = 300f;

		// Token: 0x020001D8 RID: 472
		public enum LineStatus
		{
			// Token: 0x04000F1D RID: 3869
			NotAcrossed,
			// Token: 0x04000F1E RID: 3870
			Active,
			// Token: 0x04000F1F RID: 3871
			Acrossing,
			// Token: 0x04000F20 RID: 3872
			Acrossed,
			// Token: 0x04000F21 RID: 3873
			Passed
		}
	}
}
