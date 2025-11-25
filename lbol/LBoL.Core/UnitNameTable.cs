using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YamlDotNet.RepresentationModel;
namespace LBoL.Core
{
	public static class UnitNameTable
	{
		public static async UniTask ReloadLocalizationAsync()
		{
			try
			{
				UnitNameTable._table = new Dictionary<string, UnitName>();
				foreach (KeyValuePair<YamlNode, YamlNode> keyValuePair in (await Localization.LoadFileAsync("UnitName", true)).Children)
				{
					YamlNode yamlNode;
					YamlNode yamlNode2;
					keyValuePair.Deconstruct(ref yamlNode, ref yamlNode2);
					YamlNode yamlNode3 = yamlNode;
					YamlNode yamlNode4 = yamlNode2;
					YamlScalarNode yamlScalarNode = yamlNode3 as YamlScalarNode;
					if (yamlScalarNode == null)
					{
						Debug.LogError(string.Format("[Localization] UnitName key {0} is not scalar", yamlNode3));
					}
					else
					{
						string value = yamlScalarNode.Value;
						YamlMappingNode yamlMappingNode = yamlNode4 as YamlMappingNode;
						if (yamlMappingNode == null)
						{
							Debug.LogError(string.Format("[Localization] UnitName value of {0} is not mapping: {1}", value, yamlNode4));
						}
						else if (!UnitNameTable._table.TryAdd(value, new UnitName(yamlMappingNode)))
						{
							Debug.LogError("[Localization] Cannot add duplicated " + value + " to UnitName");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[Localization] failed to load UnitName.yaml: " + ex.Message);
				UnitNameTable._table = null;
			}
		}
		public static UnitName GetName(string id, string color = null)
		{
			UnitName unitName;
			if (UnitNameTable._table != null && UnitNameTable._table.TryGetValue(id, ref unitName))
			{
				UnitName unitName2 = unitName.ShallowCopy();
				unitName2.Color = color;
				return unitName2;
			}
			return new UnitName("<" + id + ".Name>");
		}
		public static UnitName GetDefaultPlayerName()
		{
			return UnitNameTable.GetName("DefaultPlayer", null);
		}
		public static UnitName GetDefaultOwnerName()
		{
			return UnitNameTable.GetName("DefaultOwner", null);
		}
		private static Dictionary<string, UnitName> _table;
	}
}
