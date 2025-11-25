using System;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	public class ReflectDamage
	{
		public Unit Target { get; }
		public int Damage { get; }
		public ReflectDamage(Unit target, int damage)
		{
			this.Target = target;
			this.Damage = damage;
		}
	}
}
