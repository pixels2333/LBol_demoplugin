using System;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000D5 RID: 213
	[RequireComponent(typeof(RectTransform))]
	public sealed class SimpleTooltipSource : TooltipSource
	{
		// Token: 0x1700021C RID: 540
		// (get) Token: 0x06000CF2 RID: 3314 RVA: 0x000405EA File Offset: 0x0003E7EA
		public override RectTransform TargetRectTransform
		{
			get
			{
				return (RectTransform)base.transform;
			}
		}

		// Token: 0x1700021D RID: 541
		// (get) Token: 0x06000CF3 RID: 3315 RVA: 0x000405F7 File Offset: 0x0003E7F7
		public override TooltipPosition[] TooltipPositions
		{
			get
			{
				return this._positions;
			}
		}

		// Token: 0x1700021E RID: 542
		// (get) Token: 0x06000CF4 RID: 3316 RVA: 0x000405FF File Offset: 0x0003E7FF
		public override string Title
		{
			get
			{
				return this._contentGetter.Title;
			}
		}

		// Token: 0x1700021F RID: 543
		// (get) Token: 0x06000CF5 RID: 3317 RVA: 0x0004060C File Offset: 0x0003E80C
		public override string Description
		{
			get
			{
				return this._contentGetter.Description;
			}
		}

		// Token: 0x17000220 RID: 544
		// (get) Token: 0x06000CF6 RID: 3318 RVA: 0x00040619 File Offset: 0x0003E819
		public override float Gap
		{
			get
			{
				return this._gap;
			}
		}

		// Token: 0x06000CF7 RID: 3319 RVA: 0x00040621 File Offset: 0x0003E821
		public void SetDirect(string title, [CanBeNull] string description = null)
		{
			this._contentGetter = new SimpleTooltipSource.DirectContentGetter(title, description);
			base.Refresh();
		}

		// Token: 0x06000CF8 RID: 3320 RVA: 0x00040636 File Offset: 0x0003E836
		public void SetWithGetter(Func<string> titleGetter, [CanBeNull] Func<string> descriptionGetter = null)
		{
			this._contentGetter = new SimpleTooltipSource.FuncContentGetter(titleGetter, descriptionGetter);
			base.Refresh();
		}

		// Token: 0x06000CF9 RID: 3321 RVA: 0x0004064B File Offset: 0x0003E84B
		public void SetWithGeneralKey(string titleKey, [CanBeNull] string descriptionKey = null)
		{
			this._contentGetter = new SimpleTooltipSource.LocalizedContentGetter(titleKey, descriptionKey, null);
			base.Refresh();
		}

		// Token: 0x06000CFA RID: 3322 RVA: 0x00040661 File Offset: 0x0003E861
		public void SetWithGeneralKeyAndArgs(string titleKey, [CanBeNull] string descriptionKey, params object[] args)
		{
			this._contentGetter = new SimpleTooltipSource.LocalizedContentGetter(titleKey, descriptionKey, args);
			base.Refresh();
		}

		// Token: 0x06000CFB RID: 3323 RVA: 0x00040677 File Offset: 0x0003E877
		public void SetWithGeneralKeyAndArgs(string titleKey, params object[] args)
		{
			this._contentGetter = new SimpleTooltipSource.LocalizedContentGetter(titleKey, null, args);
			base.Refresh();
		}

		// Token: 0x06000CFC RID: 3324 RVA: 0x0004068D File Offset: 0x0003E88D
		public void SetWithTooltipKey(string tooltipKey)
		{
			this._contentGetter = new SimpleTooltipSource.LocalizedContentGetter("Tooltip." + tooltipKey + ".Name", "Tooltip." + tooltipKey + ".Description", null);
			base.Refresh();
		}

		// Token: 0x06000CFD RID: 3325 RVA: 0x000406C1 File Offset: 0x0003E8C1
		public void SetWithTooltipKeyAndArgs(string tooltipKey, params object[] args)
		{
			this._contentGetter = new SimpleTooltipSource.LocalizedContentGetter("Tooltip." + tooltipKey + ".Name", "Tooltip." + tooltipKey + ".Description", args);
			base.Refresh();
		}

		// Token: 0x06000CFE RID: 3326 RVA: 0x000406F5 File Offset: 0x0003E8F5
		public static SimpleTooltipSource CreateDirect(GameObject gameObject, string title, [CanBeNull] string description = null)
		{
			SimpleTooltipSource orAddComponent = gameObject.GetOrAddComponent<SimpleTooltipSource>();
			orAddComponent.SetDirect(title, description);
			return orAddComponent;
		}

		// Token: 0x06000CFF RID: 3327 RVA: 0x00040705 File Offset: 0x0003E905
		public static SimpleTooltipSource CreateWithGetter(GameObject gameObject, Func<string> titleGetter, [CanBeNull] Func<string> descriptionGetter = null)
		{
			SimpleTooltipSource orAddComponent = gameObject.GetOrAddComponent<SimpleTooltipSource>();
			orAddComponent.SetWithGetter(titleGetter, descriptionGetter);
			return orAddComponent;
		}

		// Token: 0x06000D00 RID: 3328 RVA: 0x00040715 File Offset: 0x0003E915
		public static SimpleTooltipSource CreateWithGeneralKey(GameObject gameObject, string titleKey, [CanBeNull] string descriptionKey = null)
		{
			SimpleTooltipSource orAddComponent = gameObject.GetOrAddComponent<SimpleTooltipSource>();
			orAddComponent.SetWithGeneralKey(titleKey, descriptionKey);
			return orAddComponent;
		}

		// Token: 0x06000D01 RID: 3329 RVA: 0x00040725 File Offset: 0x0003E925
		public static SimpleTooltipSource CreateWithGeneralKeyAndArgs(GameObject gameObject, string titleKey, [CanBeNull] string descriptionKey, params object[] args)
		{
			SimpleTooltipSource orAddComponent = gameObject.GetOrAddComponent<SimpleTooltipSource>();
			orAddComponent.SetWithGeneralKeyAndArgs(titleKey, descriptionKey, args);
			return orAddComponent;
		}

		// Token: 0x06000D02 RID: 3330 RVA: 0x00040736 File Offset: 0x0003E936
		public static SimpleTooltipSource CreateWithGeneralKeyAndArgs(GameObject gameObject, string titleKey, params object[] args)
		{
			SimpleTooltipSource orAddComponent = gameObject.GetOrAddComponent<SimpleTooltipSource>();
			orAddComponent.SetWithGeneralKeyAndArgs(titleKey, args);
			return orAddComponent;
		}

		// Token: 0x06000D03 RID: 3331 RVA: 0x00040746 File Offset: 0x0003E946
		public static SimpleTooltipSource CreateWithTooltipKey(GameObject gameObject, string tooltipKey)
		{
			SimpleTooltipSource orAddComponent = gameObject.GetOrAddComponent<SimpleTooltipSource>();
			orAddComponent.SetWithTooltipKey(tooltipKey);
			return orAddComponent;
		}

		// Token: 0x06000D04 RID: 3332 RVA: 0x00040755 File Offset: 0x0003E955
		public static SimpleTooltipSource CreateWithTooltipKeyAndArgs(GameObject gameObject, string tooltipKey, params object[] args)
		{
			SimpleTooltipSource orAddComponent = gameObject.GetOrAddComponent<SimpleTooltipSource>();
			orAddComponent.SetWithTooltipKeyAndArgs(tooltipKey, args);
			return orAddComponent;
		}

		// Token: 0x06000D05 RID: 3333 RVA: 0x00040765 File Offset: 0x0003E965
		public SimpleTooltipSource WithPosition(TooltipDirection direction, TooltipAlignment alignment)
		{
			this._positions[0] = new TooltipPosition(direction, alignment);
			return this;
		}

		// Token: 0x06000D06 RID: 3334 RVA: 0x00040777 File Offset: 0x0003E977
		public SimpleTooltipSource WithGap(float gap)
		{
			this._gap = gap;
			return this;
		}

		// Token: 0x06000D07 RID: 3335 RVA: 0x00040784 File Offset: 0x0003E984
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

		// Token: 0x040009DB RID: 2523
		private readonly TooltipPosition[] _positions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Max)
		};

		// Token: 0x040009DC RID: 2524
		private SimpleTooltipSource.IContentGetter _contentGetter;

		// Token: 0x040009DD RID: 2525
		private float _gap = 10f;

		// Token: 0x040009DE RID: 2526
		[SerializeField]
		private string titleKey;

		// Token: 0x040009DF RID: 2527
		[SerializeField]
		private string descriptionKey;

		// Token: 0x040009E0 RID: 2528
		[SerializeField]
		private bool isTooltip;

		// Token: 0x040009E1 RID: 2529
		[SerializeField]
		private bool manualPosition;

		// Token: 0x040009E2 RID: 2530
		[SerializeField]
		private TooltipDirection direction;

		// Token: 0x040009E3 RID: 2531
		[SerializeField]
		private TooltipAlignment alignment;

		// Token: 0x0200030C RID: 780
		private interface IContentGetter
		{
			// Token: 0x170004C7 RID: 1223
			// (get) Token: 0x06001839 RID: 6201
			string Title { get; }

			// Token: 0x170004C8 RID: 1224
			// (get) Token: 0x0600183A RID: 6202
			[CanBeNull]
			string Description { get; }
		}

		// Token: 0x0200030D RID: 781
		private sealed class DirectContentGetter : SimpleTooltipSource.IContentGetter
		{
			// Token: 0x170004C9 RID: 1225
			// (get) Token: 0x0600183B RID: 6203 RVA: 0x0006AEE1 File Offset: 0x000690E1
			public string Title { get; }

			// Token: 0x170004CA RID: 1226
			// (get) Token: 0x0600183C RID: 6204 RVA: 0x0006AEE9 File Offset: 0x000690E9
			public string Description { get; }

			// Token: 0x0600183D RID: 6205 RVA: 0x0006AEF4 File Offset: 0x000690F4
			public DirectContentGetter(string title, [CanBeNull] string description)
			{
				this.Title = title;
				this.Description = description;
			}
		}

		// Token: 0x0200030E RID: 782
		private sealed class LocalizedContentGetter : SimpleTooltipSource.IContentGetter
		{
			// Token: 0x170004CB RID: 1227
			// (get) Token: 0x0600183E RID: 6206 RVA: 0x0006AF19 File Offset: 0x00069119
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

			// Token: 0x170004CC RID: 1228
			// (get) Token: 0x0600183F RID: 6207 RVA: 0x0006AF41 File Offset: 0x00069141
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

			// Token: 0x06001840 RID: 6208 RVA: 0x0006AF78 File Offset: 0x00069178
			public LocalizedContentGetter(string titleKey, [CanBeNull] string descriptionKey, [CanBeNull] object[] args)
			{
				this._titleKey = titleKey;
				this._descriptionKey = descriptionKey;
				this._args = args;
			}

			// Token: 0x0400133B RID: 4923
			private readonly string _titleKey;

			// Token: 0x0400133C RID: 4924
			[CanBeNull]
			private readonly string _descriptionKey;

			// Token: 0x0400133D RID: 4925
			[CanBeNull]
			private readonly object[] _args;
		}

		// Token: 0x0200030F RID: 783
		private sealed class FuncContentGetter : SimpleTooltipSource.IContentGetter
		{
			// Token: 0x170004CD RID: 1229
			// (get) Token: 0x06001841 RID: 6209 RVA: 0x0006AFA6 File Offset: 0x000691A6
			public string Title
			{
				get
				{
					return this._titleGetter.Invoke();
				}
			}

			// Token: 0x170004CE RID: 1230
			// (get) Token: 0x06001842 RID: 6210 RVA: 0x0006AFB3 File Offset: 0x000691B3
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

			// Token: 0x06001843 RID: 6211 RVA: 0x0006AFC8 File Offset: 0x000691C8
			public FuncContentGetter(Func<string> titleGetter, [CanBeNull] Func<string> descriptionGetter)
			{
				this._titleGetter = titleGetter;
				this._descriptionGetter = descriptionGetter;
			}

			// Token: 0x0400133E RID: 4926
			private readonly Func<string> _titleGetter;

			// Token: 0x0400133F RID: 4927
			[CanBeNull]
			private readonly Func<string> _descriptionGetter;
		}
	}
}
