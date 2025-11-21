using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LBoL.Core.Helpers;
using UnityEngine;
using YamlDotNet.RepresentationModel;

namespace LBoL.Core
{
	// Token: 0x02000072 RID: 114
	public sealed class UnitName : IFormattable
	{
		// Token: 0x17000191 RID: 401
		// (get) Token: 0x06000525 RID: 1317 RVA: 0x000112BF File Offset: 0x0000F4BF
		// (set) Token: 0x06000526 RID: 1318 RVA: 0x000112C7 File Offset: 0x0000F4C7
		public string Color { get; internal set; }

		// Token: 0x06000527 RID: 1319 RVA: 0x000112D0 File Offset: 0x0000F4D0
		[return: TupleElementNames(new string[] { "colored", "case", "style" })]
		public static ValueTuple<bool, NounCase, UnitNameStyle> ParseFormat(string format)
		{
			bool flag = false;
			NounCase nounCase = NounCase.Nominative;
			UnitNameStyle unitNameStyle = UnitNameStyle.Default;
			for (int i = 0; i < format.Length; i++)
			{
				char c = format.get_Chars(i);
				switch (c)
				{
				case 'a':
					nounCase = NounCase.Accusative;
					break;
				case 'b':
				case 'e':
				case 'h':
				case 'j':
				case 'k':
					break;
				case 'c':
					flag = true;
					break;
				case 'd':
					nounCase = NounCase.Dative;
					break;
				case 'f':
					unitNameStyle = UnitNameStyle.Full;
					break;
				case 'g':
					nounCase = NounCase.Genitive;
					break;
				case 'i':
					nounCase = NounCase.Instrumental;
					break;
				case 'l':
					nounCase = NounCase.Locative;
					break;
				default:
					if (c != 's')
					{
						if (c == 'v')
						{
							nounCase = NounCase.Vocative;
						}
					}
					else
					{
						unitNameStyle = UnitNameStyle.Short;
					}
					break;
				}
			}
			return new ValueTuple<bool, NounCase, UnitNameStyle>(flag, nounCase, unitNameStyle);
		}

		// Token: 0x06000528 RID: 1320 RVA: 0x0001137C File Offset: 0x0000F57C
		public string ToString(string format)
		{
			if (format == null)
			{
				return this.ToString(false, NounCase.Nominative, UnitNameStyle.Default);
			}
			ValueTuple<bool, NounCase, UnitNameStyle> valueTuple = UnitName.ParseFormat(format);
			bool item = valueTuple.Item1;
			NounCase item2 = valueTuple.Item2;
			UnitNameStyle item3 = valueTuple.Item3;
			return this.ToString(item, item2, item3);
		}

		// Token: 0x06000529 RID: 1321 RVA: 0x000113BC File Offset: 0x0000F5BC
		public string ToString(bool colored, NounCase @case = NounCase.Nominative, UnitNameStyle style = UnitNameStyle.Default)
		{
			string text;
			if (!this._table.TryGetValue(new ValueTuple<NounCase, UnitNameStyle>(@case, style), ref text) && !this._table.TryGetValue(new ValueTuple<NounCase, UnitNameStyle>(@case, UnitNameStyle.Default), ref text) && !this._table.TryGetValue(new ValueTuple<NounCase, UnitNameStyle>(NounCase.Nominative, style), ref text) && !this._table.TryGetValue(new ValueTuple<NounCase, UnitNameStyle>(NounCase.Nominative, UnitNameStyle.Default), ref text))
			{
				text = "<NotFound>";
			}
			if (colored && this.Color != null)
			{
				return string.Concat(new string[] { "<color=", this.Color, ">", text, "</color>" });
			}
			return text;
		}

		// Token: 0x0600052A RID: 1322 RVA: 0x00011464 File Offset: 0x0000F664
		public string ToString(NounCase @case, UnitNameStyle style = UnitNameStyle.Default)
		{
			return this.ToString(false, @case, style);
		}

		// Token: 0x0600052B RID: 1323 RVA: 0x0001146F File Offset: 0x0000F66F
		public string ToString(UnitNameStyle style)
		{
			return this.ToString(false, NounCase.Nominative, style);
		}

		// Token: 0x0600052C RID: 1324 RVA: 0x0001147A File Offset: 0x0000F67A
		public override string ToString()
		{
			return this.ToString(false, NounCase.Nominative, UnitNameStyle.Default);
		}

		// Token: 0x0600052D RID: 1325 RVA: 0x00011485 File Offset: 0x0000F685
		public string ToString(string format, IFormatProvider formatProvider)
		{
			return this.ToString(format);
		}

		// Token: 0x0600052E RID: 1326 RVA: 0x0001148E File Offset: 0x0000F68E
		internal UnitName(string @default)
		{
			this._table.Add(new ValueTuple<NounCase, UnitNameStyle>(NounCase.Nominative, UnitNameStyle.Default), @default);
		}

		// Token: 0x0600052F RID: 1327 RVA: 0x000114B4 File Offset: 0x0000F6B4
		internal UnitName(YamlMappingNode localization)
		{
			foreach (UnitNameStyle unitNameStyle in UnitName.Styles)
			{
				YamlNode yamlNode;
				if (localization.Children.TryGetValue(unitNameStyle.ToString(), ref yamlNode))
				{
					YamlScalarNode yamlScalarNode = yamlNode as YamlScalarNode;
					if (yamlScalarNode == null)
					{
						Debug.LogError(string.Format("[Localization] {0} is not scalar. (in '{1}')", yamlNode, localization));
					}
					else
					{
						this._table.Add(new ValueTuple<NounCase, UnitNameStyle>(NounCase.Nominative, unitNameStyle), yamlScalarNode.Value);
					}
				}
			}
			foreach (NounCase nounCase in UnitName.Cases)
			{
				foreach (UnitNameStyle unitNameStyle2 in UnitName.Styles)
				{
					YamlNode yamlNode2;
					if (localization.Children.TryGetValue(nounCase.ToString() + unitNameStyle2.ToString(), ref yamlNode2))
					{
						YamlScalarNode yamlScalarNode2 = yamlNode2 as YamlScalarNode;
						if (yamlScalarNode2 == null)
						{
							Debug.LogError(string.Format("[Localization] {0} is not scalar. (in '{1}')", yamlNode2, localization));
						}
						else
						{
							this._table.Add(new ValueTuple<NounCase, UnitNameStyle>(nounCase, unitNameStyle2), yamlScalarNode2.Value);
						}
					}
				}
			}
		}

		// Token: 0x06000530 RID: 1328 RVA: 0x000115F5 File Offset: 0x0000F7F5
		internal UnitName ShallowCopy()
		{
			return (UnitName)base.MemberwiseClone();
		}

		// Token: 0x0400029B RID: 667
		private static readonly NounCase[] Cases = Enumerable.ToArray<NounCase>(Enumerable.Where<NounCase>(EnumHelper<NounCase>.GetValues(), (NounCase v) => v > NounCase.Nominative));

		// Token: 0x0400029C RID: 668
		private static readonly UnitNameStyle[] Styles = EnumHelper<UnitNameStyle>.GetValues();

		// Token: 0x0400029D RID: 669
		private readonly Dictionary<ValueTuple<NounCase, UnitNameStyle>, string> _table = new Dictionary<ValueTuple<NounCase, UnitNameStyle>, string>();
	}
}
