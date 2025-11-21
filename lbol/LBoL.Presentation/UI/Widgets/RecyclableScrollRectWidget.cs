using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200006C RID: 108
	public class RecyclableScrollRectWidget : MonoBehaviour
	{
		// Token: 0x060005C8 RID: 1480 RVA: 0x00018DD8 File Offset: 0x00016FD8
		public void ReloadData(RecyclableScrollRectWidget.IRecyclableScrollRectDataSource d)
		{
			if (this._cellsPool == null)
			{
				this.InitCellPool();
			}
			this.scrollRect.StopMovement();
			this._dataSource = d;
			foreach (int num in this._cellsDic.Keys)
			{
				this._cellsPool.Release(this._cellsDic[num]);
			}
			this._cellsDic.Clear();
			this.scrollRect.content.sizeDelta = new Vector2(this.scrollRect.viewport.rect.width, this.prototypeCell.rect.height * (float)this._dataSource.GetItemCount());
			int num2 = 0;
			while (num2 < this.minPoolSize && num2 != this._dataSource.GetItemCount())
			{
				RecyclableScrollRectWidget.RecyclableCell recyclableCell = this._cellsPool.Get();
				this._cellsDic.Add(num2, recyclableCell);
				recyclableCell.transform.SetParent(this.scrollRect.content, false);
				recyclableCell.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, (float)(-(float)num2) * this.prototypeCell.rect.height);
				this._dataSource.SetCell(recyclableCell.GetComponent<RecyclableScrollRectWidget.RecyclableCell>(), num2);
				num2++;
			}
			this._lastScrollRectVec = this.scrollRect.normalizedPosition;
			this._lastScrollRectTime = Time.time;
		}

		// Token: 0x060005C9 RID: 1481 RVA: 0x00018F70 File Offset: 0x00017170
		public void OnValueChangedListener(Vector2 vec)
		{
			if (this._dataSource == null)
			{
				return;
			}
			float num = Math.Max(0f, (1f - vec.y) * (float)this._dataSource.GetItemCount() - this.recyclingThreshold * (float)this.minPoolSize);
			float num2 = Math.Min((float)this._dataSource.GetItemCount(), num + (float)this.minPoolSize);
			for (int i = 0; i < this._dataSource.GetItemCount(); i++)
			{
				if (this._cellsDic.ContainsKey(i) && ((float)i < num || (float)i >= num2))
				{
					this._cellsPool.Release(this._cellsDic[i]);
					this._cellsDic.Remove(i);
				}
				if (!this._cellsDic.ContainsKey(i) && (float)i >= num && (float)i < num2)
				{
					RecyclableScrollRectWidget.RecyclableCell recyclableCell = this._cellsPool.Get();
					this._cellsDic.Add(i, recyclableCell);
					recyclableCell.transform.SetParent(this.scrollRect.content, false);
					recyclableCell.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, (float)(-(float)i) * this.prototypeCell.rect.height);
					this._dataSource.SetCell(recyclableCell.GetComponent<RecyclableScrollRectWidget.RecyclableCell>(), i);
				}
			}
			this._lastScrollRectVec = vec;
			this._lastScrollRectTime = Time.time;
		}

		// Token: 0x060005CA RID: 1482 RVA: 0x000190C8 File Offset: 0x000172C8
		private void Awake()
		{
			if (this.scrollRect == null)
			{
				this.scrollRect = base.gameObject.AddComponent<ScrollRect>();
			}
			this.scrollRect.onValueChanged.AddListener(new UnityAction<Vector2>(this.OnValueChangedListener));
			this.prototypeCell.gameObject.SetActive(false);
			this.scrollRect.horizontal = false;
			this.scrollRect.vertical = true;
			if (this.scrollRect.verticalScrollbar)
			{
				this.scrollRect.verticalScrollbar.value = 1f;
			}
			if (this._cellsPool == null)
			{
				this.InitCellPool();
			}
		}

		// Token: 0x060005CB RID: 1483 RVA: 0x00019170 File Offset: 0x00017370
		private void InitCellPool()
		{
			int i = 0;
			this._cellsPool = new ObjectPool<RecyclableScrollRectWidget.RecyclableCell>(delegate
			{
				GameObject gameObject = Object.Instantiate<RectTransform>(this.prototypeCell, this.scrollRect.content.transform).gameObject;
				gameObject.SetActive(true);
				string text = "Cell {0}";
				int num = i + 1;
				i = num;
				gameObject.name = string.Format(text, num);
				return gameObject.GetComponent<RecyclableScrollRectWidget.RecyclableCell>();
			}, delegate(RecyclableScrollRectWidget.RecyclableCell widget)
			{
				widget.actionOnGet();
			}, delegate(RecyclableScrollRectWidget.RecyclableCell widget)
			{
				widget.actionOnRelease();
			}, delegate(RecyclableScrollRectWidget.RecyclableCell widget)
			{
				widget.actionOnDestroy();
			}, true, 10, this.minPoolSize * 2);
		}

		// Token: 0x060005CC RID: 1484 RVA: 0x00019210 File Offset: 0x00017410
		public void ShowWithDelay()
		{
			foreach (KeyValuePair<int, RecyclableScrollRectWidget.RecyclableCell> keyValuePair in this._cellsDic)
			{
				int num;
				RecyclableScrollRectWidget.RecyclableCell recyclableCell;
				keyValuePair.Deconstruct(ref num, ref recyclableCell);
				int num2 = num;
				recyclableCell.ShowWithDelay((float)num2 * 0.2f);
			}
		}

		// Token: 0x04000379 RID: 889
		private RecyclableScrollRectWidget.IRecyclableScrollRectDataSource _dataSource;

		// Token: 0x0400037A RID: 890
		public int minPoolSize = 10;

		// Token: 0x0400037B RID: 891
		public float recyclingThreshold = 0.2f;

		// Token: 0x0400037C RID: 892
		public RectTransform prototypeCell;

		// Token: 0x0400037D RID: 893
		public ScrollRect scrollRect;

		// Token: 0x0400037E RID: 894
		private readonly Dictionary<int, RecyclableScrollRectWidget.RecyclableCell> _cellsDic = new Dictionary<int, RecyclableScrollRectWidget.RecyclableCell>();

		// Token: 0x0400037F RID: 895
		private ObjectPool<RecyclableScrollRectWidget.RecyclableCell> _cellsPool;

		// Token: 0x04000380 RID: 896
		private Vector2 _lastScrollRectVec;

		// Token: 0x04000381 RID: 897
		private float _lastScrollRectTime;

		// Token: 0x020001DA RID: 474
		public interface IRecyclableScrollRectDataSource
		{
			// Token: 0x06001347 RID: 4935
			int GetItemCount();

			// Token: 0x06001348 RID: 4936
			void SetCell(RecyclableScrollRectWidget.RecyclableCell cell, int index);
		}

		// Token: 0x020001DB RID: 475
		public abstract class RecyclableCell : MonoBehaviour
		{
			// Token: 0x06001349 RID: 4937
			public abstract void actionOnGet();

			// Token: 0x0600134A RID: 4938
			public abstract void actionOnRelease();

			// Token: 0x0600134B RID: 4939
			public abstract void actionOnDestroy();

			// Token: 0x0600134C RID: 4940
			public abstract void ShowWithDelay(float delay);
		}
	}
}
