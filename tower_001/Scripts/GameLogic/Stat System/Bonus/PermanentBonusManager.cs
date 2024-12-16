using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GlobalEnums;
using Tower_001.Scripts.GameLogic.Progression;

namespace Tower_001.Scripts.GameLogic.Stat_System
{
	public class PermanentBonusManager
	{
		private readonly PrestigeManager _prestigeManager;
		private readonly AscensionManagerNew _ascensionManager;

		public float CalculateTotalBonus(StatType statType)
		{
			float prestigeBonus = _prestigeManager.CalculateBonus(statType);
			float ascensionBonus = _ascensionManager.CalculateBonus(statType);

			return (1 + prestigeBonus) * (1 + ascensionBonus) - 1;
		}
	}
}
