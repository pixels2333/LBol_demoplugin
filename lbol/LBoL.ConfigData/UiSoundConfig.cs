using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000004 RID: 4
	public sealed class UiSoundConfig
	{
		// Token: 0x06000037 RID: 55 RVA: 0x00002903 File Offset: 0x00000B03
		private UiSoundConfig(string Name, string Folder, string Path, float Volume)
		{
			this.Name = Name;
			this.Folder = Folder;
			this.Path = Path;
			this.Volume = Volume;
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000038 RID: 56 RVA: 0x00002928 File Offset: 0x00000B28
		// (set) Token: 0x06000039 RID: 57 RVA: 0x00002930 File Offset: 0x00000B30
		public string Name { get; private set; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600003A RID: 58 RVA: 0x00002939 File Offset: 0x00000B39
		// (set) Token: 0x0600003B RID: 59 RVA: 0x00002941 File Offset: 0x00000B41
		public string Folder { get; private set; }

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x0600003C RID: 60 RVA: 0x0000294A File Offset: 0x00000B4A
		// (set) Token: 0x0600003D RID: 61 RVA: 0x00002952 File Offset: 0x00000B52
		public string Path { get; private set; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600003E RID: 62 RVA: 0x0000295B File Offset: 0x00000B5B
		// (set) Token: 0x0600003F RID: 63 RVA: 0x00002963 File Offset: 0x00000B63
		public float Volume { get; private set; }

		// Token: 0x06000040 RID: 64 RVA: 0x0000296C File Offset: 0x00000B6C
		public static IReadOnlyList<UiSoundConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<UiSoundConfig>(UiSoundConfig._data);
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00002980 File Offset: 0x00000B80
		public static UiSoundConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			UiSoundConfig uiSoundConfig;
			return (!UiSoundConfig._NameTable.TryGetValue(Name, out uiSoundConfig)) ? null : uiSoundConfig;
		}

		// Token: 0x06000042 RID: 66 RVA: 0x000029AC File Offset: 0x00000BAC
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{UiSoundConfig Name=",
				ConfigDataManager.System_String.ToString(this.Name),
				", Folder=",
				ConfigDataManager.System_String.ToString(this.Folder),
				", Path=",
				ConfigDataManager.System_String.ToString(this.Path),
				", Volume=",
				ConfigDataManager.System_Single.ToString(this.Volume),
				"}"
			});
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00002A3C File Offset: 0x00000C3C
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				UiSoundConfig[] array = new UiSoundConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new UiSoundConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader));
				}
				UiSoundConfig._data = array;
				UiSoundConfig._NameTable = Enumerable.ToDictionary<UiSoundConfig, string>(UiSoundConfig._data, (UiSoundConfig elem) => elem.Name);
			}
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00002B00 File Offset: 0x00000D00
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/UiSoundConfig");
			if (textAsset != null)
			{
				try
				{
					UiSoundConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load UiSoundConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'UiSoundConfig', please reimport config data");
			}
		}

		// Token: 0x0400001D RID: 29
		private static UiSoundConfig[] _data = Array.Empty<UiSoundConfig>();

		// Token: 0x0400001E RID: 30
		private static Dictionary<string, UiSoundConfig> _NameTable = Enumerable.ToDictionary<UiSoundConfig, string>(UiSoundConfig._data, (UiSoundConfig elem) => elem.Name);
	}
}
