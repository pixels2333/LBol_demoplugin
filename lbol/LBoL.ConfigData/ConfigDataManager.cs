using System;
using System.Collections.Generic;
using System.IO;
using LBoL.Base;
using Untitled.ConfigDataBuilder.Base;
namespace LBoL.ConfigData
{
	public static class ConfigDataManager
	{
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
		public static void Initialize()
		{
			if (ConfigDataManager._initialized)
			{
				return;
			}
			ConfigDataManager.Reload();
			ConfigDataManager._initialized = true;
		}
		internal static readonly BoolConverter System_Boolean = new BoolConverter();
		internal static readonly Int16Converter System_Int16 = new Int16Converter();
		internal static readonly UInt16Converter System_UInt16 = new UInt16Converter();
		internal static readonly Int32Converter System_Int32 = new Int32Converter();
		internal static readonly UInt32Converter System_UInt32 = new UInt32Converter();
		internal static readonly Int64Converter System_Int64 = new Int64Converter();
		internal static readonly UInt64Converter System_UInt64 = new UInt64Converter();
		internal static readonly SingleConverter System_Single = new SingleConverter();
		internal static readonly DoubleConverter System_Double = new DoubleConverter();
		internal static readonly Vector2Converter UnityEngine_Vector2 = new Vector2Converter();
		internal static readonly Vector3Converter UnityEngine_Vector3 = new Vector3Converter();
		internal static readonly Vector4Converter UnityEngine_Vector4 = new Vector4Converter();
		internal static readonly Vector2IntConverter UnityEngine_Vector2Int = new Vector2IntConverter();
		internal static readonly Vector3IntConverter UnityEngine_Vector3Int = new Vector3IntConverter();
		internal static readonly ColorConverter UnityEngine_Color = new ColorConverter();
		internal static readonly Color32Converter UnityEngine_Color32 = new Color32Converter();
		internal static readonly StringConverter System_String = new StringConverter();
		internal static readonly BaseManaGroupConverter LBoL_Base_BaseManaGroup = new BaseManaGroupConverter();
		internal static readonly ManaColorConverter LBoL_Base_ManaColor = new ManaColorConverter();
		internal static readonly ManaGroupConverter LBoL_Base_ManaGroup = new ManaGroupConverter();
		internal static readonly MinMaxConverter LBoL_Base_MinMax = new MinMaxConverter();
		private static bool _initialized;
	}
}
