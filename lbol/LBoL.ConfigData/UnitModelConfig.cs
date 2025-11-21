using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x0200000D RID: 13
	public sealed class UnitModelConfig
	{
		// Token: 0x060001B6 RID: 438 RVA: 0x00006A1C File Offset: 0x00004C1C
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

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x060001B7 RID: 439 RVA: 0x00006AFC File Offset: 0x00004CFC
		// (set) Token: 0x060001B8 RID: 440 RVA: 0x00006B04 File Offset: 0x00004D04
		public string Name { get; private set; }

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x060001B9 RID: 441 RVA: 0x00006B0D File Offset: 0x00004D0D
		// (set) Token: 0x060001BA RID: 442 RVA: 0x00006B15 File Offset: 0x00004D15
		public int Type { get; private set; }

		// Token: 0x1700009F RID: 159
		// (get) Token: 0x060001BB RID: 443 RVA: 0x00006B1E File Offset: 0x00004D1E
		// (set) Token: 0x060001BC RID: 444 RVA: 0x00006B26 File Offset: 0x00004D26
		public string EffectName { get; private set; }

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x060001BD RID: 445 RVA: 0x00006B2F File Offset: 0x00004D2F
		// (set) Token: 0x060001BE RID: 446 RVA: 0x00006B37 File Offset: 0x00004D37
		public Vector2 Offset { get; private set; }

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x060001BF RID: 447 RVA: 0x00006B40 File Offset: 0x00004D40
		// (set) Token: 0x060001C0 RID: 448 RVA: 0x00006B48 File Offset: 0x00004D48
		public bool Flip { get; private set; }

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x060001C1 RID: 449 RVA: 0x00006B51 File Offset: 0x00004D51
		// (set) Token: 0x060001C2 RID: 450 RVA: 0x00006B59 File Offset: 0x00004D59
		public int Dielevel { get; private set; }

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x060001C3 RID: 451 RVA: 0x00006B62 File Offset: 0x00004D62
		// (set) Token: 0x060001C4 RID: 452 RVA: 0x00006B6A File Offset: 0x00004D6A
		public Vector2 Box { get; private set; }

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x060001C5 RID: 453 RVA: 0x00006B73 File Offset: 0x00004D73
		// (set) Token: 0x060001C6 RID: 454 RVA: 0x00006B7B File Offset: 0x00004D7B
		public float Shield { get; private set; }

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x060001C7 RID: 455 RVA: 0x00006B84 File Offset: 0x00004D84
		// (set) Token: 0x060001C8 RID: 456 RVA: 0x00006B8C File Offset: 0x00004D8C
		public float Block { get; private set; }

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x060001C9 RID: 457 RVA: 0x00006B95 File Offset: 0x00004D95
		// (set) Token: 0x060001CA RID: 458 RVA: 0x00006B9D File Offset: 0x00004D9D
		public Vector2 Hp { get; private set; }

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x060001CB RID: 459 RVA: 0x00006BA6 File Offset: 0x00004DA6
		// (set) Token: 0x060001CC RID: 460 RVA: 0x00006BAE File Offset: 0x00004DAE
		public float HpLength { get; private set; }

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x060001CD RID: 461 RVA: 0x00006BB7 File Offset: 0x00004DB7
		// (set) Token: 0x060001CE RID: 462 RVA: 0x00006BBF File Offset: 0x00004DBF
		public Vector2 Info { get; private set; }

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x060001CF RID: 463 RVA: 0x00006BC8 File Offset: 0x00004DC8
		// (set) Token: 0x060001D0 RID: 464 RVA: 0x00006BD0 File Offset: 0x00004DD0
		public Vector2 Select { get; private set; }

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x060001D1 RID: 465 RVA: 0x00006BD9 File Offset: 0x00004DD9
		// (set) Token: 0x060001D2 RID: 466 RVA: 0x00006BE1 File Offset: 0x00004DE1
		public IReadOnlyList<float> ShootStartTime { get; private set; }

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x060001D3 RID: 467 RVA: 0x00006BEA File Offset: 0x00004DEA
		// (set) Token: 0x060001D4 RID: 468 RVA: 0x00006BF2 File Offset: 0x00004DF2
		public IReadOnlyList<Vector2> ShootPoint { get; private set; }

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x060001D5 RID: 469 RVA: 0x00006BFB File Offset: 0x00004DFB
		// (set) Token: 0x060001D6 RID: 470 RVA: 0x00006C03 File Offset: 0x00004E03
		public IReadOnlyList<Vector2> ShooterPoint { get; private set; }

		// Token: 0x170000AD RID: 173
		// (get) Token: 0x060001D7 RID: 471 RVA: 0x00006C0C File Offset: 0x00004E0C
		// (set) Token: 0x060001D8 RID: 472 RVA: 0x00006C14 File Offset: 0x00004E14
		public Vector2 Hit { get; private set; }

		// Token: 0x170000AE RID: 174
		// (get) Token: 0x060001D9 RID: 473 RVA: 0x00006C1D File Offset: 0x00004E1D
		// (set) Token: 0x060001DA RID: 474 RVA: 0x00006C25 File Offset: 0x00004E25
		public float HitRep { get; private set; }

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x060001DB RID: 475 RVA: 0x00006C2E File Offset: 0x00004E2E
		// (set) Token: 0x060001DC RID: 476 RVA: 0x00006C36 File Offset: 0x00004E36
		public float GuardRep { get; private set; }

		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x060001DD RID: 477 RVA: 0x00006C3F File Offset: 0x00004E3F
		// (set) Token: 0x060001DE RID: 478 RVA: 0x00006C47 File Offset: 0x00004E47
		public Vector2 Chat { get; private set; }

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x060001DF RID: 479 RVA: 0x00006C50 File Offset: 0x00004E50
		// (set) Token: 0x060001E0 RID: 480 RVA: 0x00006C58 File Offset: 0x00004E58
		public Vector2 ChatPortraitXY { get; private set; }

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x060001E1 RID: 481 RVA: 0x00006C61 File Offset: 0x00004E61
		// (set) Token: 0x060001E2 RID: 482 RVA: 0x00006C69 File Offset: 0x00004E69
		public Vector2 ChatPortraitWH { get; private set; }

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x060001E3 RID: 483 RVA: 0x00006C72 File Offset: 0x00004E72
		// (set) Token: 0x060001E4 RID: 484 RVA: 0x00006C7A File Offset: 0x00004E7A
		public bool HasSpellPortrait { get; private set; }

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x060001E5 RID: 485 RVA: 0x00006C83 File Offset: 0x00004E83
		// (set) Token: 0x060001E6 RID: 486 RVA: 0x00006C8B File Offset: 0x00004E8B
		public Vector2 SpellPosition { get; private set; }

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x060001E7 RID: 487 RVA: 0x00006C94 File Offset: 0x00004E94
		// (set) Token: 0x060001E8 RID: 488 RVA: 0x00006C9C File Offset: 0x00004E9C
		public float SpellScale { get; private set; }

		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x060001E9 RID: 489 RVA: 0x00006CA5 File Offset: 0x00004EA5
		// (set) Token: 0x060001EA RID: 490 RVA: 0x00006CAD File Offset: 0x00004EAD
		public IReadOnlyList<Color32> SpellColor { get; private set; }

		// Token: 0x060001EB RID: 491 RVA: 0x00006CB6 File Offset: 0x00004EB6
		public static IReadOnlyList<UnitModelConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<UnitModelConfig>(UnitModelConfig._data);
		}

		// Token: 0x060001EC RID: 492 RVA: 0x00006CC8 File Offset: 0x00004EC8
		public static UnitModelConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			UnitModelConfig unitModelConfig;
			return (!UnitModelConfig._NameTable.TryGetValue(Name, out unitModelConfig)) ? null : unitModelConfig;
		}

		// Token: 0x060001ED RID: 493 RVA: 0x00006CF4 File Offset: 0x00004EF4
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

		// Token: 0x060001EE RID: 494 RVA: 0x00007088 File Offset: 0x00005288
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

		// Token: 0x060001EF RID: 495 RVA: 0x000072A8 File Offset: 0x000054A8
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

		// Token: 0x040000F2 RID: 242
		private static UnitModelConfig[] _data = Array.Empty<UnitModelConfig>();

		// Token: 0x040000F3 RID: 243
		private static Dictionary<string, UnitModelConfig> _NameTable = Enumerable.ToDictionary<UnitModelConfig, string>(UnitModelConfig._data, (UnitModelConfig elem) => elem.Name);
	}
}
