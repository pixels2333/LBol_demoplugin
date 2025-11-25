using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class UnitModelConfig
	{
		private UnitModelConfig(string Name, int Type, string EffectName, Vector2 Offset, bool Flip, int Dielevel, Vector2 Box, float Shield, float Block, Vector2 Hp, float HpLength, Vector2 Info, Vector2 Select, IReadOnlyList<float> ShootStartTime, IReadOnlyList<Vector2> ShootPoint, IReadOnlyList<Vector2> ShooterPoint, Vector2 Hit, float HitRep, float GuardRep, Vector2 Chat, Vector2 ChatPortraitXY, Vector2 ChatPortraitWH, bool HasSpellPortrait, Vector2 SpellPosition, float SpellScale, IReadOnlyList<Color32> SpellColor)
		{
			this.Name = Name;
			this.Type = Type;
			this.EffectName = EffectName;
			this.Offset = Offset;
			this.Flip = Flip;
			this.Dielevel = Dielevel;
			this.Box = Box;
			this.Shield = Shield;
			this.Block = Block;
			this.Hp = Hp;
			this.HpLength = HpLength;
			this.Info = Info;
			this.Select = Select;
			this.ShootStartTime = ShootStartTime;
			this.ShootPoint = ShootPoint;
			this.ShooterPoint = ShooterPoint;
			this.Hit = Hit;
			this.HitRep = HitRep;
			this.GuardRep = GuardRep;
			this.Chat = Chat;
			this.ChatPortraitXY = ChatPortraitXY;
			this.ChatPortraitWH = ChatPortraitWH;
			this.HasSpellPortrait = HasSpellPortrait;
			this.SpellPosition = SpellPosition;
			this.SpellScale = SpellScale;
			this.SpellColor = SpellColor;
		}
		public string Name { get; private set; }
		public int Type { get; private set; }
		public string EffectName { get; private set; }
		public Vector2 Offset { get; private set; }
		public bool Flip { get; private set; }
		public int Dielevel { get; private set; }
		public Vector2 Box { get; private set; }
		public float Shield { get; private set; }
		public float Block { get; private set; }
		public Vector2 Hp { get; private set; }
		public float HpLength { get; private set; }
		public Vector2 Info { get; private set; }
		public Vector2 Select { get; private set; }
		public IReadOnlyList<float> ShootStartTime { get; private set; }
		public IReadOnlyList<Vector2> ShootPoint { get; private set; }
		public IReadOnlyList<Vector2> ShooterPoint { get; private set; }
		public Vector2 Hit { get; private set; }
		public float HitRep { get; private set; }
		public float GuardRep { get; private set; }
		public Vector2 Chat { get; private set; }
		public Vector2 ChatPortraitXY { get; private set; }
		public Vector2 ChatPortraitWH { get; private set; }
		public bool HasSpellPortrait { get; private set; }
		public Vector2 SpellPosition { get; private set; }
		public float SpellScale { get; private set; }
		public IReadOnlyList<Color32> SpellColor { get; private set; }
		public static IReadOnlyList<UnitModelConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<UnitModelConfig>(UnitModelConfig._data);
		}
		public static UnitModelConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			UnitModelConfig unitModelConfig;
			return (!UnitModelConfig._NameTable.TryGetValue(Name, out unitModelConfig)) ? null : unitModelConfig;
		}
		public override string ToString()
		{
			string[] array = new string[53];
			array[0] = "{UnitModelConfig Name=";
			array[1] = ConfigDataManager.System_String.ToString(this.Name);
			array[2] = ", Type=";
			array[3] = ConfigDataManager.System_Int32.ToString(this.Type);
			array[4] = ", EffectName=";
			array[5] = ConfigDataManager.System_String.ToString(this.EffectName);
			array[6] = ", Offset=";
			array[7] = ConfigDataManager.UnityEngine_Vector2.ToString(this.Offset);
			array[8] = ", Flip=";
			array[9] = ConfigDataManager.System_Boolean.ToString(this.Flip);
			array[10] = ", Dielevel=";
			array[11] = ConfigDataManager.System_Int32.ToString(this.Dielevel);
			array[12] = ", Box=";
			array[13] = ConfigDataManager.UnityEngine_Vector2.ToString(this.Box);
			array[14] = ", Shield=";
			array[15] = ConfigDataManager.System_Single.ToString(this.Shield);
			array[16] = ", Block=";
			array[17] = ConfigDataManager.System_Single.ToString(this.Block);
			array[18] = ", Hp=";
			array[19] = ConfigDataManager.UnityEngine_Vector2.ToString(this.Hp);
			array[20] = ", HpLength=";
			array[21] = ConfigDataManager.System_Single.ToString(this.HpLength);
			array[22] = ", Info=";
			array[23] = ConfigDataManager.UnityEngine_Vector2.ToString(this.Info);
			array[24] = ", Select=";
			array[25] = ConfigDataManager.UnityEngine_Vector2.ToString(this.Select);
			array[26] = ", ShootStartTime=[";
			array[27] = string.Join(", ", Enumerable.Select<float, string>(this.ShootStartTime, (float v1) => ConfigDataManager.System_Single.ToString(v1)));
			array[28] = "], ShootPoint=[";
			array[29] = string.Join(", ", Enumerable.Select<Vector2, string>(this.ShootPoint, (Vector2 v2) => ConfigDataManager.UnityEngine_Vector2.ToString(v2)));
			array[30] = "], ShooterPoint=[";
			array[31] = string.Join(", ", Enumerable.Select<Vector2, string>(this.ShooterPoint, (Vector2 v2) => ConfigDataManager.UnityEngine_Vector2.ToString(v2)));
			array[32] = "], Hit=";
			array[33] = ConfigDataManager.UnityEngine_Vector2.ToString(this.Hit);
			array[34] = ", HitRep=";
			array[35] = ConfigDataManager.System_Single.ToString(this.HitRep);
			array[36] = ", GuardRep=";
			array[37] = ConfigDataManager.System_Single.ToString(this.GuardRep);
			array[38] = ", Chat=";
			array[39] = ConfigDataManager.UnityEngine_Vector2.ToString(this.Chat);
			array[40] = ", ChatPortraitXY=";
			array[41] = ConfigDataManager.UnityEngine_Vector2.ToString(this.ChatPortraitXY);
			array[42] = ", ChatPortraitWH=";
			array[43] = ConfigDataManager.UnityEngine_Vector2.ToString(this.ChatPortraitWH);
			array[44] = ", HasSpellPortrait=";
			array[45] = ConfigDataManager.System_Boolean.ToString(this.HasSpellPortrait);
			array[46] = ", SpellPosition=";
			array[47] = ConfigDataManager.UnityEngine_Vector2.ToString(this.SpellPosition);
			array[48] = ", SpellScale=";
			array[49] = ConfigDataManager.System_Single.ToString(this.SpellScale);
			array[50] = ", SpellColor=[";
			array[51] = string.Join(", ", Enumerable.Select<Color32, string>(this.SpellColor, (Color32 v2) => ConfigDataManager.UnityEngine_Color32.ToString(v2)));
			array[52] = "]}";
			return string.Concat(array);
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				UnitModelConfig[] array = new UnitModelConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new UnitModelConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.ReadList<float>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1)), ConfigDataManager.ReadList<Vector2>(binaryReader, (BinaryReader r2) => ConfigDataManager.UnityEngine_Vector2.ReadFrom(r2)), ConfigDataManager.ReadList<Vector2>(binaryReader, (BinaryReader r2) => ConfigDataManager.UnityEngine_Vector2.ReadFrom(r2)), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader), ConfigDataManager.ReadList<Color32>(binaryReader, (BinaryReader r2) => ConfigDataManager.UnityEngine_Color32.ReadFrom(r2)));
				}
				UnitModelConfig._data = array;
				UnitModelConfig._NameTable = Enumerable.ToDictionary<UnitModelConfig, string>(UnitModelConfig._data, (UnitModelConfig elem) => elem.Name);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/UnitModelConfig");
			if (textAsset != null)
			{
				try
				{
					UnitModelConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load UnitModelConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'UnitModelConfig', please reimport config data");
			}
		}
		private static UnitModelConfig[] _data = Array.Empty<UnitModelConfig>();
		private static Dictionary<string, UnitModelConfig> _NameTable = Enumerable.ToDictionary<UnitModelConfig, string>(UnitModelConfig._data, (UnitModelConfig elem) => elem.Name);
	}
}
