using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LBoL.Core;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI
{
	// Token: 0x02000022 RID: 34
	public class LocalizationManager : MonoBehaviour
	{
		// Token: 0x17000080 RID: 128
		// (get) Token: 0x06000314 RID: 788 RVA: 0x0000D60F File Offset: 0x0000B80F
		// (set) Token: 0x06000315 RID: 789 RVA: 0x0000D616 File Offset: 0x0000B816
		public static LocalizationManager Instance { get; private set; }

		// Token: 0x06000316 RID: 790 RVA: 0x0000D61E File Offset: 0x0000B81E
		private void Awake()
		{
			if (LocalizationManager.Instance)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			LocalizationManager.Instance = this;
		}

		// Token: 0x06000317 RID: 791 RVA: 0x0000D640 File Offset: 0x0000B840
		public Material CopyFontMatStyle(Material written, Material read)
		{
			written.SetColor(LocalizationManager.FaceColor, read.GetColor(LocalizationManager.FaceColor));
			written.SetFloat(LocalizationManager.FaceDilate, read.GetFloat(LocalizationManager.FaceDilate));
			written.SetColor(LocalizationManager.OutlineColor, read.GetColor(LocalizationManager.OutlineColor));
			written.SetFloat(LocalizationManager.OutlineWidth, read.GetFloat(LocalizationManager.OutlineWidth));
			written.SetFloat(LocalizationManager.OutlineSoftness, read.GetFloat(LocalizationManager.OutlineSoftness));
			if (read.IsKeywordEnabled("UNDERLAY_ON"))
			{
				written.EnableKeyword("UNDERLAY_ON");
				written.SetColor(LocalizationManager.UnderlayColor, read.GetColor(LocalizationManager.UnderlayColor));
				written.SetFloat(LocalizationManager.UnderlayOffsetX, read.GetFloat(LocalizationManager.UnderlayOffsetX));
				written.SetFloat(LocalizationManager.UnderlayOffsetY, read.GetFloat(LocalizationManager.UnderlayOffsetY));
				written.SetFloat(LocalizationManager.UnderlayDilate, read.GetFloat(LocalizationManager.UnderlayDilate));
				written.SetFloat(LocalizationManager.UnderlaySoftness, read.GetFloat(LocalizationManager.UnderlaySoftness));
			}
			if (read.IsKeywordEnabled("GLOW_ON"))
			{
				written.EnableKeyword("GLOW_ON");
				written.SetColor(LocalizationManager.GlowColor, read.GetColor(LocalizationManager.GlowColor));
				written.SetFloat(LocalizationManager.GlowOffset, read.GetFloat(LocalizationManager.GlowOffset));
				written.SetFloat(LocalizationManager.GlowInner, read.GetFloat(LocalizationManager.GlowInner));
				written.SetFloat(LocalizationManager.GlowOuter, read.GetFloat(LocalizationManager.GlowOuter));
				written.SetFloat(LocalizationManager.GlowPower, read.GetFloat(LocalizationManager.GlowPower));
			}
			return written;
		}

		// Token: 0x06000318 RID: 792 RVA: 0x0000D7C8 File Offset: 0x0000B9C8
		[return: TupleElementNames(new string[] { "font", "mat", "resize" })]
		public ValueTuple<TMP_FontAsset, Material, float>? GetCurrentLocaleFontAndMaterial(TMP_FontAsset originFont, Material originMaterial)
		{
			Locale locale = Localization.CurrentLocale;
			LocaleFontReplaceEntry localeFontReplaceEntry = Enumerable.FirstOrDefault<LocaleFontReplaceEntry>(this.localeFontReplaceTable, (LocaleFontReplaceEntry e) => e.font == originFont && e.material.name == originMaterial.name.Replace(" (Instance)", ""));
			if (localeFontReplaceEntry == null)
			{
				return default(ValueTuple<TMP_FontAsset, Material, float>?);
			}
			LocaleFontReplacePair localeFontReplacePair = Enumerable.FirstOrDefault<LocaleFontReplacePair>(localeFontReplaceEntry.pairs, (LocaleFontReplacePair p) => p.locale == locale);
			if (localeFontReplacePair != null)
			{
				return new ValueTuple<TMP_FontAsset, Material, float>?(new ValueTuple<TMP_FontAsset, Material, float>(localeFontReplacePair.font, localeFontReplacePair.material, localeFontReplacePair.resize));
			}
			return new ValueTuple<TMP_FontAsset, Material, float>?(new ValueTuple<TMP_FontAsset, Material, float>(originFont, originMaterial, 1f));
		}

		// Token: 0x06000319 RID: 793 RVA: 0x0000D86C File Offset: 0x0000BA6C
		public void FillEmptyMaterial()
		{
			if (this.ValidateFontTable())
			{
				using (List<LocaleFontReplaceEntry>.Enumerator enumerator = this.localeFontReplaceTable.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						LocaleFontReplaceEntry localeFontReplaceEntry = enumerator.Current;
						if (localeFontReplaceEntry.pairs != null && !(localeFontReplaceEntry.font == null) && !(localeFontReplaceEntry.material == null))
						{
							foreach (LocaleFontReplacePair localeFontReplacePair in localeFontReplaceEntry.pairs)
							{
								if (localeFontReplacePair.material == null || localeFontReplacePair.material.ToString() == "null")
								{
									Material material = Object.Instantiate<Material>(localeFontReplacePair.font.material);
									this.CopyFontMatStyle(material, localeFontReplaceEntry.material);
									string text = string.Concat(new string[]
									{
										localeFontReplacePair.locale.ToTag(),
										" ",
										localeFontReplacePair.font.name,
										" ",
										localeFontReplaceEntry.name
									});
									localeFontReplacePair.material = this.GenerateMaterialFile(text, material);
								}
							}
						}
					}
					return;
				}
			}
			Debug.LogWarning("localeFontReplaceTable存在非法数据，Fill Empty Material取消。");
		}

		// Token: 0x0600031A RID: 794 RVA: 0x0000D9F4 File Offset: 0x0000BBF4
		public void ValidateAllLocalizedText()
		{
		}

		// Token: 0x0600031B RID: 795 RVA: 0x0000D9F8 File Offset: 0x0000BBF8
		private Material GenerateMaterialFile(string fileName, Material mat)
		{
			"Assets/TextMesh Pro/Resources/Fonts & Materials/Generated/" + fileName + ".mat";
			try
			{
				Debug.LogWarning("Material saving is only supported in Editor mode.");
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to save material: " + ex.Message);
			}
			return null;
		}

		// Token: 0x0600031C RID: 796 RVA: 0x0000DA4C File Offset: 0x0000BC4C
		private void OnValidate()
		{
		}

		// Token: 0x0600031D RID: 797 RVA: 0x0000DA50 File Offset: 0x0000BC50
		private bool ValidateFontTable()
		{
			bool flag = true;
			foreach (LocaleFontReplaceEntry localeFontReplaceEntry in this.localeFontReplaceTable)
			{
				if (localeFontReplaceEntry.name == "")
				{
					Debug.LogWarning("localeFontReplaceTable有未起名的entry name。");
					flag = false;
				}
				if (Enumerable.ToList<IGrouping<Locale, LocaleFontReplacePair>>(Enumerable.Where<IGrouping<Locale, LocaleFontReplacePair>>(Enumerable.GroupBy<LocaleFontReplacePair, Locale>(localeFontReplaceEntry.pairs, (LocaleFontReplacePair locale) => locale.locale), (IGrouping<Locale, LocaleFontReplacePair> group) => Enumerable.Count<LocaleFontReplacePair>(group) > 1)).Count > 0)
				{
					Debug.LogWarning("localeFontReplaceTable的 " + localeFontReplaceEntry.name + " 的pair存在相同的locale。");
					flag = false;
				}
			}
			if (Enumerable.ToList<IGrouping<Material, LocaleFontReplaceEntry>>(Enumerable.Where<IGrouping<Material, LocaleFontReplaceEntry>>(Enumerable.GroupBy<LocaleFontReplaceEntry, Material>(this.localeFontReplaceTable, (LocaleFontReplaceEntry entry) => entry.material), (IGrouping<Material, LocaleFontReplaceEntry> group) => Enumerable.Count<LocaleFontReplaceEntry>(group) > 1)).Count > 0)
			{
				Debug.LogWarning("localeFontReplaceTable存在相同的被继承字体材质。");
				flag = false;
			}
			return flag;
		}

		// Token: 0x04000145 RID: 325
		public List<LocaleFontReplaceEntry> localeFontReplaceTable;

		// Token: 0x04000147 RID: 327
		private static readonly int FaceColor = Shader.PropertyToID("_FaceColor");

		// Token: 0x04000148 RID: 328
		private static readonly int FaceDilate = Shader.PropertyToID("_FaceDilate");

		// Token: 0x04000149 RID: 329
		private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

		// Token: 0x0400014A RID: 330
		private static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");

		// Token: 0x0400014B RID: 331
		private static readonly int OutlineSoftness = Shader.PropertyToID("_OutlineSoftness");

		// Token: 0x0400014C RID: 332
		private static readonly int UnderlayColor = Shader.PropertyToID("_UnderlayColor");

		// Token: 0x0400014D RID: 333
		private static readonly int UnderlayOffsetX = Shader.PropertyToID("_UnderlayOffsetX");

		// Token: 0x0400014E RID: 334
		private static readonly int UnderlayOffsetY = Shader.PropertyToID("_UnderlayOffsetY");

		// Token: 0x0400014F RID: 335
		private static readonly int UnderlayDilate = Shader.PropertyToID("_UnderlayDilate");

		// Token: 0x04000150 RID: 336
		private static readonly int UnderlaySoftness = Shader.PropertyToID("_UnderlaySoftness");

		// Token: 0x04000151 RID: 337
		private static readonly int GlowColor = Shader.PropertyToID("_GlowColor");

		// Token: 0x04000152 RID: 338
		private static readonly int GlowOffset = Shader.PropertyToID("_GlowOffset");

		// Token: 0x04000153 RID: 339
		private static readonly int GlowInner = Shader.PropertyToID("_GlowInner");

		// Token: 0x04000154 RID: 340
		private static readonly int GlowOuter = Shader.PropertyToID("_GlowOuter");

		// Token: 0x04000155 RID: 341
		private static readonly int GlowPower = Shader.PropertyToID("_GlowPower");
	}
}
