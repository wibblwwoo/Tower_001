using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GlobalEnums;
using Tower_001.Scripts.GameLogic.Stat_System;

namespace Tower_001.Scripts.GameLogic.Progression
{
	public class AscensionManagerNew : IBonusManager
	{
		private readonly Dictionary<string, float> _ascensionBonuses = new();

		public void ApplyBonus(string sourceId, float value)
		{
			throw new NotImplementedException();
		}

		public float CalculateBonus(StatType statType)
		{
			return _ascensionBonuses.Values.Sum();
		}

		public void CheckMilestones() { }

		public void RemoveBonus(string sourceId) { }
	}
}
