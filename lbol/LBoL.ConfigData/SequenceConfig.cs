using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x0200000E RID: 14
	public sealed class SequenceConfig
	{
		// Token: 0x060001FB RID: 507 RVA: 0x000073BB File Offset: 0x000055BB
		private SequenceConfig(string Id, string Animation, IReadOnlyList<float> KeyTime, IReadOnlyList<float> Speed, float StartTime)
		{
			this.Id = Id;
			this.Animation = Animation;
			this.KeyTime = KeyTime;
			this.Speed = Speed;
			this.StartTime = StartTime;
		}

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x060001FC RID: 508 RVA: 0x000073E8 File Offset: 0x000055E8
		// (set) Token: 0x060001FD RID: 509 RVA: 0x000073F0 File Offset: 0x000055F0
		public string Id { get; private set; }

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x060001FE RID: 510 RVA: 0x000073F9 File Offset: 0x000055F9
		// (set) Token: 0x060001FF RID: 511 RVA: 0x00007401 File Offset: 0x00005601
		public string Animation { get; private set; }

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x06000200 RID: 512 RVA: 0x0000740A File Offset: 0x0000560A
		// (set) Token: 0x06000201 RID: 513 RVA: 0x00007412 File Offset: 0x00005612
		public IReadOnlyList<float> KeyTime { get; private set; }

		// Token: 0x170000BA RID: 186
		// (get) Token: 0x06000202 RID: 514 RVA: 0x0000741B File Offset: 0x0000561B
		// (set) Token: 0x06000203 RID: 515 RVA: 0x00007423 File Offset: 0x00005623
		public IReadOnlyList<float> Speed { get; private set; }

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x06000204 RID: 516 RVA: 0x0000742C File Offset: 0x0000562C
		// (set) Token: 0x06000205 RID: 517 RVA: 0x00007434 File Offset: 0x00005634
		public float StartTime { get; private set; }

		// Token: 0x06000206 RID: 518 RVA: 0x0000743D File Offset: 0x0000563D
		public static IReadOnlyList<SequenceConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<SequenceConfig>(SequenceConfig._data);
		}

		// Token: 0x06000207 RID: 519 RVA: 0x00007450 File Offset: 0x00005650
		public static SequenceConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			SequenceConfig sequenceConfig;
			return (!SequenceConfig._IdTable.TryGetValue(Id, out sequenceConfig)) ? null : sequenceConfig;
		}

		// Token: 0x06000208 RID: 520 RVA: 0x0000747C File Offset: 0x0000567C
		public override string ToString()
		{
			string[] array = new string[11];
			array[0] = "{SequenceConfig Id=";
			array[1] = ConfigDataManager.System_String.ToString(this.Id);
			array[2] = ", Animation=";
			array[3] = ConfigDataManager.System_String.ToString(this.Animation);
			array[4] = ", KeyTime=[";
			array[5] = string.Join(", ", Enumerable.Select<float, string>(this.KeyTime, (float v1) => ConfigDataManager.System_Single.ToString(v1)));
			array[6] = "], Speed=[";
			array[7] = string.Join(", ", Enumerable.Select<float, string>(this.Speed, (float v1) => ConfigDataManager.System_Single.ToString(v1)));
			array[8] = "], StartTime=";
			array[9] = ConfigDataManager.System_Single.ToString(this.StartTime);
			array[10] = "}";
			return string.Concat(array);
		}

		// Token: 0x06000209 RID: 521 RVA: 0x0000756C File Offset: 0x0000576C
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				SequenceConfig[] array = new SequenceConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new SequenceConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.ReadList<float>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1)), ConfigDataManager.ReadList<float>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1)), ConfigDataManager.System_Single.ReadFrom(binaryReader));
				}
				SequenceConfig._data = array;
				SequenceConfig._IdTable = Enumerable.ToDictionary<SequenceConfig, string>(SequenceConfig._data, (SequenceConfig elem) => elem.Id);
			}
		}

		// Token: 0x0600020A RID: 522 RVA: 0x00007668 File Offset: 0x00005868
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/SequenceConfig");
			if (textAsset != null)
			{
				try
				{
					SequenceConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load SequenceConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'SequenceConfig', please reimport config data");
			}
		}

		// Token: 0x04000102 RID: 258
		private static SequenceConfig[] _data = Array.Empty<SequenceConfig>();

		// Token: 0x04000103 RID: 259
		private static Dictionary<string, SequenceConfig> _IdTable = Enumerable.ToDictionary<SequenceConfig, string>(SequenceConfig._data, (SequenceConfig elem) => elem.Id);
	}
}
