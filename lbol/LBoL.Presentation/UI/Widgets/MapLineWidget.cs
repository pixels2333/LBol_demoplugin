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
	public class MapLineWidget : MonoBehaviour
	{
		public MapLineWidget.LineStatus Status { get; private set; }
		public MapNodeWidget SourceWidget { get; private set; }
		public MapNodeWidget TargetWidget { get; private set; }
		public int SourceX
		{
			get
			{
				return this.SourceWidget.X;
			}
		}
		public int SourceY
		{
			get
			{
				return this.SourceWidget.Y;
			}
		}
		public int TargetX
		{
			get
			{
				return this.TargetWidget.X;
			}
		}
		public int TargetY
		{
			get
			{
				return this.TargetWidget.Y;
			}
		}
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
		private void StartLineTween(float time)
		{
			this.lineOrigin.DOFillAmount(0f, time).SetUpdate(true);
			this.crossingLineOrigin.fillAmount = 0f;
			this.crossingLineOrigin.DOFillAmount(1f, time).SetUpdate(true);
			this.linePass.DOFillAmount(1f, time).SetUpdate(true);
		}
		public void SetLineCrossing()
		{
			this.crossingLineOrigin.gameObject.SetActive(true);
			this.lineOrigin.gameObject.SetActive(false);
			this.linePass.gameObject.SetActive(false);
		}
		[SerializeField]
		private Image lineOrigin;
		[SerializeField]
		private Image crossingLineOrigin;
		[SerializeField]
		private Image linePass;
		[SerializeField]
		private AssociationList<Sprite, Sprite> lineSpritePairs;
		private readonly Color _passedColor = new Color(1f, 1f, 1f, 0.5f);
		private const float DefaultMinus = 120f;
		private const float BossMinus = 300f;
		public enum LineStatus
		{
			NotAcrossed,
			Active,
			Acrossing,
			Acrossed,
			Passed
		}
	}
}
