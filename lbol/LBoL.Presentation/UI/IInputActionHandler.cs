using System;
using LBoL.Base;
namespace LBoL.Presentation.UI
{
	public interface IInputActionHandler
	{
		void OnConfirm()
		{
		}
		void OnCancel()
		{
		}
		void OnRightClickCancel()
		{
			if (GameMaster.RightClickCancel)
			{
				this.OnCancel();
			}
		}
		void OnNavigate(NavigateDirection dir)
		{
		}
		void BeginSkipDialog()
		{
		}
		void EndSkipDialog()
		{
		}
		void BeginShowEnemyMoveOrder()
		{
		}
		void EndShowEnemyMoveOrder()
		{
		}
		void OnSelect(int i)
		{
		}
		void OnPoolMana(ManaColor color)
		{
		}
		void OnUseUs()
		{
		}
		void OnEndTurn()
		{
		}
		void OnToggleBaseDeck()
		{
		}
		void OnToggleDrawZone()
		{
		}
		void OnToggleDiscardZone()
		{
		}
		void OnToggleExileZone()
		{
		}
		void OnToggleMap()
		{
		}
		void OnToggleMinimize()
		{
		}
		void Reset()
		{
		}
	}
}
