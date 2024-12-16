using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tower_001.Scripts.GameLogic.Reward_System
{
	public abstract class MilestoneReward
	{
		public string Description { get; }

		protected MilestoneReward(string description)
		{
			Description = description;
		}

		public abstract void Apply(string characterId);
	}

}
