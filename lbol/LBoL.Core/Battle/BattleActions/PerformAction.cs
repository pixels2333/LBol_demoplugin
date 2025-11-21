using System;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000196 RID: 406
	public class PerformAction : SimpleAction
	{
		// Token: 0x1700051C RID: 1308
		// (get) Token: 0x06000EE2 RID: 3810 RVA: 0x00028344 File Offset: 0x00026544
		public PerformAction.PerformArgs Args { get; }

		// Token: 0x06000EE3 RID: 3811 RVA: 0x0002834C File Offset: 0x0002654C
		private PerformAction(PerformAction.PerformArgs args)
		{
			this.Args = args;
		}

		// Token: 0x06000EE4 RID: 3812 RVA: 0x0002835B File Offset: 0x0002655B
		public static PerformAction ViewCard(Card card)
		{
			return new PerformAction(new PerformAction.ViewCardArgs
			{
				Card = card,
				Zone = card.Zone
			});
		}

		// Token: 0x06000EE5 RID: 3813 RVA: 0x0002837A File Offset: 0x0002657A
		public static PerformAction Gun(Unit source, Unit target, string gunId, float waitTime = 0f)
		{
			return new PerformAction(new PerformAction.GunArgs
			{
				Source = source,
				Target = target,
				GunId = gunId,
				WaitTime = waitTime
			});
		}

		// Token: 0x06000EE6 RID: 3814 RVA: 0x000283A2 File Offset: 0x000265A2
		public static PerformAction Doll(Doll doll, Unit target, string gunId, float waitTime, string debugString = "")
		{
			return new PerformAction(new PerformAction.DollArgs
			{
				Doll = doll,
				Target = target,
				GunId = gunId,
				WaitTime = waitTime,
				DebugString = debugString
			});
		}

		// Token: 0x06000EE7 RID: 3815 RVA: 0x000283D2 File Offset: 0x000265D2
		public static PerformAction Animation(Unit source, string animationName, float waitTime = 0f, string sfxId = null, float sfxDelay = 0f, int shakeLevel = -1)
		{
			return new PerformAction(new PerformAction.AnimationArgs
			{
				Source = source,
				AnimationName = animationName,
				WaitTime = waitTime,
				SfxId = sfxId,
				SfxDelay = sfxDelay,
				ShakeLevel = shakeLevel
			});
		}

		// Token: 0x06000EE8 RID: 3816 RVA: 0x0002840A File Offset: 0x0002660A
		public static PerformAction Sfx(string id, float delay = 0f)
		{
			return new PerformAction(new PerformAction.SfxArgs
			{
				Id = id,
				Delay = delay
			});
		}

		// Token: 0x06000EE9 RID: 3817 RVA: 0x00028424 File Offset: 0x00026624
		public static PerformAction UiSound(string id)
		{
			return new PerformAction(new PerformAction.UiSoundArgs
			{
				Id = id
			});
		}

		// Token: 0x06000EEA RID: 3818 RVA: 0x00028437 File Offset: 0x00026637
		public static PerformAction Chat(Unit source, string content, float chatTime, float delay = 0f, float waitTime = 0f, bool talk = true)
		{
			return new PerformAction(new PerformAction.ChatArgs
			{
				Source = source,
				Content = content,
				ChatTime = chatTime,
				Delay = delay,
				WaitTime = waitTime,
				Talk = talk
			});
		}

		// Token: 0x06000EEB RID: 3819 RVA: 0x0002846F File Offset: 0x0002666F
		public static PerformAction Spell(Unit source, string spellName)
		{
			return new PerformAction(new PerformAction.SpellArgs
			{
				Source = source,
				SpellName = spellName
			});
		}

		// Token: 0x06000EEC RID: 3820 RVA: 0x00028489 File Offset: 0x00026689
		public static PerformAction Effect(Unit source, string effectName, float delay = 0f, string sfxId = null, float sfxDelay = 0f, PerformAction.EffectBehavior effectType = PerformAction.EffectBehavior.PlayOneShot, float waitTime = 0f)
		{
			return new PerformAction(new PerformAction.EffectArgs
			{
				Source = source,
				EffectName = effectName,
				Delay = delay,
				SfxId = sfxId,
				SfxDelay = sfxDelay,
				EffectType = effectType,
				WaitTime = waitTime
			});
		}

		// Token: 0x06000EED RID: 3821 RVA: 0x000284C9 File Offset: 0x000266C9
		public static PerformAction EffectMessage(Unit source, string effectName, string message, object args)
		{
			return new PerformAction(new PerformAction.EffectMessageArgs
			{
				Source = source,
				EffectName = effectName,
				Message = message,
				Args = args
			});
		}

		// Token: 0x06000EEE RID: 3822 RVA: 0x000284F1 File Offset: 0x000266F1
		public static PerformAction SePop(Unit source, string popContent)
		{
			return new PerformAction(new PerformAction.SePopArgs
			{
				Source = source,
				PopContent = popContent
			});
		}

		// Token: 0x06000EEF RID: 3823 RVA: 0x0002850B File Offset: 0x0002670B
		public static PerformAction SummonFriend(Card card)
		{
			return new PerformAction(new PerformAction.SummonFriendArgs
			{
				Card = card
			});
		}

		// Token: 0x06000EF0 RID: 3824 RVA: 0x0002851E File Offset: 0x0002671E
		public static PerformAction TransformModel(Unit source, string modelName)
		{
			return new PerformAction(new PerformAction.TransformModelArgs
			{
				Source = source,
				ModelName = modelName
			});
		}

		// Token: 0x06000EF1 RID: 3825 RVA: 0x00028538 File Offset: 0x00026738
		public static PerformAction DeathAnimation(Unit source)
		{
			return new PerformAction(new PerformAction.DeathAnimationArgs
			{
				Source = source
			});
		}

		// Token: 0x06000EF2 RID: 3826 RVA: 0x0002854B File Offset: 0x0002674B
		public static PerformAction Wait(float time, bool unscale = false)
		{
			return new PerformAction(new PerformAction.WaitArgs
			{
				Time = time,
				Unscale = unscale
			});
		}

		// Token: 0x020002ED RID: 749
		public abstract class PerformArgs
		{
		}

		// Token: 0x020002EE RID: 750
		public sealed class ViewCardArgs : PerformAction.PerformArgs
		{
			// Token: 0x1700064D RID: 1613
			// (get) Token: 0x060015B2 RID: 5554 RVA: 0x0003DA77 File Offset: 0x0003BC77
			// (set) Token: 0x060015B3 RID: 5555 RVA: 0x0003DA7F File Offset: 0x0003BC7F
			public Card Card { get; set; }

			// Token: 0x1700064E RID: 1614
			// (get) Token: 0x060015B4 RID: 5556 RVA: 0x0003DA88 File Offset: 0x0003BC88
			// (set) Token: 0x060015B5 RID: 5557 RVA: 0x0003DA90 File Offset: 0x0003BC90
			public CardZone Zone { get; set; }
		}

		// Token: 0x020002EF RID: 751
		public sealed class GunArgs : PerformAction.PerformArgs
		{
			// Token: 0x1700064F RID: 1615
			// (get) Token: 0x060015B7 RID: 5559 RVA: 0x0003DAA1 File Offset: 0x0003BCA1
			// (set) Token: 0x060015B8 RID: 5560 RVA: 0x0003DAA9 File Offset: 0x0003BCA9
			public Unit Source { get; set; }

			// Token: 0x17000650 RID: 1616
			// (get) Token: 0x060015B9 RID: 5561 RVA: 0x0003DAB2 File Offset: 0x0003BCB2
			// (set) Token: 0x060015BA RID: 5562 RVA: 0x0003DABA File Offset: 0x0003BCBA
			public Unit Target { get; set; }

			// Token: 0x17000651 RID: 1617
			// (get) Token: 0x060015BB RID: 5563 RVA: 0x0003DAC3 File Offset: 0x0003BCC3
			// (set) Token: 0x060015BC RID: 5564 RVA: 0x0003DACB File Offset: 0x0003BCCB
			public string GunId { get; set; }

			// Token: 0x17000652 RID: 1618
			// (get) Token: 0x060015BD RID: 5565 RVA: 0x0003DAD4 File Offset: 0x0003BCD4
			// (set) Token: 0x060015BE RID: 5566 RVA: 0x0003DADC File Offset: 0x0003BCDC
			public float WaitTime { get; set; }
		}

		// Token: 0x020002F0 RID: 752
		public sealed class DollArgs : PerformAction.PerformArgs
		{
			// Token: 0x17000653 RID: 1619
			// (get) Token: 0x060015C0 RID: 5568 RVA: 0x0003DAED File Offset: 0x0003BCED
			// (set) Token: 0x060015C1 RID: 5569 RVA: 0x0003DAF5 File Offset: 0x0003BCF5
			public Doll Doll { get; set; }

			// Token: 0x17000654 RID: 1620
			// (get) Token: 0x060015C2 RID: 5570 RVA: 0x0003DAFE File Offset: 0x0003BCFE
			// (set) Token: 0x060015C3 RID: 5571 RVA: 0x0003DB06 File Offset: 0x0003BD06
			public Unit Target { get; set; }

			// Token: 0x17000655 RID: 1621
			// (get) Token: 0x060015C4 RID: 5572 RVA: 0x0003DB0F File Offset: 0x0003BD0F
			// (set) Token: 0x060015C5 RID: 5573 RVA: 0x0003DB17 File Offset: 0x0003BD17
			public string GunId { get; set; }

			// Token: 0x17000656 RID: 1622
			// (get) Token: 0x060015C6 RID: 5574 RVA: 0x0003DB20 File Offset: 0x0003BD20
			// (set) Token: 0x060015C7 RID: 5575 RVA: 0x0003DB28 File Offset: 0x0003BD28
			public float WaitTime { get; set; }

			// Token: 0x17000657 RID: 1623
			// (get) Token: 0x060015C8 RID: 5576 RVA: 0x0003DB31 File Offset: 0x0003BD31
			// (set) Token: 0x060015C9 RID: 5577 RVA: 0x0003DB39 File Offset: 0x0003BD39
			public string DebugString { get; set; }
		}

		// Token: 0x020002F1 RID: 753
		public sealed class AnimationArgs : PerformAction.PerformArgs
		{
			// Token: 0x17000658 RID: 1624
			// (get) Token: 0x060015CB RID: 5579 RVA: 0x0003DB4A File Offset: 0x0003BD4A
			// (set) Token: 0x060015CC RID: 5580 RVA: 0x0003DB52 File Offset: 0x0003BD52
			public Unit Source { get; set; }

			// Token: 0x17000659 RID: 1625
			// (get) Token: 0x060015CD RID: 5581 RVA: 0x0003DB5B File Offset: 0x0003BD5B
			// (set) Token: 0x060015CE RID: 5582 RVA: 0x0003DB63 File Offset: 0x0003BD63
			public string AnimationName { get; set; }

			// Token: 0x1700065A RID: 1626
			// (get) Token: 0x060015CF RID: 5583 RVA: 0x0003DB6C File Offset: 0x0003BD6C
			// (set) Token: 0x060015D0 RID: 5584 RVA: 0x0003DB74 File Offset: 0x0003BD74
			public float WaitTime { get; set; }

			// Token: 0x1700065B RID: 1627
			// (get) Token: 0x060015D1 RID: 5585 RVA: 0x0003DB7D File Offset: 0x0003BD7D
			// (set) Token: 0x060015D2 RID: 5586 RVA: 0x0003DB85 File Offset: 0x0003BD85
			public string SfxId { get; set; }

			// Token: 0x1700065C RID: 1628
			// (get) Token: 0x060015D3 RID: 5587 RVA: 0x0003DB8E File Offset: 0x0003BD8E
			// (set) Token: 0x060015D4 RID: 5588 RVA: 0x0003DB96 File Offset: 0x0003BD96
			public float SfxDelay { get; set; }

			// Token: 0x1700065D RID: 1629
			// (get) Token: 0x060015D5 RID: 5589 RVA: 0x0003DB9F File Offset: 0x0003BD9F
			// (set) Token: 0x060015D6 RID: 5590 RVA: 0x0003DBA7 File Offset: 0x0003BDA7
			public int ShakeLevel { get; set; }
		}

		// Token: 0x020002F2 RID: 754
		public sealed class SfxArgs : PerformAction.PerformArgs
		{
			// Token: 0x1700065E RID: 1630
			// (get) Token: 0x060015D8 RID: 5592 RVA: 0x0003DBB8 File Offset: 0x0003BDB8
			// (set) Token: 0x060015D9 RID: 5593 RVA: 0x0003DBC0 File Offset: 0x0003BDC0
			public string Id { get; set; }

			// Token: 0x1700065F RID: 1631
			// (get) Token: 0x060015DA RID: 5594 RVA: 0x0003DBC9 File Offset: 0x0003BDC9
			// (set) Token: 0x060015DB RID: 5595 RVA: 0x0003DBD1 File Offset: 0x0003BDD1
			public float Delay { get; set; }
		}

		// Token: 0x020002F3 RID: 755
		public sealed class UiSoundArgs : PerformAction.PerformArgs
		{
			// Token: 0x17000660 RID: 1632
			// (get) Token: 0x060015DD RID: 5597 RVA: 0x0003DBE2 File Offset: 0x0003BDE2
			// (set) Token: 0x060015DE RID: 5598 RVA: 0x0003DBEA File Offset: 0x0003BDEA
			public string Id { get; set; }
		}

		// Token: 0x020002F4 RID: 756
		public sealed class ChatArgs : PerformAction.PerformArgs
		{
			// Token: 0x17000661 RID: 1633
			// (get) Token: 0x060015E0 RID: 5600 RVA: 0x0003DBFB File Offset: 0x0003BDFB
			// (set) Token: 0x060015E1 RID: 5601 RVA: 0x0003DC03 File Offset: 0x0003BE03
			public Unit Source { get; set; }

			// Token: 0x17000662 RID: 1634
			// (get) Token: 0x060015E2 RID: 5602 RVA: 0x0003DC0C File Offset: 0x0003BE0C
			// (set) Token: 0x060015E3 RID: 5603 RVA: 0x0003DC14 File Offset: 0x0003BE14
			public string Content { get; set; }

			// Token: 0x17000663 RID: 1635
			// (get) Token: 0x060015E4 RID: 5604 RVA: 0x0003DC1D File Offset: 0x0003BE1D
			// (set) Token: 0x060015E5 RID: 5605 RVA: 0x0003DC25 File Offset: 0x0003BE25
			public float ChatTime { get; set; }

			// Token: 0x17000664 RID: 1636
			// (get) Token: 0x060015E6 RID: 5606 RVA: 0x0003DC2E File Offset: 0x0003BE2E
			// (set) Token: 0x060015E7 RID: 5607 RVA: 0x0003DC36 File Offset: 0x0003BE36
			public float Delay { get; set; }

			// Token: 0x17000665 RID: 1637
			// (get) Token: 0x060015E8 RID: 5608 RVA: 0x0003DC3F File Offset: 0x0003BE3F
			// (set) Token: 0x060015E9 RID: 5609 RVA: 0x0003DC47 File Offset: 0x0003BE47
			public float WaitTime { get; set; }

			// Token: 0x17000666 RID: 1638
			// (get) Token: 0x060015EA RID: 5610 RVA: 0x0003DC50 File Offset: 0x0003BE50
			// (set) Token: 0x060015EB RID: 5611 RVA: 0x0003DC58 File Offset: 0x0003BE58
			public bool Talk { get; set; }
		}

		// Token: 0x020002F5 RID: 757
		public sealed class SpellArgs : PerformAction.PerformArgs
		{
			// Token: 0x17000667 RID: 1639
			// (get) Token: 0x060015ED RID: 5613 RVA: 0x0003DC69 File Offset: 0x0003BE69
			// (set) Token: 0x060015EE RID: 5614 RVA: 0x0003DC71 File Offset: 0x0003BE71
			public Unit Source { get; set; }

			// Token: 0x17000668 RID: 1640
			// (get) Token: 0x060015EF RID: 5615 RVA: 0x0003DC7A File Offset: 0x0003BE7A
			// (set) Token: 0x060015F0 RID: 5616 RVA: 0x0003DC82 File Offset: 0x0003BE82
			public string SpellName { get; set; }
		}

		// Token: 0x020002F6 RID: 758
		public sealed class EffectArgs : PerformAction.PerformArgs
		{
			// Token: 0x17000669 RID: 1641
			// (get) Token: 0x060015F2 RID: 5618 RVA: 0x0003DC93 File Offset: 0x0003BE93
			// (set) Token: 0x060015F3 RID: 5619 RVA: 0x0003DC9B File Offset: 0x0003BE9B
			public Unit Source { get; set; }

			// Token: 0x1700066A RID: 1642
			// (get) Token: 0x060015F4 RID: 5620 RVA: 0x0003DCA4 File Offset: 0x0003BEA4
			// (set) Token: 0x060015F5 RID: 5621 RVA: 0x0003DCAC File Offset: 0x0003BEAC
			public string EffectName { get; set; }

			// Token: 0x1700066B RID: 1643
			// (get) Token: 0x060015F6 RID: 5622 RVA: 0x0003DCB5 File Offset: 0x0003BEB5
			// (set) Token: 0x060015F7 RID: 5623 RVA: 0x0003DCBD File Offset: 0x0003BEBD
			public float Delay { get; set; }

			// Token: 0x1700066C RID: 1644
			// (get) Token: 0x060015F8 RID: 5624 RVA: 0x0003DCC6 File Offset: 0x0003BEC6
			// (set) Token: 0x060015F9 RID: 5625 RVA: 0x0003DCCE File Offset: 0x0003BECE
			public float WaitTime { get; set; }

			// Token: 0x1700066D RID: 1645
			// (get) Token: 0x060015FA RID: 5626 RVA: 0x0003DCD7 File Offset: 0x0003BED7
			// (set) Token: 0x060015FB RID: 5627 RVA: 0x0003DCDF File Offset: 0x0003BEDF
			public string SfxId { get; set; }

			// Token: 0x1700066E RID: 1646
			// (get) Token: 0x060015FC RID: 5628 RVA: 0x0003DCE8 File Offset: 0x0003BEE8
			// (set) Token: 0x060015FD RID: 5629 RVA: 0x0003DCF0 File Offset: 0x0003BEF0
			public float SfxDelay { get; set; }

			// Token: 0x1700066F RID: 1647
			// (get) Token: 0x060015FE RID: 5630 RVA: 0x0003DCF9 File Offset: 0x0003BEF9
			// (set) Token: 0x060015FF RID: 5631 RVA: 0x0003DD01 File Offset: 0x0003BF01
			public PerformAction.EffectBehavior EffectType { get; set; }
		}

		// Token: 0x020002F7 RID: 759
		public class EffectMessageArgs : PerformAction.PerformArgs
		{
			// Token: 0x17000670 RID: 1648
			// (get) Token: 0x06001601 RID: 5633 RVA: 0x0003DD12 File Offset: 0x0003BF12
			// (set) Token: 0x06001602 RID: 5634 RVA: 0x0003DD1A File Offset: 0x0003BF1A
			public Unit Source { get; set; }

			// Token: 0x17000671 RID: 1649
			// (get) Token: 0x06001603 RID: 5635 RVA: 0x0003DD23 File Offset: 0x0003BF23
			// (set) Token: 0x06001604 RID: 5636 RVA: 0x0003DD2B File Offset: 0x0003BF2B
			public string EffectName { get; set; }

			// Token: 0x17000672 RID: 1650
			// (get) Token: 0x06001605 RID: 5637 RVA: 0x0003DD34 File Offset: 0x0003BF34
			// (set) Token: 0x06001606 RID: 5638 RVA: 0x0003DD3C File Offset: 0x0003BF3C
			public string Message { get; set; }

			// Token: 0x17000673 RID: 1651
			// (get) Token: 0x06001607 RID: 5639 RVA: 0x0003DD45 File Offset: 0x0003BF45
			// (set) Token: 0x06001608 RID: 5640 RVA: 0x0003DD4D File Offset: 0x0003BF4D
			public object Args { get; set; }
		}

		// Token: 0x020002F8 RID: 760
		public sealed class SePopArgs : PerformAction.PerformArgs
		{
			// Token: 0x17000674 RID: 1652
			// (get) Token: 0x0600160A RID: 5642 RVA: 0x0003DD5E File Offset: 0x0003BF5E
			// (set) Token: 0x0600160B RID: 5643 RVA: 0x0003DD66 File Offset: 0x0003BF66
			public Unit Source { get; set; }

			// Token: 0x17000675 RID: 1653
			// (get) Token: 0x0600160C RID: 5644 RVA: 0x0003DD6F File Offset: 0x0003BF6F
			// (set) Token: 0x0600160D RID: 5645 RVA: 0x0003DD77 File Offset: 0x0003BF77
			public string PopContent { get; set; }
		}

		// Token: 0x020002F9 RID: 761
		public sealed class SummonFriendArgs : PerformAction.PerformArgs
		{
			// Token: 0x17000676 RID: 1654
			// (get) Token: 0x0600160F RID: 5647 RVA: 0x0003DD88 File Offset: 0x0003BF88
			// (set) Token: 0x06001610 RID: 5648 RVA: 0x0003DD90 File Offset: 0x0003BF90
			public Card Card { get; set; }
		}

		// Token: 0x020002FA RID: 762
		public sealed class TransformModelArgs : PerformAction.PerformArgs
		{
			// Token: 0x17000677 RID: 1655
			// (get) Token: 0x06001612 RID: 5650 RVA: 0x0003DDA1 File Offset: 0x0003BFA1
			// (set) Token: 0x06001613 RID: 5651 RVA: 0x0003DDA9 File Offset: 0x0003BFA9
			public Unit Source { get; set; }

			// Token: 0x17000678 RID: 1656
			// (get) Token: 0x06001614 RID: 5652 RVA: 0x0003DDB2 File Offset: 0x0003BFB2
			// (set) Token: 0x06001615 RID: 5653 RVA: 0x0003DDBA File Offset: 0x0003BFBA
			public string ModelName { get; set; }
		}

		// Token: 0x020002FB RID: 763
		public sealed class DeathAnimationArgs : PerformAction.PerformArgs
		{
			// Token: 0x17000679 RID: 1657
			// (get) Token: 0x06001617 RID: 5655 RVA: 0x0003DDCB File Offset: 0x0003BFCB
			// (set) Token: 0x06001618 RID: 5656 RVA: 0x0003DDD3 File Offset: 0x0003BFD3
			public Unit Source { get; set; }
		}

		// Token: 0x020002FC RID: 764
		public sealed class WaitArgs : PerformAction.PerformArgs
		{
			// Token: 0x1700067A RID: 1658
			// (get) Token: 0x0600161A RID: 5658 RVA: 0x0003DDE4 File Offset: 0x0003BFE4
			// (set) Token: 0x0600161B RID: 5659 RVA: 0x0003DDEC File Offset: 0x0003BFEC
			public float Time { get; set; }

			// Token: 0x1700067B RID: 1659
			// (get) Token: 0x0600161C RID: 5660 RVA: 0x0003DDF5 File Offset: 0x0003BFF5
			// (set) Token: 0x0600161D RID: 5661 RVA: 0x0003DDFD File Offset: 0x0003BFFD
			public bool Unscale { get; set; }
		}

		// Token: 0x020002FD RID: 765
		public enum EffectBehavior
		{
			// Token: 0x04000BD8 RID: 3032
			PlayOneShot,
			// Token: 0x04000BD9 RID: 3033
			Add,
			// Token: 0x04000BDA RID: 3034
			Remove,
			// Token: 0x04000BDB RID: 3035
			DieOut
		}
	}
}
