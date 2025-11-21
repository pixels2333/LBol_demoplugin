using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.ConfigData;
using LBoL.Presentation.Units;
using UnityEngine;

namespace LBoL.Presentation.Bullet
{
	// Token: 0x02000110 RID: 272
	public class Gun : MonoBehaviour
	{
		// Token: 0x17000291 RID: 657
		// (get) Token: 0x06000EEB RID: 3819 RVA: 0x00046A4D File Offset: 0x00044C4D
		// (set) Token: 0x06000EEC RID: 3820 RVA: 0x00046A55 File Offset: 0x00044C55
		public bool BelongToPlayer { get; set; }

		// Token: 0x17000292 RID: 658
		// (get) Token: 0x06000EED RID: 3821 RVA: 0x00046A5E File Offset: 0x00044C5E
		// (set) Token: 0x06000EEE RID: 3822 RVA: 0x00046A66 File Offset: 0x00044C66
		public UnitView Target { get; private set; }

		// Token: 0x17000293 RID: 659
		// (get) Token: 0x06000EEF RID: 3823 RVA: 0x00046A6F File Offset: 0x00044C6F
		// (set) Token: 0x06000EF0 RID: 3824 RVA: 0x00046A77 File Offset: 0x00044C77
		public List<UnitView> Targets { get; private set; }

		// Token: 0x17000294 RID: 660
		// (get) Token: 0x06000EF1 RID: 3825 RVA: 0x00046A80 File Offset: 0x00044C80
		// (set) Token: 0x06000EF2 RID: 3826 RVA: 0x00046A88 File Offset: 0x00044C88
		public int Id { get; private set; }

		// Token: 0x17000295 RID: 661
		// (get) Token: 0x06000EF3 RID: 3827 RVA: 0x00046A91 File Offset: 0x00044C91
		// (set) Token: 0x06000EF4 RID: 3828 RVA: 0x00046A99 File Offset: 0x00044C99
		public string Name { get; private set; }

		// Token: 0x17000296 RID: 662
		// (get) Token: 0x06000EF5 RID: 3829 RVA: 0x00046AA2 File Offset: 0x00044CA2
		// (set) Token: 0x06000EF6 RID: 3830 RVA: 0x00046AAA File Offset: 0x00044CAA
		public string Spell { get; private set; }

		// Token: 0x17000297 RID: 663
		// (get) Token: 0x06000EF7 RID: 3831 RVA: 0x00046AB3 File Offset: 0x00044CB3
		// (set) Token: 0x06000EF8 RID: 3832 RVA: 0x00046ABB File Offset: 0x00044CBB
		public string Sequence { get; private set; }

		// Token: 0x17000298 RID: 664
		// (get) Token: 0x06000EF9 RID: 3833 RVA: 0x00046AC4 File Offset: 0x00044CC4
		// (set) Token: 0x06000EFA RID: 3834 RVA: 0x00046ACC File Offset: 0x00044CCC
		public string Animation { get; private set; }

		// Token: 0x17000299 RID: 665
		// (get) Token: 0x06000EFB RID: 3835 RVA: 0x00046AD5 File Offset: 0x00044CD5
		// (set) Token: 0x06000EFC RID: 3836 RVA: 0x00046ADD File Offset: 0x00044CDD
		public float? ForceHitTime { get; private set; }

		// Token: 0x1700029A RID: 666
		// (get) Token: 0x06000EFD RID: 3837 RVA: 0x00046AE6 File Offset: 0x00044CE6
		// (set) Token: 0x06000EFE RID: 3838 RVA: 0x00046AEE File Offset: 0x00044CEE
		public bool ForceHitAnimation { get; private set; }

		// Token: 0x1700029B RID: 667
		// (get) Token: 0x06000EFF RID: 3839 RVA: 0x00046AF7 File Offset: 0x00044CF7
		// (set) Token: 0x06000F00 RID: 3840 RVA: 0x00046AFF File Offset: 0x00044CFF
		public float ForceHitAnimationSpeed { get; private set; }

		// Token: 0x1700029C RID: 668
		// (get) Token: 0x06000F01 RID: 3841 RVA: 0x00046B08 File Offset: 0x00044D08
		// (set) Token: 0x06000F02 RID: 3842 RVA: 0x00046B10 File Offset: 0x00044D10
		public float? ForceShowEndStartTime { get; private set; }

		// Token: 0x1700029D RID: 669
		// (get) Token: 0x06000F03 RID: 3843 RVA: 0x00046B19 File Offset: 0x00044D19
		// (set) Token: 0x06000F04 RID: 3844 RVA: 0x00046B21 File Offset: 0x00044D21
		public string Shooter { get; private set; }

		// Token: 0x1700029E RID: 670
		// (get) Token: 0x06000F05 RID: 3845 RVA: 0x00046B2A File Offset: 0x00044D2A
		public List<Piece> Pieces { get; } = new List<Piece>();

		// Token: 0x1700029F RID: 671
		// (get) Token: 0x06000F06 RID: 3846 RVA: 0x00046B32 File Offset: 0x00044D32
		// (set) Token: 0x06000F07 RID: 3847 RVA: 0x00046B3A File Offset: 0x00044D3A
		public float ShootEnd { get; set; }

		// Token: 0x170002A0 RID: 672
		// (get) Token: 0x06000F08 RID: 3848 RVA: 0x00046B43 File Offset: 0x00044D43
		public List<Launcher> Launchers { get; } = new List<Launcher>();

		// Token: 0x170002A1 RID: 673
		// (get) Token: 0x06000F09 RID: 3849 RVA: 0x00046B4B File Offset: 0x00044D4B
		public List<Launcher> ChildLaunchers { get; } = new List<Launcher>();

		// Token: 0x170002A2 RID: 674
		// (get) Token: 0x06000F0A RID: 3850 RVA: 0x00046B53 File Offset: 0x00044D53
		// (set) Token: 0x06000F0B RID: 3851 RVA: 0x00046B5B File Offset: 0x00044D5B
		public bool LastWaveHitFlag { get; private set; }

		// Token: 0x170002A3 RID: 675
		// (get) Token: 0x06000F0C RID: 3852 RVA: 0x00046B64 File Offset: 0x00044D64
		// (set) Token: 0x06000F0D RID: 3853 RVA: 0x00046B6C File Offset: 0x00044D6C
		public bool NoPiece { get; private set; }

		// Token: 0x170002A4 RID: 676
		// (get) Token: 0x06000F0E RID: 3854 RVA: 0x00046B75 File Offset: 0x00044D75
		// (set) Token: 0x06000F0F RID: 3855 RVA: 0x00046B7D File Offset: 0x00044D7D
		public float StartTime { get; set; }

		// Token: 0x170002A5 RID: 677
		// (get) Token: 0x06000F10 RID: 3856 RVA: 0x00046B86 File Offset: 0x00044D86
		// (set) Token: 0x06000F11 RID: 3857 RVA: 0x00046B8E File Offset: 0x00044D8E
		public Vector2 ShootV2 { get; set; }

		// Token: 0x170002A6 RID: 678
		// (get) Token: 0x06000F12 RID: 3858 RVA: 0x00046B97 File Offset: 0x00044D97
		// (set) Token: 0x06000F13 RID: 3859 RVA: 0x00046B9F File Offset: 0x00044D9F
		public bool HasAnimation { get; private set; }

		// Token: 0x06000F14 RID: 3860 RVA: 0x00046BA8 File Offset: 0x00044DA8
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

		// Token: 0x06000F15 RID: 3861 RVA: 0x00046E48 File Offset: 0x00045048
		public void Aim(bool belongToPlayer, UnitView target, List<UnitView> targets)
		{
			this.BelongToPlayer = belongToPlayer;
			this.Target = target;
			this.Targets = targets;
		}

		// Token: 0x06000F16 RID: 3862 RVA: 0x00046E5F File Offset: 0x0004505F
		public void LastWaveHit()
		{
			this.LastWaveHitFlag = true;
		}

		// Token: 0x06000F17 RID: 3863 RVA: 0x00046E68 File Offset: 0x00045068
		public Piece GetPieceById(int id)
		{
			return Enumerable.FirstOrDefault<Piece>(this.Pieces, (Piece piece) => piece.Id == id);
		}
	}
}
