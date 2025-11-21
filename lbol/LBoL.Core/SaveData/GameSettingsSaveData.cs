using System;
using System.Collections.Generic;
using System.ComponentModel;
using YamlDotNet.Serialization;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000E0 RID: 224
	[Serializable]
	public sealed class GameSettingsSaveData
	{
		// Token: 0x04000469 RID: 1129
		[DefaultValue(false)]
		public bool IsTurboMode;

		// Token: 0x0400046A RID: 1130
		[DefaultValue(true)]
		public bool ShowVerboseKeywords = true;

		// Token: 0x0400046B RID: 1131
		[DefaultValue(false)]
		public bool ShowIllustrator;

		// Token: 0x0400046C RID: 1132
		[DefaultValue(false)]
		public bool IsLargeTooltips;

		// Token: 0x0400046D RID: 1133
		[DefaultValue(false)]
		public bool PreferWideTooltips;

		// Token: 0x0400046E RID: 1134
		[DefaultValue(true)]
		public bool RightClickCancel = true;

		// Token: 0x0400046F RID: 1135
		[DefaultValue(false)]
		public bool IsLoopOrder;

		// Token: 0x04000470 RID: 1136
		[DefaultValue(false)]
		public bool SingleEnemyAutoSelect;

		// Token: 0x04000471 RID: 1137
		[DefaultValue(QuickPlayLevel.Default)]
		public QuickPlayLevel QuickPlayLevel;

		// Token: 0x04000472 RID: 1138
		[DefaultValue(true)]
		public bool ShowXCostEmptyUseWarning = true;

		// Token: 0x04000473 RID: 1139
		[DefaultValue(false)]
		public bool ShowShortcut;

		// Token: 0x04000474 RID: 1140
		[DefaultValue(false)]
		public bool ShowCardOrder;

		// Token: 0x04000475 RID: 1141
		[DefaultValue(false)]
		public bool ShowReload;

		// Token: 0x04000476 RID: 1142
		[DefaultValue(true)]
		public bool Shake = true;

		// Token: 0x04000477 RID: 1143
		[DefaultValue(false)]
		public bool CostMoreLeft;

		// Token: 0x04000478 RID: 1144
		[DefaultValue(HintLevel.Detailed)]
		public HintLevel HintLevel;

		// Token: 0x04000479 RID: 1145
		[DefaultValue(false)]
		public bool ShowRandomResult;

		// Token: 0x0400047A RID: 1146
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public Dictionary<string, string> KeyboardBindings = new Dictionary<string, string>();

		// Token: 0x0400047B RID: 1147
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public Dictionary<string, string> PreferredCardIllustrators = new Dictionary<string, string>();
	}
}
