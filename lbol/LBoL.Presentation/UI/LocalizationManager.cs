using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LBoL.Core;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.UI
{
	public class LocalizationManager : MonoBehaviour
	{
		public static LocalizationManager Instance { get; private set; }
		private void Awake()
		{
			if (LocalizationManager.Instance)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			LocalizationManager.Instance = this;
		}
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
		public void ValidateAllLocalizedText()
		{
		}
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
		private void OnValidate()
		{
		}
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
		public List<LocaleFontReplaceEntry> localeFontReplaceTable;
		private static readonly int FaceColor = Shader.PropertyToID("_FaceColor");
		private static readonly int FaceDilate = Shader.PropertyToID("_FaceDilate");
		private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
		private static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
		private static readonly int OutlineSoftness = Shader.PropertyToID("_OutlineSoftness");
		private static readonly int UnderlayColor = Shader.PropertyToID("_UnderlayColor");
		private static readonly int UnderlayOffsetX = Shader.PropertyToID("_UnderlayOffsetX");
		private static readonly int UnderlayOffsetY = Shader.PropertyToID("_UnderlayOffsetY");
		private static readonly int UnderlayDilate = Shader.PropertyToID("_UnderlayDilate");
		private static readonly int UnderlaySoftness = Shader.PropertyToID("_UnderlaySoftness");
		private static readonly int GlowColor = Shader.PropertyToID("_GlowColor");
		private static readonly int GlowOffset = Shader.PropertyToID("_GlowOffset");
		private static readonly int GlowInner = Shader.PropertyToID("_GlowInner");
		private static readonly int GlowOuter = Shader.PropertyToID("_GlowOuter");
		private static readonly int GlowPower = Shader.PropertyToID("_GlowPower");
	}
}
