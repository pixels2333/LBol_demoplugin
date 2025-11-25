using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class EnemyGroupConfig
	{
		private EnemyGroupConfig(string Id, bool IsSub, IReadOnlyList<string> Subs, string Name, string FormationName, IReadOnlyList<string> Enemies, EnemyType EnemyType, bool Hidden, float DebutTime, bool RollBossExhibit, Vector2 PlayerRoot, string PreBattleDialogName, string PostBattleDialogName, string Environment)
		{
			this.Id = Id;
			this.IsSub = IsSub;
			this.Subs = Subs;
			this.Name = Name;
			this.FormationName = FormationName;
			this.Enemies = Enemies;
			this.EnemyType = EnemyType;
			this.Hidden = Hidden;
			this.DebutTime = DebutTime;
			this.RollBossExhibit = RollBossExhibit;
			this.PlayerRoot = PlayerRoot;
			this.PreBattleDialogName = PreBattleDialogName;
			this.PostBattleDialogName = PostBattleDialogName;
			this.Environment = Environment;
		}
		public string Id { get; private set; }
		public bool IsSub { get; private set; }
		public IReadOnlyList<string> Subs { get; private set; }
		public string Name { get; private set; }
		public string FormationName { get; private set; }
		public IReadOnlyList<string> Enemies { get; private set; }
		public EnemyType EnemyType { get; private set; }
		public bool Hidden { get; private set; }
		public float DebutTime { get; private set; }
		public bool RollBossExhibit { get; private set; }
		public Vector2 PlayerRoot { get; private set; }
		public string PreBattleDialogName { get; private set; }
		public string PostBattleDialogName { get; private set; }
		public string Environment { get; private set; }
		public static IReadOnlyList<EnemyGroupConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<EnemyGroupConfig>(EnemyGroupConfig._data);
		}
		public static EnemyGroupConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			EnemyGroupConfig enemyGroupConfig;
			return (!EnemyGroupConfig._IdTable.TryGetValue(Id, out enemyGroupConfig)) ? null : enemyGroupConfig;
		}
		public override string ToString()
		{
			string[] array = new string[29];
			array[0] = "{EnemyGroupConfig Id=";
			array[1] = ConfigDataManager.System_String.ToString(this.Id);
			array[2] = ", IsSub=";
			array[3] = ConfigDataManager.System_Boolean.ToString(this.IsSub);
			array[4] = ", Subs=[";
			array[5] = string.Join(", ", Enumerable.Select<string, string>(this.Subs, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[6] = "], Name=";
			array[7] = ConfigDataManager.System_String.ToString(this.Name);
			array[8] = ", FormationName=";
			array[9] = ConfigDataManager.System_String.ToString(this.FormationName);
			array[10] = ", Enemies=[";
			array[11] = string.Join(", ", Enumerable.Select<string, string>(this.Enemies, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[12] = "], EnemyType=";
			array[13] = this.EnemyType.ToString();
			array[14] = ", Hidden=";
			array[15] = ConfigDataManager.System_Boolean.ToString(this.Hidden);
			array[16] = ", DebutTime=";
			array[17] = ConfigDataManager.System_Single.ToString(this.DebutTime);
			array[18] = ", RollBossExhibit=";
			array[19] = ConfigDataManager.System_Boolean.ToString(this.RollBossExhibit);
			array[20] = ", PlayerRoot=";
			array[21] = ConfigDataManager.UnityEngine_Vector2.ToString(this.PlayerRoot);
			array[22] = ", PreBattleDialogName=";
			array[23] = ConfigDataManager.System_String.ToString(this.PreBattleDialogName);
			array[24] = ", PostBattleDialogName=";
			array[25] = ConfigDataManager.System_String.ToString(this.PostBattleDialogName);
			array[26] = ", Environment=";
			array[27] = ConfigDataManager.System_String.ToString(this.Environment);
			array[28] = "}";
			return string.Concat(array);
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				EnemyGroupConfig[] array = new EnemyGroupConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new EnemyGroupConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), (EnemyType)binaryReader.ReadInt32(), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader));
				}
				EnemyGroupConfig._data = array;
				EnemyGroupConfig._IdTable = Enumerable.ToDictionary<EnemyGroupConfig, string>(EnemyGroupConfig._data, (EnemyGroupConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/EnemyGroupConfig");
			if (textAsset != null)
			{
				try
				{
					EnemyGroupConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load EnemyGroupConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'EnemyGroupConfig', please reimport config data");
			}
		}
		private static EnemyGroupConfig[] _data = Array.Empty<EnemyGroupConfig>();
		private static Dictionary<string, EnemyGroupConfig> _IdTable = Enumerable.ToDictionary<EnemyGroupConfig, string>(EnemyGroupConfig._data, (EnemyGroupConfig elem) => elem.Id);
	}
}
