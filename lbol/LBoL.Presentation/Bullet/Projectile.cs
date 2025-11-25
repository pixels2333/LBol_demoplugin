using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LBoL.Core;
using LBoL.Presentation.Effect;
using UnityEngine;
namespace LBoL.Presentation.Bullet
{
	public abstract class Projectile : MonoBehaviour
	{
		protected string WidgetEffect { get; set; }
		protected string GrazeEffect { get; set; }
		protected string HitBlockEffect { get; set; }
		protected string HitShieldEffect { get; set; }
		protected string HitBodyEffect { get; set; }
		public bool Active { get; private set; }
		private bool FirstTickFlag { get; set; }
		protected int LifeTick { get; set; }
		protected float Timer
		{
			get
			{
				return (float)this.LifeTick * 0.016666668f;
			}
		}
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
		private float AccAngle { get; set; }
		private bool HasAcc { get; set; }
		private Vector2 RUnitVector
		{
			get
			{
				return Quaternion.Euler(0f, 0f, this._angle) * Vector2.right;
			}
		}
		private bool VelocityUpdating { get; set; }
		private bool Flip { get; set; }
		protected Gun Gun
		{
			get
			{
				return this.Launcher.Gun;
			}
		}
		protected Piece Piece
		{
			get
			{
				return this.Launcher.Piece;
			}
		}
		public Launcher Launcher { get; set; }
		private Vector2 TargetPosition { get; set; }
		private EffectWidget EffectWidget { get; set; }
		private List<BulletEvent> BulletEvents { get; set; }
		private bool EventExist { get; set; }
		private bool EventRemoving { get; set; }
		private Vector3 Position { get; set; }
		private Vector3 Velocity
		{
			get
			{
				return this.RUnitVector * this.Speed * 0.016666668f;
			}
		}
		protected int HitRemainAmount { get; set; }
		private void SetVelocity()
		{
			base.transform.rotation = Quaternion.Euler(0f, 0f, this.Flip ? (this.Angle + 180f) : this.Angle);
		}
		private void Move()
		{
			this.Position += this.Velocity;
		}
		private void LateUpdate()
		{
			base.transform.localPosition = this.Position;
		}
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
		protected virtual void Vanish()
		{
		}
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
		protected void ModifyEffect(EffectWidget effect)
		{
			EffectManager.ModifyEffect(effect, this.Launcher.Scale, this.Launcher.Color);
		}
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
		[CompilerGenerated]
		internal static float <Tick>g__Huali|115_1(int timer, ref Projectile.<>c__DisplayClass115_0 A_1)
		{
			float delta = A_1.be.Delta;
			float num = (float)timer * delta;
			return 0.5f * Mathf.Sin(num) * num;
		}
		private const float Tolerance = 0.01f;
		private float _acc;
		private float _angle;
		private float _speed;
		private Vector2 _accVector;
		private Vector2? _lastVector;
		private Vector2 _rUnitVector;
	}
}
