using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000013 RID: 19
	public sealed class GunConfig
	{
		// Token: 0x06000316 RID: 790 RVA: 0x0000A6A4 File Offset: 0x000088A4
		private GunConfig(int Id, string Name, string Spell, string Sequence, string Animation, float? ForceHitTime, bool ForceHitAnimation, float ForceHitAnimationSpeed, float? ForceShowEndStartTime, string Shooter, float ShakePower)
		{
			this.Id = Id;
			this.Name = Name;
			this.Spell = Spell;
			this.Sequence = Sequence;
			this.Animation = Animation;
			this.ForceHitTime = ForceHitTime;
			this.ForceHitAnimation = ForceHitAnimation;
			this.ForceHitAnimationSpeed = ForceHitAnimationSpeed;
			this.ForceShowEndStartTime = ForceShowEndStartTime;
			this.Shooter = Shooter;
			this.ShakePower = ShakePower;
		}

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x06000317 RID: 791 RVA: 0x0000A70C File Offset: 0x0000890C
		// (set) Token: 0x06000318 RID: 792 RVA: 0x0000A714 File Offset: 0x00008914
		public int Id { get; private set; }

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x06000319 RID: 793 RVA: 0x0000A71D File Offset: 0x0000891D
		// (set) Token: 0x0600031A RID: 794 RVA: 0x0000A725 File Offset: 0x00008925
		public string Name { get; private set; }

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x0600031B RID: 795 RVA: 0x0000A72E File Offset: 0x0000892E
		// (set) Token: 0x0600031C RID: 796 RVA: 0x0000A736 File Offset: 0x00008936
		public string Spell { get; private set; }

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x0600031D RID: 797 RVA: 0x0000A73F File Offset: 0x0000893F
		// (set) Token: 0x0600031E RID: 798 RVA: 0x0000A747 File Offset: 0x00008947
		public string Sequence { get; private set; }

		// Token: 0x17000123 RID: 291
		// (get) Token: 0x0600031F RID: 799 RVA: 0x0000A750 File Offset: 0x00008950
		// (set) Token: 0x06000320 RID: 800 RVA: 0x0000A758 File Offset: 0x00008958
		public string Animation { get; private set; }

		// Token: 0x17000124 RID: 292
		// (get) Token: 0x06000321 RID: 801 RVA: 0x0000A761 File Offset: 0x00008961
		// (set) Token: 0x06000322 RID: 802 RVA: 0x0000A769 File Offset: 0x00008969
		public float? ForceHitTime { get; private set; }

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x06000323 RID: 803 RVA: 0x0000A772 File Offset: 0x00008972
		// (set) Token: 0x06000324 RID: 804 RVA: 0x0000A77A File Offset: 0x0000897A
		public bool ForceHitAnimation { get; private set; }

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x06000325 RID: 805 RVA: 0x0000A783 File Offset: 0x00008983
		// (set) Token: 0x06000326 RID: 806 RVA: 0x0000A78B File Offset: 0x0000898B
		public float ForceHitAnimationSpeed { get; private set; }

		// Token: 0x17000127 RID: 295
		// (get) Token: 0x06000327 RID: 807 RVA: 0x0000A794 File Offset: 0x00008994
		// (set) Token: 0x06000328 RID: 808 RVA: 0x0000A79C File Offset: 0x0000899C
		public float? ForceShowEndStartTime { get; private set; }

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x06000329 RID: 809 RVA: 0x0000A7A5 File Offset: 0x000089A5
		// (set) Token: 0x0600032A RID: 810 RVA: 0x0000A7AD File Offset: 0x000089AD
		public string Shooter { get; private set; }

		// Token: 0x17000129 RID: 297
		// (get) Token: 0x0600032B RID: 811 RVA: 0x0000A7B6 File Offset: 0x000089B6
		// (set) Token: 0x0600032C RID: 812 RVA: 0x0000A7BE File Offset: 0x000089BE
		public float ShakePower { get; private set; }

		// Token: 0x0600032D RID: 813 RVA: 0x0000A7C7 File Offset: 0x000089C7
		public static IReadOnlyList<GunConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<GunConfig>(GunConfig._data);
		}

		// Token: 0x0600032E RID: 814 RVA: 0x0000A7D8 File Offset: 0x000089D8
		public static GunConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			GunConfig gunConfig;
			return (!GunConfig._NameTable.TryGetValue(Name, out gunConfig)) ? null : gunConfig;
		}

		// Token: 0x0600032F RID: 815 RVA: 0x0000A804 File Offset: 0x00008A04
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{GunConfig Id=",
				ConfigDataManager.System_Int32.ToString(this.Id),
				", Name=",
				ConfigDataManager.System_String.ToString(this.Name),
				", Spell=",
				ConfigDataManager.System_String.ToString(this.Spell),
				", Sequence=",
				ConfigDataManager.System_String.ToString(this.Sequence),
				", Animation=",
				ConfigDataManager.System_String.ToString(this.Animation),
				", ForceHitTime=",
				(this.ForceHitTime == null) ? "null" : ConfigDataManager.System_Single.ToString(this.ForceHitTime.Value),
				", ForceHitAnimation=",
				ConfigDataManager.System_Boolean.ToString(this.ForceHitAnimation),
				", ForceHitAnimationSpeed=",
				ConfigDataManager.System_Single.ToString(this.ForceHitAnimationSpeed),
				", ForceShowEndStartTime=",
				(this.ForceShowEndStartTime == null) ? "null" : ConfigDataManager.System_Single.ToString(this.ForceShowEndStartTime.Value),
				", Shooter=",
				ConfigDataManager.System_String.ToString(this.Shooter),
				", ShakePower=",
				ConfigDataManager.System_Single.ToString(this.ShakePower),
				"}"
			});
		}

		// Token: 0x06000330 RID: 816 RVA: 0x0000A9A8 File Offset: 0x00008BA8
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				GunConfig[] array = new GunConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new GunConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new float?(ConfigDataManager.System_Single.ReadFrom(binaryReader)), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new float?(ConfigDataManager.System_Single.ReadFrom(binaryReader)), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader));
				}
				GunConfig._data = array;
				GunConfig._NameTable = Enumerable.ToDictionary<GunConfig, string>(GunConfig._data, (GunConfig elem) => elem.Name);
			}
		}

		// Token: 0x06000331 RID: 817 RVA: 0x0000AB00 File Offset: 0x00008D00
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/GunConfig");
			if (textAsset != null)
			{
				try
				{
					GunConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load GunConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'GunConfig', please reimport config data");
			}
		}

		// Token: 0x0400019D RID: 413
		private static GunConfig[] _data = Array.Empty<GunConfig>();

		// Token: 0x0400019E RID: 414
		private static Dictionary<string, GunConfig> _NameTable = Enumerable.ToDictionary<GunConfig, string>(GunConfig._data, (GunConfig elem) => elem.Name);
	}
}
