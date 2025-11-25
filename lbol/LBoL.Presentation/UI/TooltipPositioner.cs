using System;
using UnityEngine;
namespace LBoL.Presentation.UI
{
	[ExecuteAlways]
	public sealed class TooltipPositioner : MonoBehaviour
	{
		public RectTransform TargetTransform { get; set; }
		public TooltipPosition[] TooltipPositions { get; set; }
		public Vector2 TooltipSize { get; set; }
		public float Gap { get; set; }
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
		private void Awake()
		{
			this._parentRectTrans = (RectTransform)base.transform.parent;
		}
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
		private RectTransform _parentRectTrans;
		private readonly Vector3[] _corners = new Vector3[4];
		private struct PlaceResult
		{
			public bool Fit;
			public Vector2 Position;
		}
	}
}
