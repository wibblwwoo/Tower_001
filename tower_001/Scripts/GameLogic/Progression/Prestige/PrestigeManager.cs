using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GlobalEnums;
using Tower_001.Scripts.GameLogic.Stat_System;

namespace Tower_001.Scripts.GameLogic.Progression
{
	public class PrestigeManager : IBonusManager
	{
		private readonly Dictionary<string, float> _prestigeBonuses = new();
		private readonly Dictionary<int, float> _milestoneBonuses = new();

		public void ApplyBonus(string sourceId, float value)
		{
			throw new NotImplementedException();
		}

		public float CalculateBonus(StatType statType)
		{
			float baseMultiplier = 1f;
			foreach (var bonus in _prestigeBonuses.Values)
				baseMultiplier *= (1 + bonus);

			float milestoneMultiplier = 1f;
			foreach (var bonus in _milestoneBonuses.Values)
				milestoneMultiplier *= (1 + bonus);

			return baseMultiplier * milestoneMultiplier - 1f;
		}

		public void CheckMilestones(){}

		public void RemoveBonus(string sourceId){}
	}
}
