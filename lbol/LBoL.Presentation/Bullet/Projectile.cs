using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LBoL.Core;
using LBoL.Presentation.Effect;
using UnityEngine;

namespace LBoL.Presentation.Bullet
{
	// Token: 0x02000117 RID: 279
	public abstract class Projectile : MonoBehaviour
	{
		// Token: 0x170002EE RID: 750
		// (get) Token: 0x06000F9B RID: 3995 RVA: 0x00048D35 File Offset: 0x00046F35
		// (set) Token: 0x06000F9C RID: 3996 RVA: 0x00048D3D File Offset: 0x00046F3D
		protected string WidgetEffect { get; set; }

		// Token: 0x170002EF RID: 751
		// (get) Token: 0x06000F9D RID: 3997 RVA: 0x00048D46 File Offset: 0x00046F46
		// (set) Token: 0x06000F9E RID: 3998 RVA: 0x00048D4E File Offset: 0x00046F4E
		protected string GrazeEffect { get; set; }

		// Token: 0x170002F0 RID: 752
		// (get) Token: 0x06000F9F RID: 3999 RVA: 0x00048D57 File Offset: 0x00046F57
		// (set) Token: 0x06000FA0 RID: 4000 RVA: 0x00048D5F File Offset: 0x00046F5F
		protected string HitBlockEffect { get; set; }

		// Token: 0x170002F1 RID: 753
		// (get) Token: 0x06000FA1 RID: 4001 RVA: 0x00048D68 File Offset: 0x00046F68
		// (set) Token: 0x06000FA2 RID: 4002 RVA: 0x00048D70 File Offset: 0x00046F70
		protected string HitShieldEffect { get; set; }

		// Token: 0x170002F2 RID: 754
		// (get) Token: 0x06000FA3 RID: 4003 RVA: 0x00048D79 File Offset: 0x00046F79
		// (set) Token: 0x06000FA4 RID: 4004 RVA: 0x00048D81 File Offset: 0x00046F81
		protected string HitBodyEffect { get; set; }

		// Token: 0x170002F3 RID: 755
		// (get) Token: 0x06000FA5 RID: 4005 RVA: 0x00048D8A File Offset: 0x00046F8A
		// (set) Token: 0x06000FA6 RID: 4006 RVA: 0x00048D92 File Offset: 0x00046F92
		public bool Active { get; private set; }

		// Token: 0x170002F4 RID: 756
		// (get) Token: 0x06000FA7 RID: 4007 RVA: 0x00048D9B File Offset: 0x00046F9B
		// (set) Token: 0x06000FA8 RID: 4008 RVA: 0x00048DA3 File Offset: 0x00046FA3
		private bool FirstTickFlag { get; set; }

		// Token: 0x170002F5 RID: 757
		// (get) Token: 0x06000FA9 RID: 4009 RVA: 0x00048DAC File Offset: 0x00046FAC
		// (set) Token: 0x06000FAA RID: 4010 RVA: 0x00048DB4 File Offset: 0x00046FB4
		protected int LifeTick { get; set; }

		// Token: 0x170002F6 RID: 758
		// (get) Token: 0x06000FAB RID: 4011 RVA: 0x00048DBD File Offset: 0x00046FBD
		protected float Timer
		{
			get
			{
				return (float)this.LifeTick * 0.016666668f;
			}
		}

		// Token: 0x170002F7 RID: 759
		// (get) Token: 0x06000FAC RID: 4012 RVA: 0x00048DCC File Offset: 0x00046FCC
		// (set) Token: 0x06000FAD RID: 4013 RVA: 0x00048DD4 File Offset: 0x00046FD4
		private float Speed
		{
			get
			{
				return this._speed;
			}
			set
			{
				this._speed = value;
				this.Flip = value < 0f;
			}
		}

		// Token: 0x170002F8 RID: 760
		// (get) Token: 0x06000FAE RID: 4014 RVA: 0x00048DEB File Offset: 0x00046FEB
		// (set) Token: 0x06000FAF RID: 4015 RVA: 0x00048DF3 File Offset: 0x00046FF3
		public float Angle
		{
			get
			{
				return this._angle;
			}
			set
			{
				if (Math.Abs(this._angle - value) > 0.01f)
				{
					this._angle = value;
				}
			}
		}

		// Token: 0x170002F9 RID: 761
		// (get) Token: 0x06000FB0 RID: 4016 RVA: 0x00048E10 File Offset: 0x00047010
		// (set) Token: 0x06000FB1 RID: 4017 RVA: 0x00048E18 File Offset: 0x00047018
		private float Acc
		{
			get
			{
				return this._acc;
			}
			set
			{
				if (value != 0f)
				{
					this.HasAcc = true;
				}
				this._acc = value;
			}
		}

		// Token: 0x170002FA RID: 762
		// (get) Token: 0x06000FB2 RID: 4018 RVA: 0x00048E30 File Offset: 0x00047030
		// (set) Token: 0x06000FB3 RID: 4019 RVA: 0x00048E38 File Offset: 0x00047038
		private Vector2 AccVector
		{
			get
			{
				return this._accVector;
			}
			set
			{
				this._accVector = value;
			}
		}

		// Token: 0x170002FB RID: 763
		// (get) Token: 0x06000FB4 RID: 4020 RVA: 0x00048E41 File Offset: 0x00047041
		// (set) Token: 0x06000FB5 RID: 4021 RVA: 0x00048E4E File Offset: 0x0004704E
		private Vector2 LastVector
		{
			get
			{
				return this._lastVector.GetValueOrDefault();
			}
			set
			{
				this._lastVector = new Vector2?(value);
			}
		}

		// Token: 0x170002FC RID: 764
		// (get) Token: 0x06000FB6 RID: 4022 RVA: 0x00048E5C File Offset: 0x0004705C
		// (set) Token: 0x06000FB7 RID: 4023 RVA: 0x00048E64 File Offset: 0x00047064
		private float AccAngle { get; set; }

		// Token: 0x170002FD RID: 765
		// (get) Token: 0x06000FB8 RID: 4024 RVA: 0x00048E6D File Offset: 0x0004706D
		// (set) Token: 0x06000FB9 RID: 4025 RVA: 0x00048E75 File Offset: 0x00047075
		private bool HasAcc { get; set; }

		// Token: 0x170002FE RID: 766
		// (get) Token: 0x06000FBA RID: 4026 RVA: 0x00048E7E File Offset: 0x0004707E
		private Vector2 RUnitVector
		{
			get
			{
				return Quaternion.Euler(0f, 0f, this._angle) * Vector2.right;
			}
		}

		// Token: 0x170002FF RID: 767
		// (get) Token: 0x06000FBB RID: 4027 RVA: 0x00048EA9 File Offset: 0x000470A9
		// (set) Token: 0x06000FBC RID: 4028 RVA: 0x00048EB1 File Offset: 0x000470B1
		private bool VelocityUpdating { get; set; }

		// Token: 0x17000300 RID: 768
		// (get) Token: 0x06000FBD RID: 4029 RVA: 0x00048EBA File Offset: 0x000470BA
		// (set) Token: 0x06000FBE RID: 4030 RVA: 0x00048EC2 File Offset: 0x000470C2
		private bool Flip { get; set; }

		// Token: 0x17000301 RID: 769
		// (get) Token: 0x06000FBF RID: 4031 RVA: 0x00048ECB File Offset: 0x000470CB
		protected Gun Gun
		{
			get
			{
				return this.Launcher.Gun;
			}
		}

		// Token: 0x17000302 RID: 770
		// (get) Token: 0x06000FC0 RID: 4032 RVA: 0x00048ED8 File Offset: 0x000470D8
		protected Piece Piece
		{
			get
			{
				return this.Launcher.Piece;
			}
		}

		// Token: 0x17000303 RID: 771
		// (get) Token: 0x06000FC1 RID: 4033 RVA: 0x00048EE5 File Offset: 0x000470E5
		// (set) Token: 0x06000FC2 RID: 4034 RVA: 0x00048EED File Offset: 0x000470ED
		public Launcher Launcher { get; set; }

		// Token: 0x17000304 RID: 772
		// (get) Token: 0x06000FC3 RID: 4035 RVA: 0x00048EF6 File Offset: 0x000470F6
		// (set) Token: 0x06000FC4 RID: 4036 RVA: 0x00048EFE File Offset: 0x000470FE
		private Vector2 TargetPosition { get; set; }

		// Token: 0x17000305 RID: 773
		// (get) Token: 0x06000FC5 RID: 4037 RVA: 0x00048F07 File Offset: 0x00047107
		// (set) Token: 0x06000FC6 RID: 4038 RVA: 0x00048F0F File Offset: 0x0004710F
		private EffectWidget EffectWidget { get; set; }

		// Token: 0x17000306 RID: 774
		// (get) Token: 0x06000FC7 RID: 4039 RVA: 0x00048F18 File Offset: 0x00047118
		// (set) Token: 0x06000FC8 RID: 4040 RVA: 0x00048F20 File Offset: 0x00047120
		private List<BulletEvent> BulletEvents { get; set; }

		// Token: 0x17000307 RID: 775
		// (get) Token: 0x06000FC9 RID: 4041 RVA: 0x00048F29 File Offset: 0x00047129
		// (set) Token: 0x06000FCA RID: 4042 RVA: 0x00048F31 File Offset: 0x00047131
		private bool EventExist { get; set; }

		// Token: 0x17000308 RID: 776
		// (get) Token: 0x06000FCB RID: 4043 RVA: 0x00048F3A File Offset: 0x0004713A
		// (set) Token: 0x06000FCC RID: 4044 RVA: 0x00048F42 File Offset: 0x00047142
		private bool EventRemoving { get; set; }

		// Token: 0x17000309 RID: 777
		// (get) Token: 0x06000FCD RID: 4045 RVA: 0x00048F4B File Offset: 0x0004714B
		// (set) Token: 0x06000FCE RID: 4046 RVA: 0x00048F53 File Offset: 0x00047153
		private Vector3 Position { get; set; }

		// Token: 0x1700030A RID: 778
		// (get) Token: 0x06000FCF RID: 4047 RVA: 0x00048F5C File Offset: 0x0004715C
		private Vector3 Velocity
		{
			get
			{
				return this.RUnitVector * this.Speed * 0.016666668f;
			}
		}

		// Token: 0x1700030B RID: 779
		// (get) Token: 0x06000FD0 RID: 4048 RVA: 0x00048F7E File Offset: 0x0004717E
		// (set) Token: 0x06000FD1 RID: 4049 RVA: 0x00048F86 File Offset: 0x00047186
		protected int HitRemainAmount { get; set; }

		// Token: 0x06000FD2 RID: 4050 RVA: 0x00048F8F File Offset: 0x0004718F
		private void SetVelocity()
		{
			base.transform.rotation = Quaternion.Euler(0f, 0f, this.Flip ? (this.Angle + 180f) : this.Angle);
		}

		// Token: 0x06000FD3 RID: 4051 RVA: 0x00048FC7 File Offset: 0x000471C7
		private void Move()
		{
			this.Position += this.Velocity;
		}

		// Token: 0x06000FD4 RID: 4052 RVA: 0x00048FE0 File Offset: 0x000471E0
		private void LateUpdate()
		{
			base.transform.localPosition = this.Position;
		}

		// Token: 0x06000FD5 RID: 4053 RVA: 0x00048FF4 File Offset: 0x000471F4
		public virtual void Tick()
		{
			if (!this.Active)
			{
				Debug.Log("Not active.");
				return;
			}
			this.Move();
			if (this.LifeTick > this.Launcher.LifeTime)
			{
				this.Vanish();
				return;
			}
			if (this.FirstTickFlag)
			{
				this.FirstTickFlag = false;
				this.FirstTick();
				return;
			}
			this.LifeTick++;
			float num = this.Speed;
			float num2 = this.Angle;
			float num3 = this.Acc;
			float num4 = this.AccAngle;
			bool flag = false;
			if (this.EventRemoving)
			{
				this.EventRemoving = false;
				this.BulletEvents.RemoveAll((BulletEvent be) => be.EvTimer >= be.EventDuration);
				if (this.BulletEvents.Count == 0)
				{
					this.EventExist = false;
				}
			}
			if (this.EventExist)
			{
				foreach (BulletEvent bulletEvent in this.BulletEvents)
				{
					Projectile.<>c__DisplayClass115_0 CS$<>8__locals1;
					CS$<>8__locals1.be = bulletEvent;
					if (this.LifeTick > CS$<>8__locals1.be.EventStart)
					{
						int num5;
						if (!CS$<>8__locals1.be.EvSetup)
						{
							num5 = CS$<>8__locals1.be.EventType[0];
							switch (num5)
							{
							case 1:
								CS$<>8__locals1.be.Delta = Projectile.EventDataInitiProcess(this.Speed, CS$<>8__locals1.be.EventNumber, CS$<>8__locals1.be.EventType, this.Launcher.GroupIndex, this.Launcher.WayIndex, CS$<>8__locals1.be.EventDuration);
								break;
							case 2:
								CS$<>8__locals1.be.Delta = Projectile.EventDataInitiProcess(this.Angle, CS$<>8__locals1.be.EventNumber, CS$<>8__locals1.be.EventType, this.Launcher.GroupIndex, this.Launcher.WayIndex, CS$<>8__locals1.be.EventDuration);
								break;
							case 3:
								CS$<>8__locals1.be.Delta = Projectile.EventDataInitiProcess(this.Acc, CS$<>8__locals1.be.EventNumber, CS$<>8__locals1.be.EventType, this.Launcher.GroupIndex, this.Launcher.WayIndex, CS$<>8__locals1.be.EventDuration);
								break;
							case 4:
								CS$<>8__locals1.be.Delta = Projectile.EventDataInitiProcess(this.AccAngle, CS$<>8__locals1.be.EventNumber, CS$<>8__locals1.be.EventType, this.Launcher.GroupIndex, this.Launcher.WayIndex, CS$<>8__locals1.be.EventDuration);
								break;
							case 5:
								CS$<>8__locals1.be.Delta = Projectile.ArrayCalcu(CS$<>8__locals1.be.EventNumber, this.Launcher.GroupIndex, this.Launcher.WayIndex);
								break;
							case 6:
							case 7:
								goto IL_06FE;
							case 8:
								break;
							case 9:
								CS$<>8__locals1.be.Delta = Projectile.ArrayCalcu(CS$<>8__locals1.be.EventNumber, this.Launcher.GroupIndex, this.Launcher.WayIndex);
								break;
							case 10:
								CS$<>8__locals1.be.Delta = Projectile.ArrayCalcu(CS$<>8__locals1.be.EventNumber, this.Launcher.GroupIndex, this.Launcher.WayIndex);
								break;
							case 11:
								CS$<>8__locals1.be.Delta = Projectile.EventDataInitiProcess(this.Position.x, CS$<>8__locals1.be.EventNumber, CS$<>8__locals1.be.EventType, this.Launcher.GroupIndex, this.Launcher.WayIndex, CS$<>8__locals1.be.EventDuration);
								break;
							case 12:
								CS$<>8__locals1.be.Delta = Projectile.EventDataInitiProcess(this.Position.y, CS$<>8__locals1.be.EventNumber, CS$<>8__locals1.be.EventType, this.Launcher.GroupIndex, this.Launcher.WayIndex, CS$<>8__locals1.be.EventDuration);
								break;
							case 13:
								CS$<>8__locals1.be.Delta = Projectile.EventDataInitiProcess(base.transform.localScale.x, CS$<>8__locals1.be.EventNumber, CS$<>8__locals1.be.EventType, this.Launcher.GroupIndex, this.Launcher.WayIndex, CS$<>8__locals1.be.EventDuration);
								break;
							case 14:
								CS$<>8__locals1.be.Delta = Projectile.EventDataInitiProcess(base.transform.localScale.y, CS$<>8__locals1.be.EventNumber, CS$<>8__locals1.be.EventType, this.Launcher.GroupIndex, this.Launcher.WayIndex, CS$<>8__locals1.be.EventDuration);
								break;
							case 15:
								CS$<>8__locals1.be.Delta = Projectile.EventDataInitiProcess(base.transform.localScale.x, CS$<>8__locals1.be.EventNumber, CS$<>8__locals1.be.EventType, this.Launcher.GroupIndex, this.Launcher.WayIndex, CS$<>8__locals1.be.EventDuration);
								break;
							case 16:
								CS$<>8__locals1.be.Delta = Projectile.EventDataInitiProcess(this.Position.x, CS$<>8__locals1.be.EventNumber, CS$<>8__locals1.be.EventType, this.Launcher.GroupIndex, this.Launcher.WayIndex, CS$<>8__locals1.be.EventDuration);
								break;
							case 17:
								CS$<>8__locals1.be.Delta = Projectile.EventDataInitiProcess(this.Position.y, CS$<>8__locals1.be.EventNumber, CS$<>8__locals1.be.EventType, this.Launcher.GroupIndex, this.Launcher.WayIndex, CS$<>8__locals1.be.EventDuration);
								break;
							case 18:
								CS$<>8__locals1.be.Delta = Projectile.EventDataInitiProcess(this.Position.x, CS$<>8__locals1.be.EventNumber, CS$<>8__locals1.be.EventType, this.Launcher.GroupIndex, this.Launcher.WayIndex, CS$<>8__locals1.be.EventDuration);
								break;
							case 19:
								CS$<>8__locals1.be.Delta = Projectile.EventDataInitiProcess(this.Position.y, CS$<>8__locals1.be.EventNumber, CS$<>8__locals1.be.EventType, this.Launcher.GroupIndex, this.Launcher.WayIndex, CS$<>8__locals1.be.EventDuration);
								break;
							default:
								if (num5 != 99)
								{
									goto IL_06FE;
								}
								CS$<>8__locals1.be.Delta = Projectile.ArrayCalcu(CS$<>8__locals1.be.EventNumber, this.Launcher.GroupIndex, this.Launcher.WayIndex);
								break;
							}
							CS$<>8__locals1.be.EvSetup = true;
							goto IL_0732;
							IL_06FE:
							throw new InvalidOperationException("unknown bullet event type" + CS$<>8__locals1.be.EventType[0].ToString());
						}
						IL_0732:
						num5 = CS$<>8__locals1.be.EventType[0];
						switch (num5)
						{
						case 1:
							num += CS$<>8__locals1.be.Delta;
							this.VelocityUpdating = true;
							break;
						case 2:
							num2 += CS$<>8__locals1.be.Delta;
							this.VelocityUpdating = true;
							break;
						case 3:
							num3 += CS$<>8__locals1.be.Delta;
							this.VelocityUpdating = true;
							break;
						case 4:
							num4 += CS$<>8__locals1.be.Delta;
							this.VelocityUpdating = true;
							break;
						case 5:
							if (this.HitRemainAmount > 0 || this.Piece.HitAmount == 0)
							{
								this.VelocityUpdating = true;
								Vector2 vector = this.TargetPosition - base.transform.position;
								vector.Normalize();
								if (CS$<>8__locals1.be.Delta == 0f)
								{
									num2 = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
								}
								else
								{
									num2 += -Vector3.Cross(vector, base.transform.right).z * CS$<>8__locals1.be.Delta;
								}
							}
							break;
						case 6:
						case 7:
							goto IL_11A2;
						case 8:
							Debug.Log("todo BulletEvent Type 8");
							break;
						case 9:
							if (CS$<>8__locals1.be.Delta >= 0f)
							{
								if ((double)base.transform.position.y > 4.5 || (double)base.transform.position.y < -4.5)
								{
									if (CS$<>8__locals1.be.Delta == 0f && CS$<>8__locals1.be.EventType.Length == 2 && CS$<>8__locals1.be.EventType[1] == 1)
									{
										Vector2 vector2 = this.TargetPosition - base.transform.position;
										num2 = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f;
									}
									else
									{
										num2 = -num2;
									}
									this.VelocityUpdating = true;
									BulletEvent be9 = CS$<>8__locals1.be;
									float num6 = be9.Delta;
									be9.Delta = num6 - 1f;
									break;
								}
								if (base.transform.position.x > 8f && !this.Gun.BelongToPlayer)
								{
									if (CS$<>8__locals1.be.Delta == 0f && CS$<>8__locals1.be.EventType.Length == 2 && CS$<>8__locals1.be.EventType[1] == 1)
									{
										Vector2 vector3 = this.TargetPosition - base.transform.position;
										num2 = Mathf.Atan2(vector3.y, vector3.x) * 57.29578f;
									}
									else
									{
										num2 = -num2 + 180f;
									}
									this.VelocityUpdating = true;
									BulletEvent be2 = CS$<>8__locals1.be;
									float num6 = be2.Delta;
									be2.Delta = num6 - 1f;
									break;
								}
								if (base.transform.position.x < -8f && this.Gun.BelongToPlayer)
								{
									if (CS$<>8__locals1.be.Delta == 0f && CS$<>8__locals1.be.EventType.Length == 2 && CS$<>8__locals1.be.EventType[1] == 1)
									{
										Vector2 vector4 = this.TargetPosition - base.transform.position;
										num2 = Mathf.Atan2(vector4.y, vector4.x) * 57.29578f;
									}
									else
									{
										num2 = -num2 + 180f;
									}
									this.VelocityUpdating = true;
									BulletEvent be3 = CS$<>8__locals1.be;
									float num6 = be3.Delta;
									be3.Delta = num6 - 1f;
									break;
								}
							}
							if (CS$<>8__locals1.be.Delta < 0f)
							{
								CS$<>8__locals1.be.EvTimer = CS$<>8__locals1.be.EventDuration + 1;
							}
							break;
						case 10:
							if (CS$<>8__locals1.be.Delta >= 0f)
							{
								if ((double)base.transform.position.y > 4.5)
								{
									if (CS$<>8__locals1.be.Delta == 0f && CS$<>8__locals1.be.EventType.Length == 2 && CS$<>8__locals1.be.EventType[1] == 1)
									{
										Vector2 vector5 = this.TargetPosition - base.transform.position;
										num2 = Mathf.Atan2(vector5.y, vector5.x) * 57.29578f;
									}
									else
									{
										num2 = -90f;
									}
									this.VelocityUpdating = true;
									BulletEvent be4 = CS$<>8__locals1.be;
									float num6 = be4.Delta;
									be4.Delta = num6 - 1f;
								}
								if ((double)base.transform.position.y < -4.5)
								{
									if (CS$<>8__locals1.be.Delta == 0f && CS$<>8__locals1.be.EventType.Length == 2 && CS$<>8__locals1.be.EventType[1] == 1)
									{
										Vector2 vector6 = this.TargetPosition - base.transform.position;
										num2 = Mathf.Atan2(vector6.y, vector6.x) * 57.29578f;
									}
									else
									{
										num2 = 90f;
									}
									this.VelocityUpdating = true;
									BulletEvent be5 = CS$<>8__locals1.be;
									float num6 = be5.Delta;
									be5.Delta = num6 - 1f;
								}
								if (base.transform.position.x > 8f && !this.Gun.BelongToPlayer)
								{
									if (CS$<>8__locals1.be.Delta == 0f && CS$<>8__locals1.be.EventType.Length == 2 && CS$<>8__locals1.be.EventType[1] == 1)
									{
										Vector2 vector7 = this.TargetPosition - base.transform.position;
										num2 = Mathf.Atan2(vector7.y, vector7.x) * 57.29578f;
									}
									else
									{
										num2 = 180f;
									}
									this.VelocityUpdating = true;
									BulletEvent be6 = CS$<>8__locals1.be;
									float num6 = be6.Delta;
									be6.Delta = num6 - 1f;
								}
								if (base.transform.position.x < -8f && this.Gun.BelongToPlayer)
								{
									if (CS$<>8__locals1.be.Delta == 0f && CS$<>8__locals1.be.EventType.Length == 2 && CS$<>8__locals1.be.EventType[1] == 1)
									{
										Vector2 vector8 = this.TargetPosition - base.transform.position;
										num2 = Mathf.Atan2(vector8.y, vector8.x) * 57.29578f;
									}
									else
									{
										num2 = 0f;
									}
									this.VelocityUpdating = true;
									BulletEvent be7 = CS$<>8__locals1.be;
									float num6 = be7.Delta;
									be7.Delta = num6 - 1f;
								}
							}
							else if (CS$<>8__locals1.be.Delta < 0f)
							{
								CS$<>8__locals1.be.EvTimer = CS$<>8__locals1.be.EventDuration + 1;
							}
							break;
						case 11:
							this.Position += new Vector3(CS$<>8__locals1.be.Delta, 0f, 0f);
							this.VelocityUpdating = true;
							break;
						case 12:
							this.Position += new Vector3(0f, CS$<>8__locals1.be.Delta, 0f);
							this.VelocityUpdating = true;
							break;
						case 13:
							base.transform.localScale += new Vector3(CS$<>8__locals1.be.Delta, CS$<>8__locals1.be.Delta, 0f);
							this.VelocityUpdating = true;
							break;
						case 14:
							base.transform.localScale += new Vector3(0f, CS$<>8__locals1.be.Delta, 0f);
							this.VelocityUpdating = true;
							break;
						case 15:
							base.transform.localScale += new Vector3(CS$<>8__locals1.be.Delta, 0f, 0f);
							this.VelocityUpdating = true;
							break;
						case 16:
							this.Position += new Vector3(Mathf.Cos(num2 * 0.017453292f) * CS$<>8__locals1.be.Delta, Mathf.Sin(num2 * 0.017453292f) * CS$<>8__locals1.be.Delta, 0f);
							this.VelocityUpdating = true;
							break;
						case 17:
							this.Position += new Vector3(Mathf.Sin(num2 * 0.017453292f) * CS$<>8__locals1.be.Delta, Mathf.Cos(num2 * 0.017453292f) * CS$<>8__locals1.be.Delta, 0f);
							this.VelocityUpdating = true;
							break;
						case 18:
							this.Position += new Vector3(Mathf.Cos(num4 * 0.017453292f) * CS$<>8__locals1.be.Delta, Mathf.Sin(num4 * 0.017453292f) * CS$<>8__locals1.be.Delta, 0f);
							this.VelocityUpdating = true;
							flag = true;
							break;
						case 19:
							this.Position += new Vector3(Mathf.Sin(num4 * 0.017453292f) * CS$<>8__locals1.be.Delta, Mathf.Cos(num4 * 0.017453292f) * CS$<>8__locals1.be.Delta, 0f);
							this.VelocityUpdating = true;
							flag = true;
							break;
						default:
						{
							if (num5 != 99)
							{
								goto IL_11A2;
							}
							float num7 = Projectile.<Tick>g__Huali|115_1(CS$<>8__locals1.be.EvTimer, ref CS$<>8__locals1) - Projectile.<Tick>g__Huali|115_1(CS$<>8__locals1.be.EvTimer - 1, ref CS$<>8__locals1);
							this.Position += new Vector3(0.07f, num7, 0f);
							this.VelocityUpdating = true;
							break;
						}
						}
						BulletEvent be8 = CS$<>8__locals1.be;
						num5 = be8.EvTimer;
						be8.EvTimer = num5 + 1;
						if (CS$<>8__locals1.be.EvTimer >= CS$<>8__locals1.be.EventDuration)
						{
							this.EventRemoving = true;
							continue;
						}
						continue;
						IL_11A2:
						throw new InvalidOperationException("unknown bullet event type" + CS$<>8__locals1.be.EventType[0].ToString());
					}
				}
			}
			if (Math.Abs(num3 - this.Acc) > 0.01f)
			{
				this.Acc = num3;
			}
			if (Math.Abs(num4 - this.AccAngle) > 0.01f)
			{
				this.AccAngle = num4;
			}
			if (Math.Abs(num4 - this.AccAngle) > 0.01f)
			{
				this.AccAngle = num4;
			}
			if (flag)
			{
				this.AccVector = num3 * new Vector2(Mathf.Cos(num4 * 0.017453292f), Mathf.Sin(num4 * 0.017453292f));
			}
			if (this.HasAcc)
			{
				Vector2 vector9 = ((this._lastVector == null || this.VelocityUpdating) ? (num * new Vector2(Mathf.Cos(num2 * 0.017453292f), Mathf.Sin(num2 * 0.017453292f))) : this.LastVector);
				Vector2 vector10 = this.AccVector + vector9;
				this.LastVector = vector10;
				num = vector10.magnitude;
				num2 = Mathf.Atan2(vector10.y, vector10.x) * 57.29578f;
			}
			if (Math.Abs(num - this.Speed) > 0.01f)
			{
				this.Speed = num;
			}
			if (Math.Abs(num2 - this.Angle) > 0.01f)
			{
				this.Angle = num2;
			}
			if (this.HasAcc)
			{
				this.SetVelocity();
				this.VelocityUpdating = false;
				return;
			}
			if (this.VelocityUpdating)
			{
				this.SetVelocity();
				this.VelocityUpdating = false;
			}
		}

		// Token: 0x06000FD6 RID: 4054 RVA: 0x0004A3A0 File Offset: 0x000485A0
		public virtual void Spawn()
		{
			base.transform.localPosition = new Vector3(this.Launcher.V2.x, this.Launcher.V2.y);
			this.FirstTickFlag = true;
			this.BulletEvents = this.Launcher.BulletEvents;
			foreach (BulletEvent bulletEvent in this.BulletEvents)
			{
				bulletEvent.Delta = 0f;
				bulletEvent.EvTimer = 0;
			}
			this.HitRemainAmount = this.Launcher.Piece.HitAmount;
			this.EventExist = this.BulletEvents.Count > 0;
			base.gameObject.SetActive(true);
			this.Active = true;
		}

		// Token: 0x06000FD7 RID: 4055 RVA: 0x0004A484 File Offset: 0x00048684
		protected virtual void FirstTick()
		{
			this.EffectWidget = EffectManager.CreateEffect(this.WidgetEffect, base.transform, 0f, new float?(0f), true, true);
			this.ModifyEffect(this.EffectWidget);
			this.Speed = this.Launcher.Speed;
			this.Angle = this.Launcher.Angle;
			this.Acc = this.Launcher.SpeedAcc;
			this.AccAngle = this.Launcher.AccAngle;
			if (this.Gun.Target != null)
			{
				this.TargetPosition = this.Gun.Target.transform.position;
			}
			this.SetVelocity();
			this.Position = base.transform.localPosition;
			this._rUnitVector = Quaternion.Euler(0f, 0f, this._angle) * Vector2.right;
			this.AccVector = this.Acc * new Vector2(Mathf.Cos(this.AccAngle * 0.017453292f), Mathf.Sin(this.AccAngle * 0.017453292f));
		}

		// Token: 0x06000FD8 RID: 4056 RVA: 0x0004A5BA File Offset: 0x000487BA
		protected virtual void Vanish()
		{
		}

		// Token: 0x06000FD9 RID: 4057 RVA: 0x0004A5BC File Offset: 0x000487BC
		protected virtual void Die()
		{
			if (this.EffectWidget != null)
			{
				this.EffectWidget.transform.SetParent(Singleton<EffectManager>.Instance.transform);
				this.EffectWidget.DieOut();
			}
			this.LifeTick = 0;
			this.Speed = 0.01f;
			this.Angle = 0f;
			this.Acc = 0f;
			this.AccAngle = 0f;
			this.SetVelocity();
			List<BulletEvent> bulletEvents = this.BulletEvents;
			if (bulletEvents != null)
			{
				bulletEvents.Clear();
			}
			if (this.Launcher != null)
			{
				this.Launcher.Active = false;
				this.Launcher.DeathPositionV2 = base.transform.localPosition;
				this.Launcher.DeathAngle = this.Angle;
				if (this.Launcher.Piece.IsParentPieceType3)
				{
					GunManager.DeathRattle(this.Launcher);
				}
				this.Launcher = null;
			}
			this.Active = false;
		}

		// Token: 0x06000FDA RID: 4058 RVA: 0x0004A6B1 File Offset: 0x000488B1
		protected void ModifyEffect(EffectWidget effect)
		{
			EffectManager.ModifyEffect(effect, this.Launcher.Scale, this.Launcher.Color);
		}

		// Token: 0x06000FDB RID: 4059 RVA: 0x0004A6D0 File Offset: 0x000488D0
		private static float EventDataInitiProcess(float currentValue, float[][] eventNumber, int[] eventType, int groupIndex, int wayIndex, int duration)
		{
			if (eventType.Length == 1)
			{
				return Projectile.ArrayCalcu(eventNumber, groupIndex, wayIndex) / (float)duration;
			}
			float num;
			switch (eventType[1])
			{
			case 0:
				num = Projectile.ArrayCalcu(eventNumber, groupIndex, wayIndex) / (float)duration;
				break;
			case 1:
				num = (Projectile.ArrayCalcu(eventNumber, groupIndex, wayIndex) - currentValue) / (float)duration;
				break;
			case 2:
				num = (Projectile.ArrayCalcu(eventNumber, groupIndex, wayIndex) - 1f) * currentValue / (float)duration;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return num;
		}

		// Token: 0x06000FDC RID: 4060 RVA: 0x0004A74C File Offset: 0x0004894C
		private static float ArrayCalcu(float[][] ps, int groupID, int wayID)
		{
			float num;
			switch (ps.Length)
			{
			case 0:
				num = 0f;
				break;
			case 1:
				num = Projectile.RandomDataCalcu(ps[0]);
				break;
			case 2:
				num = Projectile.RandomDataCalcu(ps[0]) + Projectile.RandomDataCalcu(ps[1]) * (float)groupID;
				break;
			case 3:
				num = Projectile.RandomDataCalcu(ps[0]) + Projectile.RandomDataCalcu(ps[1]) * (float)groupID + Projectile.RandomDataCalcu(ps[2]) * (float)groupID * (float)groupID;
				break;
			case 4:
				num = Projectile.RandomDataCalcu(ps[0]) + Projectile.RandomDataCalcu(ps[1]) * (float)groupID + Projectile.RandomDataCalcu(ps[2]) * (float)groupID * (float)groupID + Projectile.RandomDataCalcu(ps[3]) * (float)wayID;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return num;
		}

		// Token: 0x06000FDD RID: 4061 RVA: 0x0004A808 File Offset: 0x00048A08
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

		// Token: 0x06000FDF RID: 4063 RVA: 0x0004A860 File Offset: 0x00048A60
		[CompilerGenerated]
		internal static float <Tick>g__Huali|115_1(int timer, ref Projectile.<>c__DisplayClass115_0 A_1)
		{
			float delta = A_1.be.Delta;
			float num = (float)timer * delta;
			return 0.5f * Mathf.Sin(num) * num;
		}

		// Token: 0x04000BA8 RID: 2984
		private const float Tolerance = 0.01f;

		// Token: 0x04000BAE RID: 2990
		private float _acc;

		// Token: 0x04000BAF RID: 2991
		private float _angle;

		// Token: 0x04000BB0 RID: 2992
		private float _speed;

		// Token: 0x04000BB4 RID: 2996
		private Vector2 _accVector;

		// Token: 0x04000BB5 RID: 2997
		private Vector2? _lastVector;

		// Token: 0x04000BB8 RID: 3000
		private Vector2 _rUnitVector;
	}
}
