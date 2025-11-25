using System;
namespace LBoL.Presentation.Bullet
{
	public class BulletEvent
	{
		public BulletEvent(int eventStart, int eventDuration, float[][] eventNumber, int[] eventType)
		{
			this.EventStart = eventStart;
			this.EventDuration = eventDuration;
			this.EventNumber = eventNumber;
			this.EventType = eventType;
		}
		public int EventStart { get; }
		public int EventDuration { get; }
		public float[][] EventNumber { get; }
		public float RuntimeEventNumber { get; set; }
		public int[] EventType { get; }
		public bool EvSetup { get; set; }
		public int EvTimer { get; set; }
		public float Delta { get; set; }
	}
}
