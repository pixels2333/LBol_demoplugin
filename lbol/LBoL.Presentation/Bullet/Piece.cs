using System;
using System.Collections.Generic;
using LBoL.ConfigData;
using UnityEngine;
namespace LBoL.Presentation.Bullet
{
	public class Piece
	{
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
		public PieceConfig Config { get; }
		public int Id { get; }
		public bool Type { get; }
		public string ProjectileName { get; }
		public int ShootType { get; }
		public int ParentPiece { get; }
		public bool AddParentAngle { get; }
		public bool IsParentPieceType2 { get; set; }
		public bool IsParentPieceType3 { get; set; }
		public List<Piece> ChildPiecesType2 { get; } = new List<Piece>();
		public List<Piece> ChildPiecesType3 { get; } = new List<Piece>();
		public int FollowPiece { get; }
		public int ShootEnd { get; }
		public int HitAmount { get; }
		public float HitInterval { get; }
		public bool ZeroHitNotDie { get; }
		public int[][] Life { get; }
		public float[][] Scale { get; }
		public Vector3 VanishV3 { get; }
		public int[][] Color { get; }
		public int RootType { get; }
		public float[][] X { get; }
		public float[][] Y { get; }
		public float[][] Radius { get; }
		public float[][] RadiusA { get; }
		public int Aim { get; }
		public int StartTime { get; }
		public int GInterval { get; }
		public int Group { get; }
		public int[][] Way { get; }
		public float[][] GAngle { get; }
		public float[][] Range { get; }
		public float[][] StartSpeed { get; }
		public float[][] StartAcc { get; }
		public float[][] StartAccAngle { get; }
		public int[][][] EvStart { get; }
		public int[][][] EvDuration { get; }
		public float[][][] EvNumber { get; }
		public int[][] EvType { get; }
		public float LaserLastWave { get; }
		public bool LastWave { get; }
		public string LaunchSfx { get; }
		public string HitBodySfx { get; }
		public float HitAnimationSpeed { get; }
	}
}
