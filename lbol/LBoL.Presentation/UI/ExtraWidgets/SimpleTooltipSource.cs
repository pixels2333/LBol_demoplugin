using System;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.UI.Extensions;
namespace LBoL.Presentation.UI.ExtraWidgets
{
	[RequireComponent(typeof(RectTransform))]
	public sealed class SimpleTooltipSource : TooltipSource
	{
		public override RectTransform TargetRectTransform
		{
			get
			{
				return (RectTransform)base.transform;
			}
		}
		public override TooltipPosition[] TooltipPositions
		{
			get
			{
				return this._positions;
			}
		}
		public override string Title
		{
			get
			{
				return this._contentGetter.Title;
			}
		}
		public override string Description
		{
			get
			{
				return this._contentGetter.Description;
			}
		}
		public override float Gap
		{
			get
			{
				return this._gap;
			}
		}
		public void SetDirect(string title, [CanBeNull] string description = null)
		{
			this._contentGetter = new SimpleTooltipSource.DirectContentGetter(title, description);
			base.Refresh();
		}
		public void SetWithGetter(Func<string> titleGetter, [CanBeNull] Func<string> descriptionGetter = null)
		{
			this._contentGetter = new SimpleTooltipSource.FuncContentGetter(titleGetter, descriptionGetter);
			base.Refresh();
		}
		public void SetWithGeneralKey(string titleKey, [CanBeNull] string descriptionKey = null)
		{
			this._contentGetter = new SimpleTooltipSource.LocalizedContentGetter(titleKey, descriptionKey, null);
			base.Refresh();
		}
		public void SetWithGeneralKeyAndArgs(string titleKey, [CanBeNull] string descriptionKey, params object[] args)
		{
			this._contentGetter = new SimpleTooltipSource.LocalizedContentGetter(titleKey, descriptionKey, args);
			base.Refresh();
		}
		public void SetWithGeneralKeyAndArgs(string titleKey, params object[] args)
		{
			this._contentGetter = new SimpleTooltipSource.LocalizedContentGetter(titleKey, null, args);
			base.Refresh();
		}
		public void SetWithTooltipKey(string tooltipKey)
		{
			this._contentGetter = new SimpleTooltipSource.LocalizedContentGetter("Tooltip." + tooltipKey + ".Name", "Tooltip." + tooltipKey + ".Description", null);
			base.Refresh();
		}
		public void SetWithTooltipKeyAndArgs(string tooltipKey, params object[] args)
		{
			this._contentGetter = new SimpleTooltipSource.LocalizedContentGetter("Tooltip." + tooltipKey + ".Name", "Tooltip." + tooltipKey + ".Description", args);
			base.Refresh();
		}
		public static SimpleTooltipSource CreateDirect(GameObject gameObject, string title, [CanBeNull] string description = null)
		{
			SimpleTooltipSource orAddComponent = gameObject.GetOrAddComponent<SimpleTooltipSource>();
			orAddComponent.SetDirect(title, description);
			return orAddComponent;
		}
		public static SimpleTooltipSource CreateWithGetter(GameObject gameObject, Func<string> titleGetter, [CanBeNull] Func<string> descriptionGetter = null)
		{
			SimpleTooltipSource orAddComponent = gameObject.GetOrAddComponent<SimpleTooltipSource>();
			orAddComponent.SetWithGetter(titleGetter, descriptionGetter);
			return orAddComponent;
		}
		public static SimpleTooltipSource CreateWithGeneralKey(GameObject gameObject, string titleKey, [CanBeNull] string descriptionKey = null)
		{
			SimpleTooltipSource orAddComponent = gameObject.GetOrAddComponent<SimpleTooltipSource>();
			orAddComponent.SetWithGeneralKey(titleKey, descriptionKey);
			return orAddComponent;
		}
		public static SimpleTooltipSource CreateWithGeneralKeyAndArgs(GameObject gameObject, string titleKey, [CanBeNull] string descriptionKey, params object[] args)
		{
			SimpleTooltipSource orAddComponent = gameObject.GetOrAddComponent<SimpleTooltipSource>();
			orAddComponent.SetWithGeneralKeyAndArgs(titleKey, descriptionKey, args);
			return orAddComponent;
		}
		public static SimpleTooltipSource CreateWithGeneralKeyAndArgs(GameObject gameObject, string titleKey, params object[] args)
		{
			SimpleTooltipSource orAddComponent = gameObject.GetOrAddComponent<SimpleTooltipSource>();
			orAddComponent.SetWithGeneralKeyAndArgs(titleKey, args);
			return orAddComponent;
		}
		public static SimpleTooltipSource CreateWithTooltipKey(GameObject gameObject, string tooltipKey)
		{
			SimpleTooltipSource orAddComponent = gameObject.GetOrAddComponent<SimpleTooltipSource>();
			orAddComponent.SetWithTooltipKey(tooltipKey);
			return orAddComponent;
		}
		public static SimpleTooltipSource CreateWithTooltipKeyAndArgs(GameObject gameObject, string tooltipKey, params object[] args)
		{
			SimpleTooltipSource orAddComponent = gameObject.GetOrAddComponent<SimpleTooltipSource>();
			orAddComponent.SetWithTooltipKeyAndArgs(tooltipKey, args);
			return orAddComponent;
		}
		public SimpleTooltipSource WithPosition(TooltipDirection direction, TooltipAlignment alignment)
		{
			this._positions[0] = new TooltipPosition(direction, alignment);
			return this;
		}
		public SimpleTooltipSource WithGap(float gap)
		{
			this._gap = gap;
			return this;
		}
		private void Awake()
		{
			if (!this.titleKey.IsNullOrEmpty())
			{
				if (this.isTooltip)
				{
					this.SetWithTooltipKey(this.titleKey);
				}
				else if (this.descriptionKey.IsNullOrEmpty())
				{
					this.SetWithGeneralKey(this.titleKey, null);
				}
				else
				{
					this.SetWithGeneralKey(this.titleKey, this.descriptionKey);
				}
				if (this.manualPosition)
				{
					this.WithPosition(this.direction, this.alignment);
				}
			}
		}
		private readonly TooltipPosition[] _positions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Max)
		};
		private SimpleTooltipSource.IContentGetter _contentGetter;
		private float _gap = 10f;
		[SerializeField]
		private string titleKey;
		[SerializeField]
		private string descriptionKey;
		[SerializeField]
		private bool isTooltip;
		[SerializeField]
		private bool manualPosition;
		[SerializeField]
		private TooltipDirection direction;
		[SerializeField]
		private TooltipAlignment alignment;
		private interface IContentGetter
		{
			string Title { get; }
			[CanBeNull]
			string Description { get; }
		}
		private sealed class DirectContentGetter : SimpleTooltipSource.IContentGetter
		{
			public string Title { get; }
			public string Description { get; }
			public DirectContentGetter(string title, [CanBeNull] string description)
			{
				this.Title = title;
				this.Description = description;
			}
		}
		private sealed class LocalizedContentGetter : SimpleTooltipSource.IContentGetter
		{
			public string Title
			{
				get
				{
					if (this._args != null)
					{
						return this._titleKey.LocalizeFormat(this._args);
					}
					return this._titleKey.Localize(true);
				}
			}
			public string Description
			{
				get
				{
					if (this._args != null)
					{
						string descriptionKey = this._descriptionKey;
						if (descriptionKey == null)
						{
							return null;
						}
						return descriptionKey.LocalizeFormat(this._args);
					}
					else
					{
						string descriptionKey2 = this._descriptionKey;
						if (descriptionKey2 == null)
						{
							return null;
						}
						return descriptionKey2.Localize(true);
					}
				}
			}
			public LocalizedContentGetter(string titleKey, [CanBeNull] string descriptionKey, [CanBeNull] object[] args)
			{
				this._titleKey = titleKey;
				this._descriptionKey = descriptionKey;
				this._args = args;
			}
			private readonly string _titleKey;
			[CanBeNull]
			private readonly string _descriptionKey;
			[CanBeNull]
			private readonly object[] _args;
		}
		private sealed class FuncContentGetter : SimpleTooltipSource.IContentGetter
		{
			public string Title
			{
				get
				{
					return this._titleGetter.Invoke();
				}
			}
			public string Description
			{
				get
				{
					Func<string> descriptionGetter = this._descriptionGetter;
					if (descriptionGetter == null)
					{
						return null;
					}
					return descriptionGetter.Invoke();
				}
			}
			public FuncContentGetter(Func<string> titleGetter, [CanBeNull] Func<string> descriptionGetter)
			{
				this._titleGetter = titleGetter;
				this._descriptionGetter = descriptionGetter;
			}
			private readonly Func<string> _titleGetter;
			[CanBeNull]
			private readonly Func<string> _descriptionGetter;
		}
	}
}
