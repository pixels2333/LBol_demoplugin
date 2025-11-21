using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YamlDotNet.RepresentationModel;

namespace LBoL.Core
{
	// Token: 0x0200000B RID: 11
	public static class EntityNameTable
	{
		// Token: 0x0600003C RID: 60 RVA: 0x00002964 File Offset: 0x00000B64
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

		// Token: 0x0600003D RID: 61 RVA: 0x000029A0 File Offset: 0x00000BA0
		internal static string TryGet(string qualifiedId, string @case)
		{
			string text;
			if (EntityNameTable._extraTable != null && EntityNameTable._extraTable.TryGetValue(qualifiedId + ":" + @case, ref text))
			{
				return text;
			}
			return null;
		}

		// Token: 0x0400005B RID: 91
		private static Dictionary<string, string> _extraTable;
	}
}
