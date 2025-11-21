using System;
using System.Collections.Generic;
using LBoL.ConfigData;
using UnityEngine;

namespace LBoL.Presentation.Bullet
{
	// Token: 0x02000116 RID: 278
	public class Piece
	{
		// Token: 0x06000F6C RID: 3948 RVA: 0x000489B0 File Offset: 0x00046BB0
		public Piece(PieceConfig config)
		{
			this.Config = config;
			this.Id = config.Id;
			this.Type = config.Type;
			this.ProjectileName = config.Projectile;
			this.ShootType = config.ShootType;
			this.ShootEnd = config.ShootEnd;
			this.HitAmount = config.HitAmount;
			this.HitInterval = (float)config.HitInterval;
			this.ZeroHitNotDie = config.ZeroHitNotDie;
			this.Life = config.Life;
			this.Scale = config.Scale;
			this.VanishV3 = config.VanishV3;
			this.Color = config.Color;
			this.RootType = config.RootType;
			this.ParentPiece = config.Id / 100 * 100 + config.ParentPiece;
			this.AddParentAngle = config.AddParentAngle;
			this.FollowPiece = config.FollowPiece;
			this.X = config.X;
			this.Y = config.Y;
			this.Radius = config.Radius;
			this.RadiusA = config.RadiusA;
			this.Way = config.Way;
			this.GAngle = config.GAngle;
			this.Range = config.Range;
			this.StartSpeed = config.StartSpeed;
			this.StartAcc = config.StartAcc;
			this.StartAccAngle = config.StartAccAngle;
			this.EvStart = config.EvStart;
			this.EvDuration = config.EvDuration;
			this.EvNumber = config.EvNumber;
			this.EvType = config.EvType;
			this.Aim = config.Aim;
			this.StartTime = config.StartTime;
			this.GInterval = config.GInterval;
			this.Group = config.Group;
			this.LaserLastWave = (float)config.LaserLastWave;
			this.LastWave = config.LastWave;
			this.LaunchSfx = config.LaunchSfx;
			this.HitBodySfx = config.HitBodySfx;
			this.HitAnimationSpeed = config.HitAnimationSpeed;
		}

		// Token: 0x170002C2 RID: 706
		// (get) Token: 0x06000F6D RID: 3949 RVA: 0x00048BC3 File Offset: 0x00046DC3
		public PieceConfig Config { get; }

		// Token: 0x170002C3 RID: 707
		// (get) Token: 0x06000F6E RID: 3950 RVA: 0x00048BCB File Offset: 0x00046DCB
		public int Id { get; }

		// Token: 0x170002C4 RID: 708
		// (get) Token: 0x06000F6F RID: 3951 RVA: 0x00048BD3 File Offset: 0x00046DD3
		public bool Type { get; }

		// Token: 0x170002C5 RID: 709
		// (get) Token: 0x06000F70 RID: 3952 RVA: 0x00048BDB File Offset: 0x00046DDB
		public string ProjectileName { get; }

		// Token: 0x170002C6 RID: 710
		// (get) Token: 0x06000F71 RID: 3953 RVA: 0x00048BE3 File Offset: 0x00046DE3
		public int ShootType { get; }

		// Token: 0x170002C7 RID: 711
		// (get) Token: 0x06000F72 RID: 3954 RVA: 0x00048BEB File Offset: 0x00046DEB
		public int ParentPiece { get; }

		// Token: 0x170002C8 RID: 712
		// (get) Token: 0x06000F73 RID: 3955 RVA: 0x00048BF3 File Offset: 0x00046DF3
		public bool AddParentAngle { get; }

		// Token: 0x170002C9 RID: 713
		// (get) Token: 0x06000F74 RID: 3956 RVA: 0x00048BFB File Offset: 0x00046DFB
		// (set) Token: 0x06000F75 RID: 3957 RVA: 0x00048C03 File Offset: 0x00046E03
		public bool IsParentPieceType2 { get; set; }

		// Token: 0x170002CA RID: 714
		// (get) Token: 0x06000F76 RID: 3958 RVA: 0x00048C0C File Offset: 0x00046E0C
		// (set) Token: 0x06000F77 RID: 3959 RVA: 0x00048C14 File Offset: 0x00046E14
		public bool IsParentPieceType3 { get; set; }

		// Token: 0x170002CB RID: 715
		// (get) Token: 0x06000F78 RID: 3960 RVA: 0x00048C1D File Offset: 0x00046E1D
		public List<Piece> ChildPiecesType2 { get; } = new List<Piece>();

		// Token: 0x170002CC RID: 716
		// (get) Token: 0x06000F79 RID: 3961 RVA: 0x00048C25 File Offset: 0x00046E25
		public List<Piece> ChildPiecesType3 { get; } = new List<Piece>();

		// Token: 0x170002CD RID: 717
		// (get) Token: 0x06000F7A RID: 3962 RVA: 0x00048C2D File Offset: 0x00046E2D
		public int FollowPiece { get; }

		// Token: 0x170002CE RID: 718
		// (get) Token: 0x06000F7B RID: 3963 RVA: 0x00048C35 File Offset: 0x00046E35
		public int ShootEnd { get; }

		// Token: 0x170002CF RID: 719
		// (get) Token: 0x06000F7C RID: 3964 RVA: 0x00048C3D File Offset: 0x00046E3D
		public int HitAmount { get; }

		// Token: 0x170002D0 RID: 720
		// (get) Token: 0x06000F7D RID: 3965 RVA: 0x00048C45 File Offset: 0x00046E45
		public float HitInterval { get; }

		// Token: 0x170002D1 RID: 721
		// (get) Token: 0x06000F7E RID: 3966 RVA: 0x00048C4D File Offset: 0x00046E4D
		public bool ZeroHitNotDie { get; }

		// Token: 0x170002D2 RID: 722
		// (get) Token: 0x06000F7F RID: 3967 RVA: 0x00048C55 File Offset: 0x00046E55
		public int[][] Life { get; }

		// Token: 0x170002D3 RID: 723
		// (get) Token: 0x06000F80 RID: 3968 RVA: 0x00048C5D File Offset: 0x00046E5D
		public float[][] Scale { get; }

		// Token: 0x170002D4 RID: 724
		// (get) Token: 0x06000F81 RID: 3969 RVA: 0x00048C65 File Offset: 0x00046E65
		public Vector3 VanishV3 { get; }

		// Token: 0x170002D5 RID: 725
		// (get) Token: 0x06000F82 RID: 3970 RVA: 0x00048C6D File Offset: 0x00046E6D
		public int[][] Color { get; }

		// Token: 0x170002D6 RID: 726
		// (get) Token: 0x06000F83 RID: 3971 RVA: 0x00048C75 File Offset: 0x00046E75
		public int RootType { get; }

		// Token: 0x170002D7 RID: 727
		// (get) Token: 0x06000F84 RID: 3972 RVA: 0x00048C7D File Offset: 0x00046E7D
		public float[][] X { get; }

		// Token: 0x170002D8 RID: 728
		// (get) Token: 0x06000F85 RID: 3973 RVA: 0x00048C85 File Offset: 0x00046E85
		public float[][] Y { get; }

		// Token: 0x170002D9 RID: 729
		// (get) Token: 0x06000F86 RID: 3974 RVA: 0x00048C8D File Offset: 0x00046E8D
		public float[][] Radius { get; }

		// Token: 0x170002DA RID: 730
		// (get) Token: 0x06000F87 RID: 3975 RVA: 0x00048C95 File Offset: 0x00046E95
		public float[][] RadiusA { get; }

		// Token: 0x170002DB RID: 731
		// (get) Token: 0x06000F88 RID: 3976 RVA: 0x00048C9D File Offset: 0x00046E9D
		public int Aim { get; }

		// Token: 0x170002DC RID: 732
		// (get) Token: 0x06000F89 RID: 3977 RVA: 0x00048CA5 File Offset: 0x00046EA5
		public int StartTime { get; }

		// Token: 0x170002DD RID: 733
		// (get) Token: 0x06000F8A RID: 3978 RVA: 0x00048CAD File Offset: 0x00046EAD
		public int GInterval { get; }

		// Token: 0x170002DE RID: 734
		// (get) Token: 0x06000F8B RID: 3979 RVA: 0x00048CB5 File Offset: 0x00046EB5
		public int Group { get; }

		// Token: 0x170002DF RID: 735
		// (get) Token: 0x06000F8C RID: 3980 RVA: 0x00048CBD File Offset: 0x00046EBD
		public int[][] Way { get; }

		// Token: 0x170002E0 RID: 736
		// (get) Token: 0x06000F8D RID: 3981 RVA: 0x00048CC5 File Offset: 0x00046EC5
		public float[][] GAngle { get; }

		// Token: 0x170002E1 RID: 737
		// (get) Token: 0x06000F8E RID: 3982 RVA: 0x00048CCD File Offset: 0x00046ECD
		public float[][] Range { get; }

		// Token: 0x170002E2 RID: 738
		// (get) Token: 0x06000F8F RID: 3983 RVA: 0x00048CD5 File Offset: 0x00046ED5
		public float[][] StartSpeed { get; }

		// Token: 0x170002E3 RID: 739
		// (get) Token: 0x06000F90 RID: 3984 RVA: 0x00048CDD File Offset: 0x00046EDD
		public float[][] StartAcc { get; }

		// Token: 0x170002E4 RID: 740
		// (get) Token: 0x06000F91 RID: 3985 RVA: 0x00048CE5 File Offset: 0x00046EE5
		public float[][] StartAccAngle { get; }

		// Token: 0x170002E5 RID: 741
		// (get) Token: 0x06000F92 RID: 3986 RVA: 0x00048CED File Offset: 0x00046EED
		public int[][][] EvStart { get; }

		// Token: 0x170002E6 RID: 742
		// (get) Token: 0x06000F93 RID: 3987 RVA: 0x00048CF5 File Offset: 0x00046EF5
		public int[][][] EvDuration { get; }

		// Token: 0x170002E7 RID: 743
		// (get) Token: 0x06000F94 RID: 3988 RVA: 0x00048CFD File Offset: 0x00046EFD
		public float[][][] EvNumber { get; }

		// Token: 0x170002E8 RID: 744
		// (get) Token: 0x06000F95 RID: 3989 RVA: 0x00048D05 File Offset: 0x00046F05
		public int[][] EvType { get; }

		// Token: 0x170002E9 RID: 745
		// (get) Token: 0x06000F96 RID: 3990 RVA: 0x00048D0D File Offset: 0x00046F0D
		public float LaserLastWave { get; }

		// Token: 0x170002EA RID: 746
		// (get) Token: 0x06000F97 RID: 3991 RVA: 0x00048D15 File Offset: 0x00046F15
		public bool LastWave { get; }

		// Token: 0x170002EB RID: 747
		// (get) Token: 0x06000F98 RID: 3992 RVA: 0x00048D1D File Offset: 0x00046F1D
		public string LaunchSfx { get; }

		// Token: 0x170002EC RID: 748
		// (get) Token: 0x06000F99 RID: 3993 RVA: 0x00048D25 File Offset: 0x00046F25
		public string HitBodySfx { get; }

		// Token: 0x170002ED RID: 749
		// (get) Token: 0x06000F9A RID: 3994 RVA: 0x00048D2D File Offset: 0x00046F2D
		public float HitAnimationSpeed { get; }
	}
}
