using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using UnityEngine;

namespace LBoL.Presentation.Bullet
{
	// Token: 0x02000111 RID: 273
	public class GunManager : Singleton<GunManager>
	{
		// Token: 0x06000F19 RID: 3865 RVA: 0x00046EC4 File Offset: 0x000450C4
		private static GunManager GetInstance()
		{
			GunManager instance = Singleton<GunManager>.Instance;
			if (instance._initialized)
			{
				return instance;
			}
			throw new InvalidOperationException("GunManager is not initialized, call 'InitializeAsync()' is required");
		}

		// Token: 0x06000F1A RID: 3866 RVA: 0x00046EEC File Offset: 0x000450EC
		public static void ClearAll()
		{
			foreach (Gun gun in GunManager.ActiveGuns)
			{
				if (gun && gun.gameObject)
				{
					Object.Destroy(gun.gameObject);
				}
			}
			GunManager.ActiveGuns.Clear();
			foreach (Gun gun2 in GunManager.DeadGuns)
			{
				if (gun2 && gun2.gameObject)
				{
					Object.Destroy(gun2.gameObject);
				}
			}
			GunManager.DeadGuns.Clear();
			GunManager.LoadedLaunchers.Clear();
			GunManager.ShootingLaunchers.Clear();
			GunManager.AttackingLasers.Clear();
		}

		// Token: 0x06000F1B RID: 3867 RVA: 0x00046FE4 File Offset: 0x000451E4
		public void Tick()
		{
			if (GunManager.LoadedLaunchers.Count > 0)
			{
				foreach (Launcher launcher in GunManager.LoadedLaunchers)
				{
					Launcher launcher2 = launcher;
					int tick = launcher2.Tick;
					launcher2.Tick = tick - 1;
					if (launcher.Tick < 1)
					{
						GunManager.ShootingLaunchers.Add(launcher);
					}
				}
				if (GunManager.ShootingLaunchers.Count > 0)
				{
					foreach (Launcher launcher3 in GunManager.ShootingLaunchers)
					{
						GunManager.LoadedLaunchers.Remove(launcher3);
						this.LauncherShoot(launcher3);
					}
					GunManager.ShootingLaunchers.Clear();
				}
			}
			foreach (Laser laser in GunManager.AttackingLasers)
			{
				laser.TryToAttack();
			}
			GunManager.DeadGuns.Clear();
			foreach (Gun gun in GunManager.ActiveGuns)
			{
				if (!gun)
				{
					GunManager.DeadGuns.Add(gun);
				}
			}
			foreach (Gun gun2 in GunManager.DeadGuns)
			{
				GunManager.ActiveGuns.Remove(gun2);
			}
			foreach (Gun gun3 in GunManager.ActiveGuns)
			{
				foreach (Launcher launcher4 in gun3.Launchers)
				{
					Projectile projectile = launcher4.Projectile;
					if (projectile != null && projectile.Active)
					{
						projectile.Tick();
					}
				}
				if (gun3.ChildLaunchers.Count > 0)
				{
					foreach (Launcher launcher5 in gun3.ChildLaunchers)
					{
						Projectile projectile2 = launcher5.Projectile;
						if (projectile2 != null && projectile2.Active)
						{
							projectile2.Tick();
						}
					}
				}
			}
		}

		// Token: 0x06000F1C RID: 3868 RVA: 0x000472B4 File Offset: 0x000454B4
		public static Gun CreateGun(string gunName)
		{
			return GunManager.GetInstance().InternalCreateGun(gunName);
		}

		// Token: 0x06000F1D RID: 3869 RVA: 0x000472C4 File Offset: 0x000454C4
		private Gun InternalCreateGun(string gunName)
		{
			GunConfig gunConfig = GunConfig.FromName(gunName);
			if (gunConfig == null)
			{
				Debug.LogWarning("No such gun named: " + gunName + ". Use Simple1 gun instead.");
				gunConfig = GunConfig.FromName("Simple1");
			}
			Gun gun = Object.Instantiate<Gun>(this._gunTemplate, base.transform);
			gun.SetGun(gunConfig);
			gun.gameObject.name = gun.Name + "(Gun)";
			GunManager.ActiveGuns.Add(gun);
			return gun;
		}

		// Token: 0x06000F1E RID: 3870 RVA: 0x0004733B File Offset: 0x0004553B
		public static Gun GunShoot(Gun gun, bool instant = false)
		{
			return GunManager.GetInstance().InternalGunShoot(gun, instant);
		}

		// Token: 0x06000F1F RID: 3871 RVA: 0x0004734C File Offset: 0x0004554C
		private Gun InternalGunShoot(Gun gun, bool instant)
		{
			if (gun.NoPiece)
			{
				gun.ShootEnd = gun.ForceShowEndStartTime.GetValueOrDefault();
				return gun;
			}
			int num = Mathf.FloorToInt((instant ? 0f : gun.StartTime) * 60f);
			int num2 = 0;
			foreach (Piece piece in gun.Pieces)
			{
				if (piece.ShootType == 0 || piece.ShootType == 1)
				{
					GunManager.CalcLauncher(gun, num, piece, null);
				}
			}
			foreach (Launcher launcher in gun.Launchers)
			{
				if (launcher.Piece.IsParentPieceType2)
				{
					foreach (Piece piece2 in launcher.Piece.ChildPiecesType2)
					{
						GunManager.CalcLauncher(gun, num, piece2, launcher);
					}
				}
			}
			foreach (Launcher launcher2 in gun.Launchers)
			{
				GunManager.LoadedLaunchers.Add(launcher2);
				if (launcher2.Piece.ShootType == 0)
				{
					int num3 = ((launcher2.Piece.ShootEnd > 0) ? launcher2.Piece.ShootEnd : launcher2.Delay);
					if (num2 < num3)
					{
						num2 = num3;
					}
				}
			}
			if (gun.ChildLaunchers.Count > 0)
			{
				foreach (Launcher launcher3 in gun.ChildLaunchers)
				{
					GunManager.LoadedLaunchers.Add(launcher3);
				}
			}
			GameObject gameObject = gun.gameObject;
			gameObject.SetActive(true);
			Object.Destroy(gameObject, 10f);
			gun.ShootEnd = gun.ForceShowEndStartTime ?? ((float)num2 * 0.016666668f);
			return gun;
		}

		// Token: 0x06000F20 RID: 3872 RVA: 0x000475A8 File Offset: 0x000457A8
		public static void DeathRattle(Launcher launcher)
		{
			Gun gun = launcher.Gun;
			foreach (Piece piece in launcher.Piece.ChildPiecesType3)
			{
				GunManager.CalcLauncher(gun, 0, piece, launcher);
			}
			foreach (Launcher launcher2 in gun.ChildLaunchers)
			{
				if (launcher2.Active && !GunManager.LoadedLaunchers.Contains(launcher2))
				{
					GunManager.LoadedLaunchers.Add(launcher2);
				}
			}
		}

		// Token: 0x06000F21 RID: 3873 RVA: 0x00047668 File Offset: 0x00045868
		private static void CalcLauncher(Gun gun, int gunStartTick, Piece piece, Launcher parentLauncher = null)
		{
			float num;
			float num2;
			float num3;
			gun.transform.position.Deconstruct(out num, out num2, out num3);
			float num4 = num;
			float num5 = num2;
			Vector2 vector = new Vector2(num4, num5);
			gun.Target.transform.position.Deconstruct(out num3, out num2, out num);
			float num6 = num3;
			float num7 = num2;
			Vector2 vector2 = new Vector2(num6, num7) + gun.Target.BoxCollider.offset;
			float num8 = 0f;
			for (int i = 0; i < piece.Group; i++)
			{
				float num9 = GunManager.ArrayWayCalculate(piece.Way, i);
				int num10 = 0;
				while ((float)num10 < num9)
				{
					float num11 = GunManager.ArrayCalculate(piece.X, i, num10);
					float num12 = GunManager.ArrayCalculate(piece.Y, i, num10);
					Vector2 vector3 = new Vector2(num11, num12);
					Vector2 vector4 = vector + vector3;
					Vector2 vector5 = vector2 - vector4;
					Vector2 vector6;
					Vector2 vector7;
					switch (piece.RootType)
					{
					case 0:
						vector6 = vector5;
						vector7 = vector3;
						break;
					case 1:
						vector6 = -vector3;
						vector7 = vector2 + vector3 - vector;
						break;
					case 2:
						vector6 = vector5;
						vector7 = vector3 - vector;
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
					switch (piece.Aim)
					{
					case 0:
					case 3:
						num8 = Vector2.SignedAngle(Vector2.right, vector6);
						break;
					case 2:
					case 5:
						if (i == 0)
						{
							num8 = Vector2.SignedAngle(Vector2.right, vector6);
						}
						break;
					}
					float num13 = GunManager.ArrayCalculate(piece.Scale, i, num10);
					if (num13 == 0f)
					{
						num13 = 1f;
					}
					int num14 = GunManager.ArrayCalculate(piece.Life, i, num10);
					if (num14 == 0)
					{
						num14 = 300;
					}
					float num15 = GunManager.ArrayCalculate(piece.Range, i, num10);
					float num16 = GunManager.ArrayCalculate(piece.GAngle, i, num10) - num15 / 2f + num15 / num9 / 2f;
					float num17 = GunManager.ArrayCalculate(piece.Radius, i, num10);
					float num18 = GunManager.ArrayCalculate(piece.RadiusA, i, num10);
					float num19 = GunManager.ArrayCalculate(piece.StartSpeed, i, num10);
					float num20 = GunManager.ArrayCalculate(piece.StartAcc, i, num10) / 60f;
					float num21 = GunManager.ArrayCalculate(piece.StartAccAngle, i, num10);
					int num22 = gunStartTick + piece.StartTime + i * piece.GInterval;
					float num23 = num8 + num16 + num15 / num9 * (float)num10;
					float num24 = (float)Math.Cos((double)num23 * 3.141592653589793 / 180.0) * num17;
					float num25 = (float)Math.Sin((double)num23 * 3.141592653589793 / 180.0) * num17;
					Vector2 vector8 = vector7 + new Vector2(num24, num25);
					float num26 = Mathf.Repeat(num23 + num18, 360f);
					int num27 = GunManager.CalculateColor(piece.Color, i, num10);
					if (piece.FollowPiece != 0)
					{
						int index = piece.FollowPiece;
						if (index >= piece.Id)
						{
							throw new InvalidOperationException("只能Follow比自己Id小的piece, 错误piece Id: " + piece.Id.ToString());
						}
						if (PieceConfig.FromId(piece.FollowPiece) == null)
						{
							throw new InvalidOperationException("Follow的piece Id不存在, 错误piece Id: " + piece.Id.ToString());
						}
						Piece piece2 = gun.Pieces.Find((Piece p) => p.Id == index);
						if (piece2 != null)
						{
							foreach (Launcher launcher in gun.Launchers)
							{
								if (launcher.Piece == piece2 && ((launcher.GroupIndex == i) & (launcher.WayIndex == num10)))
								{
									vector8 = launcher.V2;
									num26 = launcher.Angle;
								}
							}
						}
					}
					Launcher launcher2 = new Launcher(gun, piece, parentLauncher, vector8, num22, num19, num26, num20, num21, i, num10, num27, piece.Aim, num13, num14);
					if (parentLauncher == null)
					{
						gun.Launchers.Add(launcher2);
					}
					else
					{
						gun.ChildLaunchers.Add(launcher2);
					}
					num10++;
				}
			}
		}

		// Token: 0x06000F22 RID: 3874 RVA: 0x00047AC0 File Offset: 0x00045CC0
		private void LauncherShoot(Launcher launcher)
		{
			Launcher parentLauncher = launcher.ParentLauncher;
			if (parentLauncher != null)
			{
				if (parentLauncher.Active)
				{
					if (parentLauncher.Piece.IsParentPieceType2 && launcher.Piece.ShootType == 2)
					{
						Projectile projectile = parentLauncher.Projectile;
						Vector2 vector = projectile.transform.localPosition;
						launcher.V2 += vector;
						int num = launcher.Aim;
						if (num == 3 || num == 4 || num == 5)
						{
							launcher.Angle += projectile.Angle;
						}
						if (launcher.Piece.AddParentAngle)
						{
							launcher.Angle += projectile.Angle;
						}
						this.SpawnProjectile(launcher);
						return;
					}
				}
				else if (parentLauncher.Piece.IsParentPieceType3 && launcher.Piece.ShootType == 3)
				{
					Vector2 deathPositionV = parentLauncher.DeathPositionV2;
					launcher.V2 += deathPositionV;
					int num = launcher.Aim;
					if (num == 3 || num == 4 || num == 5)
					{
						launcher.Angle += parentLauncher.Projectile.Angle;
					}
					if (launcher.Piece.AddParentAngle)
					{
						launcher.Angle += parentLauncher.DeathAngle;
					}
					this.SpawnProjectile(launcher);
					return;
				}
			}
			else
			{
				this.SpawnProjectile(launcher);
			}
		}

		// Token: 0x06000F23 RID: 3875 RVA: 0x00047C14 File Offset: 0x00045E14
		private void SpawnProjectile(Launcher launcher)
		{
			if (launcher.Piece.Type)
			{
				LaserConfig laserConfig = LaserConfig.FromName(launcher.Piece.ProjectileName);
				if (laserConfig != null)
				{
					Laser laser = Object.Instantiate<Laser>(this._laserTemplate, launcher.Gun.transform);
					laser.Set(laserConfig);
					launcher.Projectile = laser;
					laser.Launcher = launcher;
					laser.Spawn();
					return;
				}
				Debug.LogError("No such Laser '" + launcher.Piece.ProjectileName + "'");
				return;
			}
			else
			{
				BulletConfig bulletConfig = BulletConfig.FromName(launcher.Piece.ProjectileName);
				if (bulletConfig != null)
				{
					Bullet bullet = Object.Instantiate<Bullet>(this._bulletTemplate, launcher.Gun.transform);
					bullet.Set(bulletConfig);
					launcher.Projectile = bullet;
					bullet.Launcher = launcher;
					bullet.Spawn();
					return;
				}
				Debug.LogError("No such Bullet named: " + launcher.Piece.ProjectileName);
				return;
			}
		}

		// Token: 0x06000F24 RID: 3876 RVA: 0x00047CF8 File Offset: 0x00045EF8
		private static int CalculateColor(int[][] color, int groupID, int wayID)
		{
			switch (color.Length)
			{
			case 0:
				return 0;
			case 1:
				return color[0][0];
			case 2:
				switch (color[0][0])
				{
				case 1:
					return color[1][groupID % color[1].Length];
				case 2:
					return color[1][wayID % color[1].Length];
				case 3:
				{
					int num = Random.Range(0, color[1].Length);
					return color[1][num];
				}
				default:
					throw new ArgumentOutOfRangeException();
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x06000F25 RID: 3877 RVA: 0x00047D78 File Offset: 0x00045F78
		private static float ArrayCalculate(float[][] ps, int groupID, int wayID)
		{
			float num;
			switch (ps.Length)
			{
			case 0:
				num = 0f;
				break;
			case 1:
				num = GunManager.RandomDataCalcu(ps[0]);
				break;
			case 2:
				num = GunManager.RandomDataCalcu(ps[0]) + GunManager.RandomDataCalcu(ps[1]) * (float)groupID;
				break;
			case 3:
				num = GunManager.RandomDataCalcu(ps[0]) + GunManager.RandomDataCalcu(ps[1]) * (float)groupID + GunManager.RandomDataCalcu(ps[2]) * (float)groupID * (float)groupID;
				break;
			case 4:
				num = GunManager.RandomDataCalcu(ps[0]) + GunManager.RandomDataCalcu(ps[1]) * (float)groupID + GunManager.RandomDataCalcu(ps[2]) * (float)groupID * (float)groupID + GunManager.RandomDataCalcu(ps[3]) * (float)wayID;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return num;
		}

		// Token: 0x06000F26 RID: 3878 RVA: 0x00047E34 File Offset: 0x00046034
		private static int ArrayCalculate(int[][] ps, int groupID, int wayID)
		{
			int num;
			switch (ps.Length)
			{
			case 0:
				num = 0;
				break;
			case 1:
				num = GunManager.RandomDataCalcu(ps[0]);
				break;
			case 2:
				num = GunManager.RandomDataCalcu(ps[0]) + GunManager.RandomDataCalcu(ps[1]) * groupID;
				break;
			case 3:
				num = GunManager.RandomDataCalcu(ps[0]) + GunManager.RandomDataCalcu(ps[1]) * groupID + GunManager.RandomDataCalcu(ps[2]) * groupID * groupID;
				break;
			case 4:
				num = GunManager.RandomDataCalcu(ps[0]) + GunManager.RandomDataCalcu(ps[1]) * groupID + GunManager.RandomDataCalcu(ps[2]) * groupID * groupID + GunManager.RandomDataCalcu(ps[3]) * wayID;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return num;
		}

		// Token: 0x06000F27 RID: 3879 RVA: 0x00047EE0 File Offset: 0x000460E0
		private static float ArrayWayCalculate(int[][] ps, int groupID)
		{
			int num;
			switch (ps.Length)
			{
			case 0:
				num = 1;
				break;
			case 1:
				num = GunManager.RandomDataCalcu(ps[0]);
				break;
			case 2:
				num = GunManager.RandomDataCalcu(ps[0]) + GunManager.RandomDataCalcu(ps[1]) * groupID;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return (float)num;
		}

		// Token: 0x06000F28 RID: 3880 RVA: 0x00047F34 File Offset: 0x00046134
		private static float RandomDataCalcu(float[] ps)
		{
			float num;
			switch (ps.Length)
			{
			case 0:
				num = 0f;
				break;
			case 1:
				num = ps[0];
				break;
			case 2:
				num = ps[0] + Random.Range(-ps[1], ps[1]);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return num;
		}

		// Token: 0x06000F29 RID: 3881 RVA: 0x00047F84 File Offset: 0x00046184
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
				num = ps[0] + Random.Range(-ps[1], ps[1]);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return num;
		}

		// Token: 0x06000F2A RID: 3882 RVA: 0x00047FCD File Offset: 0x000461CD
		public static UniTask InitializeAsync()
		{
			return Singleton<GunManager>.Instance.InternalInitializeAsync();
		}

		// Token: 0x06000F2B RID: 3883 RVA: 0x00047FDC File Offset: 0x000461DC
		private async UniTask InternalInitializeAsync()
		{
			if (!this._initialized)
			{
				Object @object = await Resources.LoadAsync<Gun>("Template/GunTemplate");
				this._gunTemplate = (Gun)@object;
				if (!this._gunTemplate)
				{
					throw new NullReferenceException("Gun Template Lost!");
				}
				@object = await Resources.LoadAsync<Bullet>("Template/BulletTemplate");
				this._bulletTemplate = (Bullet)@object;
				if (!this._bulletTemplate)
				{
					throw new NullReferenceException("Bullet Template Lost!");
				}
				if (!this._bulletTemplate.CompareTag("Bullet"))
				{
					throw new InvalidDataException("Bullet Template tag error.");
				}
				@object = await Resources.LoadAsync<Laser>("Template/LaserTemplate");
				this._laserTemplate = (Laser)@object;
				if (!this._laserTemplate)
				{
					throw new NullReferenceException("Laser Template Lost!");
				}
				this._initialized = true;
			}
		}

		// Token: 0x04000B4B RID: 2891
		private bool _initialized;

		// Token: 0x04000B4C RID: 2892
		private Gun _gunTemplate;

		// Token: 0x04000B4D RID: 2893
		private Bullet _bulletTemplate;

		// Token: 0x04000B4E RID: 2894
		private Laser _laserTemplate;

		// Token: 0x04000B4F RID: 2895
		private static readonly List<Launcher> LoadedLaunchers = new List<Launcher>();

		// Token: 0x04000B50 RID: 2896
		private static readonly List<Launcher> ShootingLaunchers = new List<Launcher>();

		// Token: 0x04000B51 RID: 2897
		private static readonly List<Gun> ActiveGuns = new List<Gun>();

		// Token: 0x04000B52 RID: 2898
		private static readonly List<Gun> DeadGuns = new List<Gun>();

		// Token: 0x04000B53 RID: 2899
		public static readonly List<Laser> AttackingLasers = new List<Laser>();
	}
}
