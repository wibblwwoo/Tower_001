using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GlobalEnums;

namespace Tower_001.Scripts.Events
{
	public class StatOverflowEventArgs : BaseStatEventArgs
	{
		public StatType StatType { get; }
		public float AttemptedValue { get; }
		public float MaxAllowedValue { get; }
		public string Source { get; }

		public StatOverflowEventArgs(
			string characterId,
			StatType statType,
			float attemptedValue,
			float maxAllowedValue,
			string source) : base(characterId)
		{
			StatType = statType;
			AttemptedValue = attemptedValue;
			MaxAllowedValue = maxAllowedValue;
			Source = source;
		}
	}
}
