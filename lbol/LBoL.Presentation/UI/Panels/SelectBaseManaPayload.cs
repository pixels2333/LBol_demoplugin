using System;
using LBoL.Base;
namespace LBoL.Presentation.UI.Panels
{
	public class SelectBaseManaPayload
	{
		public ManaGroup BaseMana { get; set; }
		public int Min { get; set; }
		public int Max { get; set; }
		public bool CanCancel { get; set; }
	}
}
