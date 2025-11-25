using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class RecyclableScrollRectWidget : MonoBehaviour
	{
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
		private RecyclableScrollRectWidget.IRecyclableScrollRectDataSource _dataSource;
		public int minPoolSize = 10;
		public float recyclingThreshold = 0.2f;
		public RectTransform prototypeCell;
		public ScrollRect scrollRect;
		private readonly Dictionary<int, RecyclableScrollRectWidget.RecyclableCell> _cellsDic = new Dictionary<int, RecyclableScrollRectWidget.RecyclableCell>();
		private ObjectPool<RecyclableScrollRectWidget.RecyclableCell> _cellsPool;
		private Vector2 _lastScrollRectVec;
		private float _lastScrollRectTime;
		public interface IRecyclableScrollRectDataSource
		{
			int GetItemCount();
			void SetCell(RecyclableScrollRectWidget.RecyclableCell cell, int index);
		}
		public abstract class RecyclableCell : MonoBehaviour
		{
			public abstract void actionOnGet();
			public abstract void actionOnRelease();
			public abstract void actionOnDestroy();
			public abstract void ShowWithDelay(float delay);
		}
	}
}
