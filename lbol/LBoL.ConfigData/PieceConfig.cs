using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class PieceConfig
	{
		private PieceConfig(int Id, bool Type, string Projectile, int ShootType, int ParentPiece, bool AddParentAngle, bool LastWave, int FollowPiece, int ShootEnd, int HitAmount, int HitInterval, bool ZeroHitNotDie, float[][] Scale, int[][] Color, int RootType, float[][] X, float[][] Y, float[][] Radius, float[][] RadiusA, int Aim, int StartTime, int GInterval, int Group, int[][] Way, float[][] GAngle, float[][] Range, int[][] Life, int LaserLastWave, float[][] StartSpeed, float[][] StartAcc, float[][] StartAccAngle, int[][][] EvStart, int[][][] EvDuration, float[][][] EvNumber, int[][] EvType, Vector3 VanishV3, string LaunchSfx, string HitBodySfx, float HitAnimationSpeed)
		{
			this.Id = Id;
			this.Type = Type;
			this.Projectile = Projectile;
			this.ShootType = ShootType;
			this.ParentPiece = ParentPiece;
			this.AddParentAngle = AddParentAngle;
			this.LastWave = LastWave;
			this.FollowPiece = FollowPiece;
			this.ShootEnd = ShootEnd;
			this.HitAmount = HitAmount;
			this.HitInterval = HitInterval;
			this.ZeroHitNotDie = ZeroHitNotDie;
			this.Scale = Scale;
			this.Color = Color;
			this.RootType = RootType;
			this.X = X;
			this.Y = Y;
			this.Radius = Radius;
			this.RadiusA = RadiusA;
			this.Aim = Aim;
			this.StartTime = StartTime;
			this.GInterval = GInterval;
			this.Group = Group;
			this.Way = Way;
			this.GAngle = GAngle;
			this.Range = Range;
			this.Life = Life;
			this.LaserLastWave = LaserLastWave;
			this.StartSpeed = StartSpeed;
			this.StartAcc = StartAcc;
			this.StartAccAngle = StartAccAngle;
			this.EvStart = EvStart;
			this.EvDuration = EvDuration;
			this.EvNumber = EvNumber;
			this.EvType = EvType;
			this.VanishV3 = VanishV3;
			this.LaunchSfx = LaunchSfx;
			this.HitBodySfx = HitBodySfx;
			this.HitAnimationSpeed = HitAnimationSpeed;
		}
		public int Id { get; private set; }
		public bool Type { get; private set; }
		public string Projectile { get; private set; }
		public int ShootType { get; private set; }
		public int ParentPiece { get; private set; }
		public bool AddParentAngle { get; private set; }
		public bool LastWave { get; private set; }
		public int FollowPiece { get; private set; }
		public int ShootEnd { get; private set; }
		public int HitAmount { get; private set; }
		public int HitInterval { get; private set; }
		public bool ZeroHitNotDie { get; private set; }
		public float[][] Scale { get; private set; }
		public int[][] Color { get; private set; }
		public int RootType { get; private set; }
		public float[][] X { get; private set; }
		public float[][] Y { get; private set; }
		public float[][] Radius { get; private set; }
		public float[][] RadiusA { get; private set; }
		public int Aim { get; private set; }
		public int StartTime { get; private set; }
		public int GInterval { get; private set; }
		public int Group { get; private set; }
		public int[][] Way { get; private set; }
		public float[][] GAngle { get; private set; }
		public float[][] Range { get; private set; }
		public int[][] Life { get; private set; }
		public int LaserLastWave { get; private set; }
		public float[][] StartSpeed { get; private set; }
		public float[][] StartAcc { get; private set; }
		public float[][] StartAccAngle { get; private set; }
		public int[][][] EvStart { get; private set; }
		public int[][][] EvDuration { get; private set; }
		public float[][][] EvNumber { get; private set; }
		public int[][] EvType { get; private set; }
		public Vector3 VanishV3 { get; private set; }
		public string LaunchSfx { get; private set; }
		public string HitBodySfx { get; private set; }
		public float HitAnimationSpeed { get; private set; }
		public static IReadOnlyList<PieceConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<PieceConfig>(PieceConfig._data);
		}
		public static PieceConfig FromId(int Id)
		{
			ConfigDataManager.Initialize();
			PieceConfig pieceConfig;
			return (!PieceConfig._IdTable.TryGetValue(Id, out pieceConfig)) ? null : pieceConfig;
		}
		public override string ToString()
		{
			string[] array = new string[79];
			array[0] = "{PieceConfig Id=";
			array[1] = ConfigDataManager.System_Int32.ToString(this.Id);
			array[2] = ", Type=";
			array[3] = ConfigDataManager.System_Boolean.ToString(this.Type);
			array[4] = ", Projectile=";
			array[5] = ConfigDataManager.System_String.ToString(this.Projectile);
			array[6] = ", ShootType=";
			array[7] = ConfigDataManager.System_Int32.ToString(this.ShootType);
			array[8] = ", ParentPiece=";
			array[9] = ConfigDataManager.System_Int32.ToString(this.ParentPiece);
			array[10] = ", AddParentAngle=";
			array[11] = ConfigDataManager.System_Boolean.ToString(this.AddParentAngle);
			array[12] = ", LastWave=";
			array[13] = ConfigDataManager.System_Boolean.ToString(this.LastWave);
			array[14] = ", FollowPiece=";
			array[15] = ConfigDataManager.System_Int32.ToString(this.FollowPiece);
			array[16] = ", ShootEnd=";
			array[17] = ConfigDataManager.System_Int32.ToString(this.ShootEnd);
			array[18] = ", HitAmount=";
			array[19] = ConfigDataManager.System_Int32.ToString(this.HitAmount);
			array[20] = ", HitInterval=";
			array[21] = ConfigDataManager.System_Int32.ToString(this.HitInterval);
			array[22] = ", ZeroHitNotDie=";
			array[23] = ConfigDataManager.System_Boolean.ToString(this.ZeroHitNotDie);
			array[24] = ", Scale=[";
			array[25] = string.Join(", ", Enumerable.Select<float[], string>(this.Scale, (float[] v2) => "[" + string.Join(", ", Enumerable.Select<float, string>(v2, (float v1) => ConfigDataManager.System_Single.ToString(v1))) + "]"));
			array[26] = "], Color=[";
			array[27] = string.Join(", ", Enumerable.Select<int[], string>(this.Color, (int[] v2) => "[" + string.Join(", ", Enumerable.Select<int, string>(v2, (int v1) => ConfigDataManager.System_Int32.ToString(v1))) + "]"));
			array[28] = "], RootType=";
			array[29] = ConfigDataManager.System_Int32.ToString(this.RootType);
			array[30] = ", X=[";
			array[31] = string.Join(", ", Enumerable.Select<float[], string>(this.X, (float[] v2) => "[" + string.Join(", ", Enumerable.Select<float, string>(v2, (float v1) => ConfigDataManager.System_Single.ToString(v1))) + "]"));
			array[32] = "], Y=[";
			array[33] = string.Join(", ", Enumerable.Select<float[], string>(this.Y, (float[] v2) => "[" + string.Join(", ", Enumerable.Select<float, string>(v2, (float v1) => ConfigDataManager.System_Single.ToString(v1))) + "]"));
			array[34] = "], Radius=[";
			array[35] = string.Join(", ", Enumerable.Select<float[], string>(this.Radius, (float[] v2) => "[" + string.Join(", ", Enumerable.Select<float, string>(v2, (float v1) => ConfigDataManager.System_Single.ToString(v1))) + "]"));
			array[36] = "], RadiusA=[";
			array[37] = string.Join(", ", Enumerable.Select<float[], string>(this.RadiusA, (float[] v2) => "[" + string.Join(", ", Enumerable.Select<float, string>(v2, (float v1) => ConfigDataManager.System_Single.ToString(v1))) + "]"));
			array[38] = "], Aim=";
			array[39] = ConfigDataManager.System_Int32.ToString(this.Aim);
			array[40] = ", StartTime=";
			array[41] = ConfigDataManager.System_Int32.ToString(this.StartTime);
			array[42] = ", GInterval=";
			array[43] = ConfigDataManager.System_Int32.ToString(this.GInterval);
			array[44] = ", Group=";
			array[45] = ConfigDataManager.System_Int32.ToString(this.Group);
			array[46] = ", Way=[";
			array[47] = string.Join(", ", Enumerable.Select<int[], string>(this.Way, (int[] v2) => "[" + string.Join(", ", Enumerable.Select<int, string>(v2, (int v1) => ConfigDataManager.System_Int32.ToString(v1))) + "]"));
			array[48] = "], GAngle=[";
			array[49] = string.Join(", ", Enumerable.Select<float[], string>(this.GAngle, (float[] v2) => "[" + string.Join(", ", Enumerable.Select<float, string>(v2, (float v1) => ConfigDataManager.System_Single.ToString(v1))) + "]"));
			array[50] = "], Range=[";
			array[51] = string.Join(", ", Enumerable.Select<float[], string>(this.Range, (float[] v2) => "[" + string.Join(", ", Enumerable.Select<float, string>(v2, (float v1) => ConfigDataManager.System_Single.ToString(v1))) + "]"));
			array[52] = "], Life=[";
			array[53] = string.Join(", ", Enumerable.Select<int[], string>(this.Life, (int[] v2) => "[" + string.Join(", ", Enumerable.Select<int, string>(v2, (int v1) => ConfigDataManager.System_Int32.ToString(v1))) + "]"));
			array[54] = "], LaserLastWave=";
			array[55] = ConfigDataManager.System_Int32.ToString(this.LaserLastWave);
			array[56] = ", StartSpeed=[";
			array[57] = string.Join(", ", Enumerable.Select<float[], string>(this.StartSpeed, (float[] v2) => "[" + string.Join(", ", Enumerable.Select<float, string>(v2, (float v1) => ConfigDataManager.System_Single.ToString(v1))) + "]"));
			array[58] = "], StartAcc=[";
			array[59] = string.Join(", ", Enumerable.Select<float[], string>(this.StartAcc, (float[] v2) => "[" + string.Join(", ", Enumerable.Select<float, string>(v2, (float v1) => ConfigDataManager.System_Single.ToString(v1))) + "]"));
			array[60] = "], StartAccAngle=[";
			array[61] = string.Join(", ", Enumerable.Select<float[], string>(this.StartAccAngle, (float[] v2) => "[" + string.Join(", ", Enumerable.Select<float, string>(v2, (float v1) => ConfigDataManager.System_Single.ToString(v1))) + "]"));
			array[62] = "], EvStart=[";
			array[63] = string.Join(", ", Enumerable.Select<int[][], string>(this.EvStart, (int[][] v3) => "[" + string.Join(", ", Enumerable.Select<int[], string>(v3, (int[] v2) => "[" + string.Join(", ", Enumerable.Select<int, string>(v2, (int v1) => ConfigDataManager.System_Int32.ToString(v1))) + "]")) + "]"));
			array[64] = "], EvDuration=[";
			array[65] = string.Join(", ", Enumerable.Select<int[][], string>(this.EvDuration, (int[][] v3) => "[" + string.Join(", ", Enumerable.Select<int[], string>(v3, (int[] v2) => "[" + string.Join(", ", Enumerable.Select<int, string>(v2, (int v1) => ConfigDataManager.System_Int32.ToString(v1))) + "]")) + "]"));
			array[66] = "], EvNumber=[";
			array[67] = string.Join(", ", Enumerable.Select<float[][], string>(this.EvNumber, (float[][] v3) => "[" + string.Join(", ", Enumerable.Select<float[], string>(v3, (float[] v2) => "[" + string.Join(", ", Enumerable.Select<float, string>(v2, (float v1) => ConfigDataManager.System_Single.ToString(v1))) + "]")) + "]"));
			array[68] = "], EvType=[";
			array[69] = string.Join(", ", Enumerable.Select<int[], string>(this.EvType, (int[] v2) => "[" + string.Join(", ", Enumerable.Select<int, string>(v2, (int v1) => ConfigDataManager.System_Int32.ToString(v1))) + "]"));
			array[70] = "], VanishV3=";
			array[71] = ConfigDataManager.UnityEngine_Vector3.ToString(this.VanishV3);
			array[72] = ", LaunchSfx=";
			array[73] = ConfigDataManager.System_String.ToString(this.LaunchSfx);
			array[74] = ", HitBodySfx=";
			array[75] = ConfigDataManager.System_String.ToString(this.HitBodySfx);
			array[76] = ", HitAnimationSpeed=";
			array[77] = ConfigDataManager.System_Single.ToString(this.HitAnimationSpeed);
			array[78] = "}";
			return string.Concat(array);
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				PieceConfig[] array = new PieceConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new PieceConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.ReadArray<float[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<float>(r2, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1))), ConfigDataManager.ReadArray<int[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<int>(r2, (BinaryReader r1) => ConfigDataManager.System_Int32.ReadFrom(r1))), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.ReadArray<float[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<float>(r2, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1))), ConfigDataManager.ReadArray<float[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<float>(r2, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1))), ConfigDataManager.ReadArray<float[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<float>(r2, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1))), ConfigDataManager.ReadArray<float[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<float>(r2, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1))), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.ReadArray<int[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<int>(r2, (BinaryReader r1) => ConfigDataManager.System_Int32.ReadFrom(r1))), ConfigDataManager.ReadArray<float[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<float>(r2, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1))), ConfigDataManager.ReadArray<float[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<float>(r2, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1))), ConfigDataManager.ReadArray<int[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<int>(r2, (BinaryReader r1) => ConfigDataManager.System_Int32.ReadFrom(r1))), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.ReadArray<float[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<float>(r2, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1))), ConfigDataManager.ReadArray<float[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<float>(r2, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1))), ConfigDataManager.ReadArray<float[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<float>(r2, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1))), ConfigDataManager.ReadArray<int[][]>(binaryReader, (BinaryReader r3) => ConfigDataManager.ReadArray<int[]>(r3, (BinaryReader r2) => ConfigDataManager.ReadArray<int>(r2, (BinaryReader r1) => ConfigDataManager.System_Int32.ReadFrom(r1)))), ConfigDataManager.ReadArray<int[][]>(binaryReader, (BinaryReader r3) => ConfigDataManager.ReadArray<int[]>(r3, (BinaryReader r2) => ConfigDataManager.ReadArray<int>(r2, (BinaryReader r1) => ConfigDataManager.System_Int32.ReadFrom(r1)))), ConfigDataManager.ReadArray<float[][]>(binaryReader, (BinaryReader r3) => ConfigDataManager.ReadArray<float[]>(r3, (BinaryReader r2) => ConfigDataManager.ReadArray<float>(r2, (BinaryReader r1) => ConfigDataManager.System_Single.ReadFrom(r1)))), ConfigDataManager.ReadArray<int[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<int>(r2, (BinaryReader r1) => ConfigDataManager.System_Int32.ReadFrom(r1))), ConfigDataManager.UnityEngine_Vector3.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader));
				}
				PieceConfig._data = array;
				PieceConfig._IdTable = Enumerable.ToDictionary<PieceConfig, int>(PieceConfig._data, (PieceConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/PieceConfig");
			if (textAsset != null)
			{
				try
				{
					PieceConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load PieceConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'PieceConfig', please reimport config data");
			}
		}
		private static PieceConfig[] _data = Array.Empty<PieceConfig>();
		private static Dictionary<int, PieceConfig> _IdTable = Enumerable.ToDictionary<PieceConfig, int>(PieceConfig._data, (PieceConfig elem) => elem.Id);
	}
}
