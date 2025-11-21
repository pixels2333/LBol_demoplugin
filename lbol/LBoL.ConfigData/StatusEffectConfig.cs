using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x0200000C RID: 12
	public sealed class StatusEffectConfig
	{
		// Token: 0x0600017F RID: 383 RVA: 0x00006120 File Offset: 0x00004320
		private StatusEffectConfig(int Index, string Id, int Order, StatusEffectType Type, bool IsVerbose, bool IsStackable, int? StackActionTriggerLevel, bool HasLevel, StackType? LevelStackType, bool HasDuration, StackType? DurationStackType, DurationDecreaseTiming DurationDecreaseTiming, bool HasCount, StackType? CountStackType, StackType? LimitStackType, bool ShowPlusByLimit, Keyword Keywords, IReadOnlyList<string> RelativeEffects, string ImageId, string VFX, string VFXloop, string SFX)
		{
			this.Index = Index;
			this.Id = Id;
			this.Order = Order;
			this.Type = Type;
			this.IsVerbose = IsVerbose;
			this.IsStackable = IsStackable;
			this.StackActionTriggerLevel = StackActionTriggerLevel;
			this.HasLevel = HasLevel;
			this.LevelStackType = LevelStackType;
			this.HasDuration = HasDuration;
			this.DurationStackType = DurationStackType;
			this.DurationDecreaseTiming = DurationDecreaseTiming;
			this.HasCount = HasCount;
			this.CountStackType = CountStackType;
			this.LimitStackType = LimitStackType;
			this.ShowPlusByLimit = ShowPlusByLimit;
			this.Keywords = Keywords;
			this.RelativeEffects = RelativeEffects;
			this.ImageId = ImageId;
			this.VFX = VFX;
			this.VFXloop = VFXloop;
			this.SFX = SFX;
		}

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x06000180 RID: 384 RVA: 0x000061E0 File Offset: 0x000043E0
		// (set) Token: 0x06000181 RID: 385 RVA: 0x000061E8 File Offset: 0x000043E8
		public int Index { get; private set; }

		// Token: 0x17000088 RID: 136
		// (get) Token: 0x06000182 RID: 386 RVA: 0x000061F1 File Offset: 0x000043F1
		// (set) Token: 0x06000183 RID: 387 RVA: 0x000061F9 File Offset: 0x000043F9
		public string Id { get; private set; }

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x06000184 RID: 388 RVA: 0x00006202 File Offset: 0x00004402
		// (set) Token: 0x06000185 RID: 389 RVA: 0x0000620A File Offset: 0x0000440A
		public int Order { get; private set; }

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x06000186 RID: 390 RVA: 0x00006213 File Offset: 0x00004413
		// (set) Token: 0x06000187 RID: 391 RVA: 0x0000621B File Offset: 0x0000441B
		public StatusEffectType Type { get; private set; }

		// Token: 0x1700008B RID: 139
		// (get) Token: 0x06000188 RID: 392 RVA: 0x00006224 File Offset: 0x00004424
		// (set) Token: 0x06000189 RID: 393 RVA: 0x0000622C File Offset: 0x0000442C
		public bool IsVerbose { get; private set; }

		// Token: 0x1700008C RID: 140
		// (get) Token: 0x0600018A RID: 394 RVA: 0x00006235 File Offset: 0x00004435
		// (set) Token: 0x0600018B RID: 395 RVA: 0x0000623D File Offset: 0x0000443D
		public bool IsStackable { get; private set; }

		// Token: 0x1700008D RID: 141
		// (get) Token: 0x0600018C RID: 396 RVA: 0x00006246 File Offset: 0x00004446
		// (set) Token: 0x0600018D RID: 397 RVA: 0x0000624E File Offset: 0x0000444E
		public int? StackActionTriggerLevel { get; private set; }

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x0600018E RID: 398 RVA: 0x00006257 File Offset: 0x00004457
		// (set) Token: 0x0600018F RID: 399 RVA: 0x0000625F File Offset: 0x0000445F
		public bool HasLevel { get; private set; }

		// Token: 0x1700008F RID: 143
		// (get) Token: 0x06000190 RID: 400 RVA: 0x00006268 File Offset: 0x00004468
		// (set) Token: 0x06000191 RID: 401 RVA: 0x00006270 File Offset: 0x00004470
		public StackType? LevelStackType { get; private set; }

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x06000192 RID: 402 RVA: 0x00006279 File Offset: 0x00004479
		// (set) Token: 0x06000193 RID: 403 RVA: 0x00006281 File Offset: 0x00004481
		public bool HasDuration { get; private set; }

		// Token: 0x17000091 RID: 145
		// (get) Token: 0x06000194 RID: 404 RVA: 0x0000628A File Offset: 0x0000448A
		// (set) Token: 0x06000195 RID: 405 RVA: 0x00006292 File Offset: 0x00004492
		public StackType? DurationStackType { get; private set; }

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x06000196 RID: 406 RVA: 0x0000629B File Offset: 0x0000449B
		// (set) Token: 0x06000197 RID: 407 RVA: 0x000062A3 File Offset: 0x000044A3
		public DurationDecreaseTiming DurationDecreaseTiming { get; private set; }

		// Token: 0x17000093 RID: 147
		// (get) Token: 0x06000198 RID: 408 RVA: 0x000062AC File Offset: 0x000044AC
		// (set) Token: 0x06000199 RID: 409 RVA: 0x000062B4 File Offset: 0x000044B4
		public bool HasCount { get; private set; }

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x0600019A RID: 410 RVA: 0x000062BD File Offset: 0x000044BD
		// (set) Token: 0x0600019B RID: 411 RVA: 0x000062C5 File Offset: 0x000044C5
		public StackType? CountStackType { get; private set; }

		// Token: 0x17000095 RID: 149
		// (get) Token: 0x0600019C RID: 412 RVA: 0x000062CE File Offset: 0x000044CE
		// (set) Token: 0x0600019D RID: 413 RVA: 0x000062D6 File Offset: 0x000044D6
		public StackType? LimitStackType { get; private set; }

		// Token: 0x17000096 RID: 150
		// (get) Token: 0x0600019E RID: 414 RVA: 0x000062DF File Offset: 0x000044DF
		// (set) Token: 0x0600019F RID: 415 RVA: 0x000062E7 File Offset: 0x000044E7
		public bool ShowPlusByLimit { get; private set; }

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x060001A0 RID: 416 RVA: 0x000062F0 File Offset: 0x000044F0
		// (set) Token: 0x060001A1 RID: 417 RVA: 0x000062F8 File Offset: 0x000044F8
		public Keyword Keywords { get; private set; }

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x060001A2 RID: 418 RVA: 0x00006301 File Offset: 0x00004501
		// (set) Token: 0x060001A3 RID: 419 RVA: 0x00006309 File Offset: 0x00004509
		public IReadOnlyList<string> RelativeEffects { get; private set; }

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x060001A4 RID: 420 RVA: 0x00006312 File Offset: 0x00004512
		// (set) Token: 0x060001A5 RID: 421 RVA: 0x0000631A File Offset: 0x0000451A
		public string ImageId { get; private set; }

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x060001A6 RID: 422 RVA: 0x00006323 File Offset: 0x00004523
		// (set) Token: 0x060001A7 RID: 423 RVA: 0x0000632B File Offset: 0x0000452B
		public string VFX { get; private set; }

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x060001A8 RID: 424 RVA: 0x00006334 File Offset: 0x00004534
		// (set) Token: 0x060001A9 RID: 425 RVA: 0x0000633C File Offset: 0x0000453C
		public string VFXloop { get; private set; }

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x060001AA RID: 426 RVA: 0x00006345 File Offset: 0x00004545
		// (set) Token: 0x060001AB RID: 427 RVA: 0x0000634D File Offset: 0x0000454D
		public string SFX { get; private set; }

		// Token: 0x060001AC RID: 428 RVA: 0x00006356 File Offset: 0x00004556
		public static IReadOnlyList<StatusEffectConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<StatusEffectConfig>(StatusEffectConfig._data);
		}

		// Token: 0x060001AD RID: 429 RVA: 0x00006368 File Offset: 0x00004568
		public static StatusEffectConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			StatusEffectConfig statusEffectConfig;
			return (!StatusEffectConfig._IdTable.TryGetValue(Id, out statusEffectConfig)) ? null : statusEffectConfig;
		}

		// Token: 0x060001AE RID: 430 RVA: 0x00006394 File Offset: 0x00004594
		public override string ToString()
		{
			string[] array = new string[45];
			array[0] = "{StatusEffectConfig Index=";
			array[1] = ConfigDataManager.System_Int32.ToString(this.Index);
			array[2] = ", Id=";
			array[3] = ConfigDataManager.System_String.ToString(this.Id);
			array[4] = ", Order=";
			array[5] = ConfigDataManager.System_Int32.ToString(this.Order);
			array[6] = ", Type=";
			array[7] = this.Type.ToString();
			array[8] = ", IsVerbose=";
			array[9] = ConfigDataManager.System_Boolean.ToString(this.IsVerbose);
			array[10] = ", IsStackable=";
			array[11] = ConfigDataManager.System_Boolean.ToString(this.IsStackable);
			array[12] = ", StackActionTriggerLevel=";
			array[13] = ((this.StackActionTriggerLevel == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.StackActionTriggerLevel.Value));
			array[14] = ", HasLevel=";
			array[15] = ConfigDataManager.System_Boolean.ToString(this.HasLevel);
			array[16] = ", LevelStackType=";
			array[17] = ((this.LevelStackType == null) ? "null" : this.LevelStackType.Value.ToString());
			array[18] = ", HasDuration=";
			array[19] = ConfigDataManager.System_Boolean.ToString(this.HasDuration);
			array[20] = ", DurationStackType=";
			array[21] = ((this.DurationStackType == null) ? "null" : this.DurationStackType.Value.ToString());
			array[22] = ", DurationDecreaseTiming=";
			array[23] = this.DurationDecreaseTiming.ToString();
			array[24] = ", HasCount=";
			array[25] = ConfigDataManager.System_Boolean.ToString(this.HasCount);
			array[26] = ", CountStackType=";
			array[27] = ((this.CountStackType == null) ? "null" : this.CountStackType.Value.ToString());
			array[28] = ", LimitStackType=";
			array[29] = ((this.LimitStackType == null) ? "null" : this.LimitStackType.Value.ToString());
			array[30] = ", ShowPlusByLimit=";
			array[31] = ConfigDataManager.System_Boolean.ToString(this.ShowPlusByLimit);
			array[32] = ", Keywords=";
			array[33] = this.Keywords.ToString();
			array[34] = ", RelativeEffects=[";
			array[35] = string.Join(", ", Enumerable.Select<string, string>(this.RelativeEffects, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[36] = "], ImageId=";
			array[37] = ConfigDataManager.System_String.ToString(this.ImageId);
			array[38] = ", VFX=";
			array[39] = ConfigDataManager.System_String.ToString(this.VFX);
			array[40] = ", VFXloop=";
			array[41] = ConfigDataManager.System_String.ToString(this.VFXloop);
			array[42] = ", SFX=";
			array[43] = ConfigDataManager.System_String.ToString(this.SFX);
			array[44] = "}";
			return string.Concat(array);
		}

		// Token: 0x060001AF RID: 431 RVA: 0x00006730 File Offset: 0x00004930
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				StatusEffectConfig[] array = new StatusEffectConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new StatusEffectConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), (StatusEffectType)binaryReader.ReadInt32(), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new StackType?((StackType)binaryReader.ReadInt32()), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new StackType?((StackType)binaryReader.ReadInt32()), (DurationDecreaseTiming)binaryReader.ReadInt32(), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new StackType?((StackType)binaryReader.ReadInt32()), (!binaryReader.ReadBoolean()) ? null : new StackType?((StackType)binaryReader.ReadInt32()), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), (Keyword)binaryReader.ReadInt64(), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader));
				}
				StatusEffectConfig._data = array;
				StatusEffectConfig._IdTable = Enumerable.ToDictionary<StatusEffectConfig, string>(StatusEffectConfig._data, (StatusEffectConfig elem) => elem.Id);
			}
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x00006954 File Offset: 0x00004B54
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/StatusEffectConfig");
			if (textAsset != null)
			{
				try
				{
					StatusEffectConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load StatusEffectConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'StatusEffectConfig', please reimport config data");
			}
		}

		// Token: 0x040000D3 RID: 211
		private static StatusEffectConfig[] _data = Array.Empty<StatusEffectConfig>();

		// Token: 0x040000D4 RID: 212
		private static Dictionary<string, StatusEffectConfig> _IdTable = Enumerable.ToDictionary<StatusEffectConfig, string>(StatusEffectConfig._data, (StatusEffectConfig elem) => elem.Id);
	}
}
