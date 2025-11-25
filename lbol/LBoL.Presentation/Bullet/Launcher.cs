using System;
using System.Collections.Generic;
using UnityEngine;
namespace LBoL.Presentation.Bullet
{
	public class Launcher
	{
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
		public Gun Gun { get; }
		public Piece Piece { get; }
		public Launcher ParentLauncher { get; }
		public Projectile Projectile { get; set; }
		public bool Active { get; set; }
		public Vector2 V2 { get; set; }
		public int Delay { get; }
		public int Tick { get; set; }
		public float Angle { get; set; }
		public float Speed { get; }
		public float SpeedAcc { get; }
		public float AccAngle { get; }
		public int GroupIndex { get; }
		public int WayIndex { get; }
		public List<BulletEvent> BulletEvents { get; }
		public Vector2 DeathPositionV2 { get; set; }
		public float DeathAngle { get; set; }
		public int Color { get; set; }
		public int Aim { get; set; }
		public float Scale { get; set; }
		public int LifeTime { get; set; }
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
