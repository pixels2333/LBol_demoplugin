using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YamlDotNet.RepresentationModel;
namespace LBoL.Core
{
	public static class EntityNameTable
	{
		public static async UniTask ReloadExtraLocalizationAsync()
		{
			try
			{
				EntityNameTable._extraTable = new Dictionary<string, string>();
				foreach (KeyValuePair<YamlNode, YamlNode> keyValuePair in (await Localization.LoadFileAsync("ExtraName", false)).Children)
				{
					YamlNode yamlNode;
					YamlNode yamlNode2;
					keyValuePair.Deconstruct(ref yamlNode, ref yamlNode2);
					YamlNode yamlNode3 = yamlNode;
					YamlNode yamlNode4 = yamlNode2;
					YamlScalarNode yamlScalarNode = yamlNode3 as YamlScalarNode;
					if (yamlScalarNode == null)
					{
						Debug.LogError(string.Format("[Localization] ExtraName key {0} is not scalar", yamlNode3));
					}
					else
					{
						string value = yamlScalarNode.Value;
						YamlScalarNode yamlScalarNode2 = yamlNode4 as YamlScalarNode;
						if (yamlScalarNode2 == null)
						{
							Debug.LogError(string.Format("[Localization] ExtraName value of {0} is not scalar: {1}", value, yamlNode4));
						}
						else if (!EntityNameTable._extraTable.TryAdd(value, yamlScalarNode2.Value))
						{
							Debug.LogError("[Localization] Cannot add duplicated " + value + " to ExtraName");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[Localization] failed to load ExtraName.yaml: " + ex.Message);
				EntityNameTable._extraTable = null;
			}
		}
		internal static string TryGet(string qualifiedId, string @case)
		{
			string text;
			if (EntityNameTable._extraTable != null && EntityNameTable._extraTable.TryGetValue(qualifiedId + ":" + @case, ref text))
			{
				return text;
			}
			return null;
		}
		private static Dictionary<string, string> _extraTable;
	}
}
