using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.Stat_System
{
	public interface IBonusManager
	{
		float CalculateBonus(StatType statType);
		void ApplyBonus(string sourceId, float value);
		void RemoveBonus(string sourceId);
		void CheckMilestones();
	}
}
