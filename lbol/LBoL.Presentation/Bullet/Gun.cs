using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.ConfigData;
using LBoL.Presentation.Units;
using UnityEngine;
namespace LBoL.Presentation.Bullet
{
	public class Gun : MonoBehaviour
	{
		public bool BelongToPlayer { get; set; }
		public UnitView Target { get; private set; }
		public List<UnitView> Targets { get; private set; }
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
		public List<Piece> Pieces { get; } = new List<Piece>();
		public float ShootEnd { get; set; }
		public List<Launcher> Launchers { get; } = new List<Launcher>();
		public List<Launcher> ChildLaunchers { get; } = new List<Launcher>();
		public bool LastWaveHitFlag { get; private set; }
		public bool NoPiece { get; private set; }
		public float StartTime { get; set; }
		public Vector2 ShootV2 { get; set; }
		public bool HasAnimation { get; private set; }
		public void SetGun(GunConfig config)
		{
			this.Id = config.Id;
			this.Name = config.Name;
			this.Spell = config.Spell;
			this.Sequence = config.Sequence;
			this.Animation = config.Animation;
			this.ForceHitTime = config.ForceHitTime;
			this.ForceHitAnimation = config.ForceHitAnimation;
			this.ForceHitAnimationSpeed = config.ForceHitAnimationSpeed;
			this.ForceShowEndStartTime = config.ForceShowEndStartTime;
			this.Shooter = config.Shooter;
			int num = this.Id * 100;
			bool flag = false;
			int num2 = 0;
			for (int i = num; i < num + 100; i++)
			{
				PieceConfig pieceConfig = PieceConfig.FromId(i);
				if (pieceConfig == null)
				{
					break;
				}
				num2++;
				Piece piece = new Piece(pieceConfig);
				this.Pieces.Add(piece);
				if (pieceConfig.LastWave)
				{
					flag = true;
				}
			}
			if (num2 == 0)
			{
				this.NoPiece = true;
				if (this.ForceHitTime == null)
				{
					Debug.LogWarning("Gun has no piece: " + this.Name);
				}
			}
			else if (!flag && this.ForceHitTime == null)
			{
				Debug.LogError("Gun has no LastWave: " + this.Name);
			}
			using (List<Piece>.Enumerator enumerator = this.Pieces.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.ShootType == 0)
					{
						this.HasAnimation = true;
						break;
					}
				}
			}
			foreach (Piece piece2 in this.Pieces)
			{
				if (piece2.ShootType == 2)
				{
					Piece pieceById = this.GetPieceById(piece2.ParentPiece);
					if (pieceById == null)
					{
						throw new InvalidOperationException("Gun内没有对应的ParentPiece，GunId: " + this.Id.ToString());
					}
					pieceById.IsParentPieceType2 = true;
					pieceById.ChildPiecesType2.Add(piece2);
				}
			}
			foreach (Piece piece3 in this.Pieces)
			{
				if (piece3.ShootType == 3)
				{
					Piece pieceById2 = this.GetPieceById(piece3.ParentPiece);
					if (pieceById2 == null)
					{
						throw new InvalidOperationException("Gun内没有对应的ParentPiece，GunId: " + this.Id.ToString());
					}
					pieceById2.IsParentPieceType3 = true;
					pieceById2.ChildPiecesType3.Add(piece3);
				}
			}
		}
		public void Aim(bool belongToPlayer, UnitView target, List<UnitView> targets)
		{
			this.BelongToPlayer = belongToPlayer;
			this.Target = target;
			this.Targets = targets;
		}
		public void LastWaveHit()
		{
			this.LastWaveHitFlag = true;
		}
		public Piece GetPieceById(int id)
		{
			return Enumerable.FirstOrDefault<Piece>(this.Pieces, (Piece piece) => piece.Id == id);
		}
	}
}
