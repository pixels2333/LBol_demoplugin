using System;
using System.Collections.Generic;
using System.ComponentModel;
using YamlDotNet.Serialization;
namespace LBoL.Core.SaveData
{
	[Serializable]
	public sealed class GameSettingsSaveData
	{
		[DefaultValue(false)]
		public bool IsTurboMode;
		[DefaultValue(true)]
		public bool ShowVerboseKeywords = true;
		[DefaultValue(false)]
		public bool ShowIllustrator;
		[DefaultValue(false)]
		public bool IsLargeTooltips;
		[DefaultValue(false)]
		public bool PreferWideTooltips;
		[DefaultValue(true)]
		public bool RightClickCancel = true;
		[DefaultValue(false)]
		public bool IsLoopOrder;
		[DefaultValue(false)]
		public bool SingleEnemyAutoSelect;
		[DefaultValue(QuickPlayLevel.Default)]
		public QuickPlayLevel QuickPlayLevel;
		[DefaultValue(true)]
		public bool ShowXCostEmptyUseWarning = true;
		[DefaultValue(false)]
		public bool ShowShortcut;
		[DefaultValue(false)]
		public bool ShowCardOrder;
		[DefaultValue(false)]
		public bool ShowReload;
		[DefaultValue(true)]
		public bool Shake = true;
		[DefaultValue(false)]
		public bool CostMoreLeft;
		[DefaultValue(HintLevel.Detailed)]
		public HintLevel HintLevel;
		[DefaultValue(false)]
		public bool ShowRandomResult;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public Dictionary<string, string> KeyboardBindings = new Dictionary<string, string>();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public Dictionary<string, string> PreferredCardIllustrators = new Dictionary<string, string>();
	}
}
