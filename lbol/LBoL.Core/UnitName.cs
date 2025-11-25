using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LBoL.Core.Helpers;
using UnityEngine;
using YamlDotNet.RepresentationModel;
namespace LBoL.Core
{
	public sealed class UnitName : IFormattable
	{
		public string Color { get; internal set; }
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
		public string ToString(NounCase @case, UnitNameStyle style = UnitNameStyle.Default)
		{
			return this.ToString(false, @case, style);
		}
		public string ToString(UnitNameStyle style)
		{
			return this.ToString(false, NounCase.Nominative, style);
		}
		public override string ToString()
		{
			return this.ToString(false, NounCase.Nominative, UnitNameStyle.Default);
		}
		public string ToString(string format, IFormatProvider formatProvider)
		{
			return this.ToString(format);
		}
		internal UnitName(string @default)
		{
			this._table.Add(new ValueTuple<NounCase, UnitNameStyle>(NounCase.Nominative, UnitNameStyle.Default), @default);
		}
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
		internal UnitName ShallowCopy()
		{
			return (UnitName)base.MemberwiseClone();
		}
		private static readonly NounCase[] Cases = Enumerable.ToArray<NounCase>(Enumerable.Where<NounCase>(EnumHelper<NounCase>.GetValues(), (NounCase v) => v > NounCase.Nominative));
		private static readonly UnitNameStyle[] Styles = EnumHelper<UnitNameStyle>.GetValues();
		private readonly Dictionary<ValueTuple<NounCase, UnitNameStyle>, string> _table = new Dictionary<ValueTuple<NounCase, UnitNameStyle>, string>();
	}
}
