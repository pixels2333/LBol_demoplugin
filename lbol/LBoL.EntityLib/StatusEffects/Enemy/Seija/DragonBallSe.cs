using System;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.Exhibits.Seija;
namespace LBoL.EntityLib.StatusEffects.Enemy.Seija
{
	public sealed class DragonBallSe : SeijaSe
	{
		protected override Type ExhibitType
		{
			get
			{
				return typeof(DragonBall);
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			this.React(PerformAction.Sfx("GuirenItem", 0f));
			this.React(PerformAction.EffectMessage(unit, "SeijaExhibitManager", "AddExhibit", this));
			base.HandleOwnerEvent<DieEventArgs>(base.Owner.Dying, new GameEventHandler<DieEventArgs>(this.OnOwnerDying));
			base.Highlight = true;
		}
		private void OnOwnerDying(DieEventArgs args)
		{
			if (base.Owner is Seija && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.SeijaSpecial);
			}
		}
	}
}
