using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class StatusEffectConfig
	{
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
		public int Index { get; private set; }
		public string Id { get; private set; }
		public int Order { get; private set; }
		public StatusEffectType Type { get; private set; }
		public bool IsVerbose { get; private set; }
		public bool IsStackable { get; private set; }
		public int? StackActionTriggerLevel { get; private set; }
		public bool HasLevel { get; private set; }
		public StackType? LevelStackType { get; private set; }
		public bool HasDuration { get; private set; }
		public StackType? DurationStackType { get; private set; }
		public DurationDecreaseTiming DurationDecreaseTiming { get; private set; }
		public bool HasCount { get; private set; }
		public StackType? CountStackType { get; private set; }
		public StackType? LimitStackType { get; private set; }
		public bool ShowPlusByLimit { get; private set; }
		public Keyword Keywords { get; private set; }
		public IReadOnlyList<string> RelativeEffects { get; private set; }
		public string ImageId { get; private set; }
		public string VFX { get; private set; }
		public string VFXloop { get; private set; }
		public string SFX { get; private set; }
		public static IReadOnlyList<StatusEffectConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<StatusEffectConfig>(StatusEffectConfig._data);
		}
		public static StatusEffectConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			StatusEffectConfig statusEffectConfig;
			return (!StatusEffectConfig._IdTable.TryGetValue(Id, out statusEffectConfig)) ? null : statusEffectConfig;
		}
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
		private static StatusEffectConfig[] _data = Array.Empty<StatusEffectConfig>();
		private static Dictionary<string, StatusEffectConfig> _IdTable = Enumerable.ToDictionary<StatusEffectConfig, string>(StatusEffectConfig._data, (StatusEffectConfig elem) => elem.Id);
	}
}
