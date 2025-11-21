using System;
using System.Collections.Generic;
using UnityEngine;

namespace LBoL.Presentation.Bullet
{
	// Token: 0x02000114 RID: 276
	public class Launcher
	{
		// Token: 0x06000F46 RID: 3910 RVA: 0x000485FC File Offset: 0x000467FC
		public Launcher(Gun gun, Piece piece, Launcher parentLauncher, Vector2 v2, int delay, float speed, float angle, float startAcc, float startAccAngle, int groupIndex, int wayIndex, int color, int aim, float scale, int lifeTime)
		{
			this.Gun = gun;
			this.Piece = piece;
			this.ParentLauncher = parentLauncher;
			this.Active = true;
			this.V2 = v2;
			this.Delay = delay;
			this.Speed = speed;
			this.Angle = angle;
			this.SpeedAcc = startAcc;
			this.AccAngle = startAccAngle;
			this.GroupIndex = groupIndex;
			this.WayIndex = wayIndex;
			this.Color = color;
			this.BulletEvents = new List<BulletEvent>();
			for (int i = 0; i < this.Piece.EvStart.Length; i++)
			{
				BulletEvent bulletEvent = new BulletEvent(Launcher.EvTimeArrayCalcu(this.Piece.EvStart[i], groupIndex, wayIndex), Launcher.EvTimeArrayCalcu(this.Piece.EvDuration[i], groupIndex, wayIndex), this.Piece.EvNumber[i], this.Piece.EvType[i]);
				this.BulletEvents.Add(bulletEvent);
			}
			this.Tick = this.Delay;
			this.Aim = aim;
			this.Scale = scale;
			this.LifeTime = lifeTime;
		}

		// Token: 0x170002AD RID: 685
		// (get) Token: 0x06000F47 RID: 3911 RVA: 0x00048712 File Offset: 0x00046912
		public Gun Gun { get; }

		// Token: 0x170002AE RID: 686
		// (get) Token: 0x06000F48 RID: 3912 RVA: 0x0004871A File Offset: 0x0004691A
		public Piece Piece { get; }

		// Token: 0x170002AF RID: 687
		// (get) Token: 0x06000F49 RID: 3913 RVA: 0x00048722 File Offset: 0x00046922
		public Launcher ParentLauncher { get; }

		// Token: 0x170002B0 RID: 688
		// (get) Token: 0x06000F4A RID: 3914 RVA: 0x0004872A File Offset: 0x0004692A
		// (set) Token: 0x06000F4B RID: 3915 RVA: 0x00048732 File Offset: 0x00046932
		public Projectile Projectile { get; set; }

		// Token: 0x170002B1 RID: 689
		// (get) Token: 0x06000F4C RID: 3916 RVA: 0x0004873B File Offset: 0x0004693B
		// (set) Token: 0x06000F4D RID: 3917 RVA: 0x00048743 File Offset: 0x00046943
		public bool Active { get; set; }

		// Token: 0x170002B2 RID: 690
		// (get) Token: 0x06000F4E RID: 3918 RVA: 0x0004874C File Offset: 0x0004694C
		// (set) Token: 0x06000F4F RID: 3919 RVA: 0x00048754 File Offset: 0x00046954
		public Vector2 V2 { get; set; }

		// Token: 0x170002B3 RID: 691
		// (get) Token: 0x06000F50 RID: 3920 RVA: 0x0004875D File Offset: 0x0004695D
		public int Delay { get; }

		// Token: 0x170002B4 RID: 692
		// (get) Token: 0x06000F51 RID: 3921 RVA: 0x00048765 File Offset: 0x00046965
		// (set) Token: 0x06000F52 RID: 3922 RVA: 0x0004876D File Offset: 0x0004696D
		public int Tick { get; set; }

		// Token: 0x170002B5 RID: 693
		// (get) Token: 0x06000F53 RID: 3923 RVA: 0x00048776 File Offset: 0x00046976
		// (set) Token: 0x06000F54 RID: 3924 RVA: 0x0004877E File Offset: 0x0004697E
		public float Angle { get; set; }

		// Token: 0x170002B6 RID: 694
		// (get) Token: 0x06000F55 RID: 3925 RVA: 0x00048787 File Offset: 0x00046987
		public float Speed { get; }

		// Token: 0x170002B7 RID: 695
		// (get) Token: 0x06000F56 RID: 3926 RVA: 0x0004878F File Offset: 0x0004698F
		public float SpeedAcc { get; }

		// Token: 0x170002B8 RID: 696
		// (get) Token: 0x06000F57 RID: 3927 RVA: 0x00048797 File Offset: 0x00046997
		public float AccAngle { get; }

		// Token: 0x170002B9 RID: 697
		// (get) Token: 0x06000F58 RID: 3928 RVA: 0x0004879F File Offset: 0x0004699F
		public int GroupIndex { get; }

		// Token: 0x170002BA RID: 698
		// (get) Token: 0x06000F59 RID: 3929 RVA: 0x000487A7 File Offset: 0x000469A7
		public int WayIndex { get; }

		// Token: 0x170002BB RID: 699
		// (get) Token: 0x06000F5A RID: 3930 RVA: 0x000487AF File Offset: 0x000469AF
		public List<BulletEvent> BulletEvents { get; }

		// Token: 0x170002BC RID: 700
		// (get) Token: 0x06000F5B RID: 3931 RVA: 0x000487B7 File Offset: 0x000469B7
		// (set) Token: 0x06000F5C RID: 3932 RVA: 0x000487BF File Offset: 0x000469BF
		public Vector2 DeathPositionV2 { get; set; }

		// Token: 0x170002BD RID: 701
		// (get) Token: 0x06000F5D RID: 3933 RVA: 0x000487C8 File Offset: 0x000469C8
		// (set) Token: 0x06000F5E RID: 3934 RVA: 0x000487D0 File Offset: 0x000469D0
		public float DeathAngle { get; set; }

		// Token: 0x170002BE RID: 702
		// (get) Token: 0x06000F5F RID: 3935 RVA: 0x000487D9 File Offset: 0x000469D9
		// (set) Token: 0x06000F60 RID: 3936 RVA: 0x000487E1 File Offset: 0x000469E1
		public int Color { get; set; }

		// Token: 0x170002BF RID: 703
		// (get) Token: 0x06000F61 RID: 3937 RVA: 0x000487EA File Offset: 0x000469EA
		// (set) Token: 0x06000F62 RID: 3938 RVA: 0x000487F2 File Offset: 0x000469F2
		public int Aim { get; set; }

		// Token: 0x170002C0 RID: 704
		// (get) Token: 0x06000F63 RID: 3939 RVA: 0x000487FB File Offset: 0x000469FB
		// (set) Token: 0x06000F64 RID: 3940 RVA: 0x00048803 File Offset: 0x00046A03
		public float Scale { get; set; }

		// Token: 0x170002C1 RID: 705
		// (get) Token: 0x06000F65 RID: 3941 RVA: 0x0004880C File Offset: 0x00046A0C
		// (set) Token: 0x06000F66 RID: 3942 RVA: 0x00048814 File Offset: 0x00046A14
		public int LifeTime { get; set; }

		// Token: 0x06000F67 RID: 3943 RVA: 0x00048820 File Offset: 0x00046A20
		private static int EvTimeArrayCalcu(int[][] ps, int groupID, int wayID)
		{
			int num;
			switch (ps.Length)
			{
			case 0:
				num = 0;
				break;
			case 1:
				num = Launcher.RandomDataCalcu(ps[0]);
				break;
			case 2:
				num = Launcher.RandomDataCalcu(ps[0]) + Launcher.RandomDataCalcu(ps[1]) * groupID;
				break;
			case 3:
				num = Launcher.RandomDataCalcu(ps[0]) + Launcher.RandomDataCalcu(ps[1]) * groupID + Launcher.RandomDataCalcu(ps[2]) * wayID;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return num;
		}

		// Token: 0x06000F68 RID: 3944 RVA: 0x00048898 File Offset: 0x00046A98
		private static int RandomDataCalcu(int[] ps)
		{
			int num;
			switch (ps.Length)
			{
			case 0:
				num = 0;
				break;
			case 1:
				num = ps[0];
				break;
			case 2:
				num = ps[0] + Random.Range(-ps[1], ps[1] + 1);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return num;
		}
	}
}
