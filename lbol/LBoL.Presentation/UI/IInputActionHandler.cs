using System;
using LBoL.Base;

namespace LBoL.Presentation.UI
{
	// Token: 0x0200001F RID: 31
	public interface IInputActionHandler
	{
		// Token: 0x060002FE RID: 766 RVA: 0x0000D53C File Offset: 0x0000B73C
		void OnConfirm()
		{
		}

		// Token: 0x060002FF RID: 767 RVA: 0x0000D53E File Offset: 0x0000B73E
		void OnCancel()
		{
		}

		// Token: 0x06000300 RID: 768 RVA: 0x0000D540 File Offset: 0x0000B740
		void OnRightClickCancel()
		{
			if (GameMaster.RightClickCancel)
			{
				this.OnCancel();
			}
		}

		// Token: 0x06000301 RID: 769 RVA: 0x0000D54F File Offset: 0x0000B74F
		void OnNavigate(NavigateDirection dir)
		{
		}

		// Token: 0x06000302 RID: 770 RVA: 0x0000D551 File Offset: 0x0000B751
		void BeginSkipDialog()
		{
		}

		// Token: 0x06000303 RID: 771 RVA: 0x0000D553 File Offset: 0x0000B753
		void EndSkipDialog()
		{
		}

		// Token: 0x06000304 RID: 772 RVA: 0x0000D555 File Offset: 0x0000B755
		void BeginShowEnemyMoveOrder()
		{
		}

		// Token: 0x06000305 RID: 773 RVA: 0x0000D557 File Offset: 0x0000B757
		void EndShowEnemyMoveOrder()
		{
		}

		// Token: 0x06000306 RID: 774 RVA: 0x0000D559 File Offset: 0x0000B759
		void OnSelect(int i)
		{
		}

		// Token: 0x06000307 RID: 775 RVA: 0x0000D55B File Offset: 0x0000B75B
		void OnPoolMana(ManaColor color)
		{
		}

		// Token: 0x06000308 RID: 776 RVA: 0x0000D55D File Offset: 0x0000B75D
		void OnUseUs()
		{
		}

		// Token: 0x06000309 RID: 777 RVA: 0x0000D55F File Offset: 0x0000B75F
		void OnEndTurn()
		{
		}

		// Token: 0x0600030A RID: 778 RVA: 0x0000D561 File Offset: 0x0000B761
		void OnToggleBaseDeck()
		{
		}

		// Token: 0x0600030B RID: 779 RVA: 0x0000D563 File Offset: 0x0000B763
		void OnToggleDrawZone()
		{
		}

		// Token: 0x0600030C RID: 780 RVA: 0x0000D565 File Offset: 0x0000B765
		void OnToggleDiscardZone()
		{
		}

		// Token: 0x0600030D RID: 781 RVA: 0x0000D567 File Offset: 0x0000B767
		void OnToggleExileZone()
		{
		}

		// Token: 0x0600030E RID: 782 RVA: 0x0000D569 File Offset: 0x0000B769
		void OnToggleMap()
		{
		}

		// Token: 0x0600030F RID: 783 RVA: 0x0000D56B File Offset: 0x0000B76B
		void OnToggleMinimize()
		{
		}

		// Token: 0x06000310 RID: 784 RVA: 0x0000D56D File Offset: 0x0000B76D
		void Reset()
		{
		}
	}
}
