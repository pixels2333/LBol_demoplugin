using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class GunConfig
	{
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
		public int Id { get; private set; }
		public string Name { get; private set; }
		public string Spell { get; private set; }
		public string Sequence { get; private set; }
		public string Animation { get; private set; }
		public float? ForceHitTime { get; private set; }
		public bool ForceHitAnimation { get; private set; }
		public float ForceHitAnimationSpeed { get; private set; }
		public float? ForceShowEndStartTime { get; private set; }
		public string Shooter { get; private set; }
		public float ShakePower { get; private set; }
		public static IReadOnlyList<GunConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<GunConfig>(GunConfig._data);
		}
		public static GunConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			GunConfig gunConfig;
			return (!GunConfig._NameTable.TryGetValue(Name, out gunConfig)) ? null : gunConfig;
		}
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
		private static GunConfig[] _data = Array.Empty<GunConfig>();
		private static Dictionary<string, GunConfig> _NameTable = Enumerable.ToDictionary<GunConfig, string>(GunConfig._data, (GunConfig elem) => elem.Name);
	}
}
