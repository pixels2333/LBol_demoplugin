using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YamlDotNet.RepresentationModel;

namespace LBoL.Core
{
	// Token: 0x02000075 RID: 117
	public static class UnitNameTable
	{
		// Token: 0x06000532 RID: 1330 RVA: 0x00011634 File Offset: 0x0000F834
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

		// Token: 0x06000533 RID: 1331 RVA: 0x00011670 File Offset: 0x0000F870
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

		// Token: 0x06000534 RID: 1332 RVA: 0x000116B6 File Offset: 0x0000F8B6
		public static UnitName GetDefaultPlayerName()
		{
			return UnitNameTable.GetName("DefaultPlayer", null);
		}

		// Token: 0x06000535 RID: 1333 RVA: 0x000116C3 File Offset: 0x0000F8C3
		public static UnitName GetDefaultOwnerName()
		{
			return UnitNameTable.GetName("DefaultOwner", null);
		}

		// Token: 0x040002AB RID: 683
		private static Dictionary<string, UnitName> _table;
	}
}
