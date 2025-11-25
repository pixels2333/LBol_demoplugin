using System;
using System.Globalization;
namespace LBoL.Presentation.I10N
{
	public class L10nInfo
	{
		public float VnTextRevealSpeed { get; set; }
		public float VnTextRevealAhead { get; set; }
		public bool PreferShortName { get; set; }
		public bool PreferItalicInFlavor { get; set; }
		public bool PreferWideTooltip { get; set; }
		public bool HideExhibitRarity { get; set; }
		public CultureInfo Culture { get; set; }
	}
}
