using System;
using UnityEngine;

namespace LBoL.Presentation.UI
{
	// Token: 0x0200002C RID: 44
	[ExecuteAlways]
	public sealed class TooltipPositioner : MonoBehaviour
	{
		// Token: 0x17000086 RID: 134
		// (get) Token: 0x06000334 RID: 820 RVA: 0x0000DF0F File Offset: 0x0000C10F
		// (set) Token: 0x06000335 RID: 821 RVA: 0x0000DF17 File Offset: 0x0000C117
		public RectTransform TargetTransform { get; set; }

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x06000336 RID: 822 RVA: 0x0000DF20 File Offset: 0x0000C120
		// (set) Token: 0x06000337 RID: 823 RVA: 0x0000DF28 File Offset: 0x0000C128
		public TooltipPosition[] TooltipPositions { get; set; }

		// Token: 0x17000088 RID: 136
		// (get) Token: 0x06000338 RID: 824 RVA: 0x0000DF31 File Offset: 0x0000C131
		// (set) Token: 0x06000339 RID: 825 RVA: 0x0000DF39 File Offset: 0x0000C139
		public Vector2 TooltipSize { get; set; }

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x0600033A RID: 826 RVA: 0x0000DF42 File Offset: 0x0000C142
		// (set) Token: 0x0600033B RID: 827 RVA: 0x0000DF4A File Offset: 0x0000C14A
		public float Gap { get; set; }

		// Token: 0x0600033C RID: 828 RVA: 0x0000DF54 File Offset: 0x0000C154
		private Vector2 GetAlignedPositionX(TooltipAlignment alignment, bool top)
		{
			Vector2 vector;
			if (top)
			{
				float num = (this._corners[1].y + this._corners[2].y) / 2f + this.TooltipSize.y / 2f + this.Gap;
				switch (alignment)
				{
				case TooltipAlignment.Min:
					vector = new Vector2(this._corners[1].x + this.TooltipSize.x / 2f, num);
					break;
				case TooltipAlignment.Center:
					vector = new Vector2((this._corners[1].x + this._corners[2].x) / 2f, num);
					break;
				case TooltipAlignment.Max:
					vector = new Vector2(this._corners[2].x - this.TooltipSize.x / 2f, num);
					break;
				default:
					throw new ArgumentOutOfRangeException("alignment", alignment, null);
				}
				return vector;
			}
			float num2 = (this._corners[0].y + this._corners[3].y) / 2f - this.TooltipSize.y / 2f - this.Gap;
			switch (alignment)
			{
			case TooltipAlignment.Min:
				vector = new Vector2(this._corners[0].x + this.TooltipSize.x / 2f, num2);
				break;
			case TooltipAlignment.Center:
				vector = new Vector2((this._corners[0].x + this._corners[3].x) / 2f, num2);
				break;
			case TooltipAlignment.Max:
				vector = new Vector2(this._corners[3].x - this.TooltipSize.x / 2f, num2);
				break;
			default:
				throw new ArgumentOutOfRangeException("alignment", alignment, null);
			}
			return vector;
		}

		// Token: 0x0600033D RID: 829 RVA: 0x0000E158 File Offset: 0x0000C358
		private Vector2 GetAlignedPositionY(TooltipAlignment alignment, bool left)
		{
			Vector2 vector;
			if (left)
			{
				float num = (this._corners[0].x + this._corners[1].x) / 2f - this.TooltipSize.x / 2f - this.Gap;
				switch (alignment)
				{
				case TooltipAlignment.Min:
					vector = new Vector2(num, this._corners[0].y + this.TooltipSize.y / 2f);
					break;
				case TooltipAlignment.Center:
					vector = new Vector2(num, (this._corners[0].y + this._corners[1].y) / 2f);
					break;
				case TooltipAlignment.Max:
					vector = new Vector2(num, this._corners[1].y - this.TooltipSize.y / 2f);
					break;
				default:
					throw new ArgumentOutOfRangeException("alignment", alignment, null);
				}
				return vector;
			}
			float num2 = (this._corners[2].x + this._corners[3].x) / 2f + this.TooltipSize.x / 2f + this.Gap;
			switch (alignment)
			{
			case TooltipAlignment.Min:
				vector = new Vector2(num2, this._corners[3].y + this.TooltipSize.y / 2f);
				break;
			case TooltipAlignment.Center:
				vector = new Vector2(num2, (this._corners[3].y + this._corners[2].y) / 2f);
				break;
			case TooltipAlignment.Max:
				vector = new Vector2(num2, this._corners[2].y - this.TooltipSize.y / 2f);
				break;
			default:
				throw new ArgumentOutOfRangeException("alignment", alignment, null);
			}
			return vector;
		}

		// Token: 0x0600033E RID: 830 RVA: 0x0000E35C File Offset: 0x0000C55C
		private TooltipPositioner.PlaceResult TryPlaceTooltip(Rect rect, TooltipPosition position)
		{
			Vector2 vector;
			switch (position.Direction)
			{
			case TooltipDirection.Left:
				vector = this.GetAlignedPositionY(position.Alignment, true);
				break;
			case TooltipDirection.Top:
				vector = this.GetAlignedPositionX(position.Alignment, true);
				break;
			case TooltipDirection.Right:
				vector = this.GetAlignedPositionY(position.Alignment, false);
				break;
			case TooltipDirection.Bottom:
				vector = this.GetAlignedPositionX(position.Alignment, false);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			Vector2 vector2 = vector;
			bool flag = vector2.x - this.TooltipSize.x / 2f >= rect.xMin && vector2.x + this.TooltipSize.x / 2f <= rect.xMax && vector2.y - this.TooltipSize.y / 2f >= rect.yMin && vector2.y + this.TooltipSize.y / 2f <= rect.yMax;
			return new TooltipPositioner.PlaceResult
			{
				Position = vector2,
				Fit = flag
			};
		}

		// Token: 0x0600033F RID: 831 RVA: 0x0000E475 File Offset: 0x0000C675
		private void Awake()
		{
			this._parentRectTrans = (RectTransform)base.transform.parent;
		}

		// Token: 0x06000340 RID: 832 RVA: 0x0000E490 File Offset: 0x0000C690
		private void LateUpdate()
		{
			if (this.TooltipPositions == null)
			{
				Debug.LogError(base.name + " has no TooltipPositions");
				Object.Destroy(this);
				return;
			}
			RectTransform targetTransform = this.TargetTransform;
			if (targetTransform)
			{
				this.ForceUpdateTo(targetTransform);
				return;
			}
			Object.Destroy(this);
		}

		// Token: 0x06000341 RID: 833 RVA: 0x0000E4E0 File Offset: 0x0000C6E0
		public void ForceUpdateTo(RectTransform trans)
		{
			Rect rect = this._parentRectTrans.rect;
			TooltipPositioner.PlaceResult placeResult = default(TooltipPositioner.PlaceResult);
			trans.GetWorldCorners(this._corners);
			Matrix4x4 worldToLocalMatrix = this._parentRectTrans.worldToLocalMatrix;
			for (int i = 0; i < 4; i++)
			{
				this._corners[i] = worldToLocalMatrix.MultiplyPoint(this._corners[i]);
			}
			foreach (TooltipPosition tooltipPosition in this.TooltipPositions)
			{
				placeResult = this.TryPlaceTooltip(rect, tooltipPosition);
				if (placeResult.Fit)
				{
					break;
				}
			}
			base.transform.localPosition = placeResult.Position;
		}

		// Token: 0x04000178 RID: 376
		private RectTransform _parentRectTrans;

		// Token: 0x0400017D RID: 381
		private readonly Vector3[] _corners = new Vector3[4];

		// Token: 0x020001C0 RID: 448
		private struct PlaceResult
		{
			// Token: 0x04000EB9 RID: 3769
			public bool Fit;

			// Token: 0x04000EBA RID: 3770
			public Vector2 Position;
		}
	}
}
