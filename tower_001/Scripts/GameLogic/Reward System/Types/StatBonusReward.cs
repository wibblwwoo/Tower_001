using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GlobalEnums;
using Tower_001.Scripts.GameLogic.Stat_System.Bonus;

namespace Tower_001.Scripts.GameLogic.Reward_System
{
	public class StatBonusReward : MilestoneReward
	{
		public StatType StatType { get; }
		public float BonusValue { get; }
		public BonusSource Source { get; }

		public StatBonusReward(
			StatType statType,
			float bonusValue,
			BonusSource source)
			: base($"+{bonusValue:P0} {statType} bonus")
		{
			StatType = statType;
			BonusValue = bonusValue;
			Source = source;
		}

		public override void Apply(string characterId)
		{
			var character = Globals.Instance.gameMangers.Player.GetCharacter(characterId);
			var bonus = new PermanentBonus(
				Description,
				StatType,
				Source,
				BonusValue, "Apply Bonsues"
			);
			//character._statSystem.AddPermanentBonus(bonus);
		}
	}
}
