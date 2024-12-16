using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tower_001.Scripts.Events
{
	public abstract class BaseStatEventArgs : GameEventArgs
	{
		public string CharacterId { get; }
		public new DateTime Timestamp { get; }

		protected BaseStatEventArgs(string characterId)
		{
			CharacterId = characterId;
			Timestamp = DateTime.UtcNow;
		}
	}
}
