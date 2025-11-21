using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000014 RID: 20
	public sealed class PieceConfig
	{
		// Token: 0x06000335 RID: 821 RVA: 0x0000ABAC File Offset: 0x00008DAC
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

		// Token: 0x1700012A RID: 298
		// (get) Token: 0x06000336 RID: 822 RVA: 0x0000ACF4 File Offset: 0x00008EF4
		// (set) Token: 0x06000337 RID: 823 RVA: 0x0000ACFC File Offset: 0x00008EFC
		public int Id { get; private set; }

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x06000338 RID: 824 RVA: 0x0000AD05 File Offset: 0x00008F05
		// (set) Token: 0x06000339 RID: 825 RVA: 0x0000AD0D File Offset: 0x00008F0D
		public bool Type { get; private set; }

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x0600033A RID: 826 RVA: 0x0000AD16 File Offset: 0x00008F16
		// (set) Token: 0x0600033B RID: 827 RVA: 0x0000AD1E File Offset: 0x00008F1E
		public string Projectile { get; private set; }

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x0600033C RID: 828 RVA: 0x0000AD27 File Offset: 0x00008F27
		// (set) Token: 0x0600033D RID: 829 RVA: 0x0000AD2F File Offset: 0x00008F2F
		public int ShootType { get; private set; }

		// Token: 0x1700012E RID: 302
		// (get) Token: 0x0600033E RID: 830 RVA: 0x0000AD38 File Offset: 0x00008F38
		// (set) Token: 0x0600033F RID: 831 RVA: 0x0000AD40 File Offset: 0x00008F40
		public int ParentPiece { get; private set; }

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x06000340 RID: 832 RVA: 0x0000AD49 File Offset: 0x00008F49
		// (set) Token: 0x06000341 RID: 833 RVA: 0x0000AD51 File Offset: 0x00008F51
		public bool AddParentAngle { get; private set; }

		// Token: 0x17000130 RID: 304
		// (get) Token: 0x06000342 RID: 834 RVA: 0x0000AD5A File Offset: 0x00008F5A
		// (set) Token: 0x06000343 RID: 835 RVA: 0x0000AD62 File Offset: 0x00008F62
		public bool LastWave { get; private set; }

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x06000344 RID: 836 RVA: 0x0000AD6B File Offset: 0x00008F6B
		// (set) Token: 0x06000345 RID: 837 RVA: 0x0000AD73 File Offset: 0x00008F73
		public int FollowPiece { get; private set; }

		// Token: 0x17000132 RID: 306
		// (get) Token: 0x06000346 RID: 838 RVA: 0x0000AD7C File Offset: 0x00008F7C
		// (set) Token: 0x06000347 RID: 839 RVA: 0x0000AD84 File Offset: 0x00008F84
		public int ShootEnd { get; private set; }

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x06000348 RID: 840 RVA: 0x0000AD8D File Offset: 0x00008F8D
		// (set) Token: 0x06000349 RID: 841 RVA: 0x0000AD95 File Offset: 0x00008F95
		public int HitAmount { get; private set; }

		// Token: 0x17000134 RID: 308
		// (get) Token: 0x0600034A RID: 842 RVA: 0x0000AD9E File Offset: 0x00008F9E
		// (set) Token: 0x0600034B RID: 843 RVA: 0x0000ADA6 File Offset: 0x00008FA6
		public int HitInterval { get; private set; }

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x0600034C RID: 844 RVA: 0x0000ADAF File Offset: 0x00008FAF
		// (set) Token: 0x0600034D RID: 845 RVA: 0x0000ADB7 File Offset: 0x00008FB7
		public bool ZeroHitNotDie { get; private set; }

		// Token: 0x17000136 RID: 310
		// (get) Token: 0x0600034E RID: 846 RVA: 0x0000ADC0 File Offset: 0x00008FC0
		// (set) Token: 0x0600034F RID: 847 RVA: 0x0000ADC8 File Offset: 0x00008FC8
		public float[][] Scale { get; private set; }

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x06000350 RID: 848 RVA: 0x0000ADD1 File Offset: 0x00008FD1
		// (set) Token: 0x06000351 RID: 849 RVA: 0x0000ADD9 File Offset: 0x00008FD9
		public int[][] Color { get; private set; }

		// Token: 0x17000138 RID: 312
		// (get) Token: 0x06000352 RID: 850 RVA: 0x0000ADE2 File Offset: 0x00008FE2
		// (set) Token: 0x06000353 RID: 851 RVA: 0x0000ADEA File Offset: 0x00008FEA
		public int RootType { get; private set; }

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x06000354 RID: 852 RVA: 0x0000ADF3 File Offset: 0x00008FF3
		// (set) Token: 0x06000355 RID: 853 RVA: 0x0000ADFB File Offset: 0x00008FFB
		public float[][] X { get; private set; }

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x06000356 RID: 854 RVA: 0x0000AE04 File Offset: 0x00009004
		// (set) Token: 0x06000357 RID: 855 RVA: 0x0000AE0C File Offset: 0x0000900C
		public float[][] Y { get; private set; }

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x06000358 RID: 856 RVA: 0x0000AE15 File Offset: 0x00009015
		// (set) Token: 0x06000359 RID: 857 RVA: 0x0000AE1D File Offset: 0x0000901D
		public float[][] Radius { get; private set; }

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x0600035A RID: 858 RVA: 0x0000AE26 File Offset: 0x00009026
		// (set) Token: 0x0600035B RID: 859 RVA: 0x0000AE2E File Offset: 0x0000902E
		public float[][] RadiusA { get; private set; }

		// Token: 0x1700013D RID: 317
		// (get) Token: 0x0600035C RID: 860 RVA: 0x0000AE37 File Offset: 0x00009037
		// (set) Token: 0x0600035D RID: 861 RVA: 0x0000AE3F File Offset: 0x0000903F
		public int Aim { get; private set; }

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x0600035E RID: 862 RVA: 0x0000AE48 File Offset: 0x00009048
		// (set) Token: 0x0600035F RID: 863 RVA: 0x0000AE50 File Offset: 0x00009050
		public int StartTime { get; private set; }

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x06000360 RID: 864 RVA: 0x0000AE59 File Offset: 0x00009059
		// (set) Token: 0x06000361 RID: 865 RVA: 0x0000AE61 File Offset: 0x00009061
		public int GInterval { get; private set; }

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x06000362 RID: 866 RVA: 0x0000AE6A File Offset: 0x0000906A
		// (set) Token: 0x06000363 RID: 867 RVA: 0x0000AE72 File Offset: 0x00009072
		public int Group { get; private set; }

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x06000364 RID: 868 RVA: 0x0000AE7B File Offset: 0x0000907B
		// (set) Token: 0x06000365 RID: 869 RVA: 0x0000AE83 File Offset: 0x00009083
		public int[][] Way { get; private set; }

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x06000366 RID: 870 RVA: 0x0000AE8C File Offset: 0x0000908C
		// (set) Token: 0x06000367 RID: 871 RVA: 0x0000AE94 File Offset: 0x00009094
		public float[][] GAngle { get; private set; }

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x06000368 RID: 872 RVA: 0x0000AE9D File Offset: 0x0000909D
		// (set) Token: 0x06000369 RID: 873 RVA: 0x0000AEA5 File Offset: 0x000090A5
		public float[][] Range { get; private set; }

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x0600036A RID: 874 RVA: 0x0000AEAE File Offset: 0x000090AE
		// (set) Token: 0x0600036B RID: 875 RVA: 0x0000AEB6 File Offset: 0x000090B6
		public int[][] Life { get; private set; }

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x0600036C RID: 876 RVA: 0x0000AEBF File Offset: 0x000090BF
		// (set) Token: 0x0600036D RID: 877 RVA: 0x0000AEC7 File Offset: 0x000090C7
		public int LaserLastWave { get; private set; }

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x0600036E RID: 878 RVA: 0x0000AED0 File Offset: 0x000090D0
		// (set) Token: 0x0600036F RID: 879 RVA: 0x0000AED8 File Offset: 0x000090D8
		public float[][] StartSpeed { get; private set; }

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x06000370 RID: 880 RVA: 0x0000AEE1 File Offset: 0x000090E1
		// (set) Token: 0x06000371 RID: 881 RVA: 0x0000AEE9 File Offset: 0x000090E9
		public float[][] StartAcc { get; private set; }

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x06000372 RID: 882 RVA: 0x0000AEF2 File Offset: 0x000090F2
		// (set) Token: 0x06000373 RID: 883 RVA: 0x0000AEFA File Offset: 0x000090FA
		public float[][] StartAccAngle { get; private set; }

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x06000374 RID: 884 RVA: 0x0000AF03 File Offset: 0x00009103
		// (set) Token: 0x06000375 RID: 885 RVA: 0x0000AF0B File Offset: 0x0000910B
		public int[][][] EvStart { get; private set; }

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x06000376 RID: 886 RVA: 0x0000AF14 File Offset: 0x00009114
		// (set) Token: 0x06000377 RID: 887 RVA: 0x0000AF1C File Offset: 0x0000911C
		public int[][][] EvDuration { get; private set; }

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x06000378 RID: 888 RVA: 0x0000AF25 File Offset: 0x00009125
		// (set) Token: 0x06000379 RID: 889 RVA: 0x0000AF2D File Offset: 0x0000912D
		public float[][][] EvNumber { get; private set; }

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x0600037A RID: 890 RVA: 0x0000AF36 File Offset: 0x00009136
		// (set) Token: 0x0600037B RID: 891 RVA: 0x0000AF3E File Offset: 0x0000913E
		public int[][] EvType { get; private set; }

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x0600037C RID: 892 RVA: 0x0000AF47 File Offset: 0x00009147
		// (set) Token: 0x0600037D RID: 893 RVA: 0x0000AF4F File Offset: 0x0000914F
		public Vector3 VanishV3 { get; private set; }

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x0600037E RID: 894 RVA: 0x0000AF58 File Offset: 0x00009158
		// (set) Token: 0x0600037F RID: 895 RVA: 0x0000AF60 File Offset: 0x00009160
		public string LaunchSfx { get; private set; }

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x06000380 RID: 896 RVA: 0x0000AF69 File Offset: 0x00009169
		// (set) Token: 0x06000381 RID: 897 RVA: 0x0000AF71 File Offset: 0x00009171
		public string HitBodySfx { get; private set; }

		// Token: 0x17000150 RID: 336
		// (get) Token: 0x06000382 RID: 898 RVA: 0x0000AF7A File Offset: 0x0000917A
		// (set) Token: 0x06000383 RID: 899 RVA: 0x0000AF82 File Offset: 0x00009182
		public float HitAnimationSpeed { get; private set; }

		// Token: 0x06000384 RID: 900 RVA: 0x0000AF8B File Offset: 0x0000918B
		public static IReadOnlyList<PieceConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<PieceConfig>(PieceConfig._data);
		}

		// Token: 0x06000385 RID: 901 RVA: 0x0000AF9C File Offset: 0x0000919C
		public static PieceConfig FromId(int Id)
		{
			ConfigDataManager.Initialize();
			PieceConfig pieceConfig;
			return (!PieceConfig._IdTable.TryGetValue(Id, out pieceConfig)) ? null : pieceConfig;
		}

		// Token: 0x06000386 RID: 902 RVA: 0x0000AFC8 File Offset: 0x000091C8
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

		// Token: 0x06000387 RID: 903 RVA: 0x0000B690 File Offset: 0x00009890
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

		// Token: 0x06000388 RID: 904 RVA: 0x0000BA78 File Offset: 0x00009C78
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

		// Token: 0x040001C7 RID: 455
		private static PieceConfig[] _data = Array.Empty<PieceConfig>();

		// Token: 0x040001C8 RID: 456
		private static Dictionary<int, PieceConfig> _IdTable = Enumerable.ToDictionary<PieceConfig, int>(PieceConfig._data, (PieceConfig elem) => elem.Id);
	}
}
