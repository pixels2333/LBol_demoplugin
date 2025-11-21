using System;
using System.Collections.Generic;
using System.IO;
using LBoL.Base;
using Untitled.ConfigDataBuilder.Base;

namespace LBoL.ConfigData
{
	// Token: 0x0200001E RID: 30
	public static class ConfigDataManager
	{
		// Token: 0x060004A3 RID: 1187 RVA: 0x0000E3B8 File Offset: 0x0000C5B8
		internal static T[] ReadArray<T>(BinaryReader reader, Func<BinaryReader, T> readFunc)
		{
			int num = reader.ReadInt32();
			T[] array = new T[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = readFunc(reader);
			}
			return array;
		}

		// Token: 0x060004A4 RID: 1188 RVA: 0x0000E3F4 File Offset: 0x0000C5F4
		internal static IReadOnlyList<T> ReadList<T>(BinaryReader reader, Func<BinaryReader, T> readFunc)
		{
			int num = reader.ReadInt32();
			T[] array = new T[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = readFunc(reader);
			}
			return Array.AsReadOnly<T>(array);
		}

		// Token: 0x060004A5 RID: 1189 RVA: 0x0000E438 File Offset: 0x0000C638
		internal static IReadOnlyDictionary<TKey, TValue> ReadDictionary<TKey, TValue>(BinaryReader reader, Func<BinaryReader, TKey> readKeyFunc, Func<BinaryReader, TValue> readValueFunc)
		{
			int num = reader.ReadInt32();
			Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
			for (int i = 0; i < num; i++)
			{
				TKey tkey = readKeyFunc(reader);
				TValue tvalue = readValueFunc(reader);
				dictionary.Add(tkey, tvalue);
			}
			return dictionary;
		}

		// Token: 0x060004A6 RID: 1190 RVA: 0x0000E480 File Offset: 0x0000C680
		public static void Reload()
		{
			BgmConfig.Reload();
			SfxConfig.Reload();
			UiSoundConfig.Reload();
			EffectConfig.Reload();
			SpineEventConfig.Reload();
			EnemyUnitConfig.Reload();
			EnemyGroupConfig.Reload();
			PlayerUnitConfig.Reload();
			UltimateSkillConfig.Reload();
			DollConfig.Reload();
			StatusEffectConfig.Reload();
			UnitModelConfig.Reload();
			SequenceConfig.Reload();
			SpellConfig.Reload();
			CardConfig.Reload();
			ExhibitConfig.Reload();
			JadeBoxConfig.Reload();
			GunConfig.Reload();
			PieceConfig.Reload();
			BulletConfig.Reload();
			LaserConfig.Reload();
			StageConfig.Reload();
			AdventureConfig.Reload();
			ExpConfig.Reload();
			AchievementConfig.Reload();
			BluePointConfig.Reload();
			RuleConfig.Reload();
			PuzzleConfig.Reload();
		}

		// Token: 0x060004A7 RID: 1191 RVA: 0x0000E519 File Offset: 0x0000C719
		public static void Initialize()
		{
			if (ConfigDataManager._initialized)
			{
				return;
			}
			ConfigDataManager.Reload();
			ConfigDataManager._initialized = true;
		}

		// Token: 0x04000271 RID: 625
		internal static readonly BoolConverter System_Boolean = new BoolConverter();

		// Token: 0x04000272 RID: 626
		internal static readonly Int16Converter System_Int16 = new Int16Converter();

		// Token: 0x04000273 RID: 627
		internal static readonly UInt16Converter System_UInt16 = new UInt16Converter();

		// Token: 0x04000274 RID: 628
		internal static readonly Int32Converter System_Int32 = new Int32Converter();

		// Token: 0x04000275 RID: 629
		internal static readonly UInt32Converter System_UInt32 = new UInt32Converter();

		// Token: 0x04000276 RID: 630
		internal static readonly Int64Converter System_Int64 = new Int64Converter();

		// Token: 0x04000277 RID: 631
		internal static readonly UInt64Converter System_UInt64 = new UInt64Converter();

		// Token: 0x04000278 RID: 632
		internal static readonly SingleConverter System_Single = new SingleConverter();

		// Token: 0x04000279 RID: 633
		internal static readonly DoubleConverter System_Double = new DoubleConverter();

		// Token: 0x0400027A RID: 634
		internal static readonly Vector2Converter UnityEngine_Vector2 = new Vector2Converter();

		// Token: 0x0400027B RID: 635
		internal static readonly Vector3Converter UnityEngine_Vector3 = new Vector3Converter();

		// Token: 0x0400027C RID: 636
		internal static readonly Vector4Converter UnityEngine_Vector4 = new Vector4Converter();

		// Token: 0x0400027D RID: 637
		internal static readonly Vector2IntConverter UnityEngine_Vector2Int = new Vector2IntConverter();

		// Token: 0x0400027E RID: 638
		internal static readonly Vector3IntConverter UnityEngine_Vector3Int = new Vector3IntConverter();

		// Token: 0x0400027F RID: 639
		internal static readonly ColorConverter UnityEngine_Color = new ColorConverter();

		// Token: 0x04000280 RID: 640
		internal static readonly Color32Converter UnityEngine_Color32 = new Color32Converter();

		// Token: 0x04000281 RID: 641
		internal static readonly StringConverter System_String = new StringConverter();

		// Token: 0x04000282 RID: 642
		internal static readonly BaseManaGroupConverter LBoL_Base_BaseManaGroup = new BaseManaGroupConverter();

		// Token: 0x04000283 RID: 643
		internal static readonly ManaColorConverter LBoL_Base_ManaColor = new ManaColorConverter();

		// Token: 0x04000284 RID: 644
		internal static readonly ManaGroupConverter LBoL_Base_ManaGroup = new ManaGroupConverter();

		// Token: 0x04000285 RID: 645
		internal static readonly MinMaxConverter LBoL_Base_MinMax = new MinMaxConverter();

		// Token: 0x04000286 RID: 646
		private static bool _initialized;
	}
}
