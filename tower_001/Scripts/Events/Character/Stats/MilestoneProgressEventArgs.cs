using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tower_001.Scripts.Events
{
	public class MilestoneProgressEventArgs : BaseStatEventArgs
	{
		public string MilestoneId { get; }
		public float Progress { get; }
		public float Target { get; }
		public bool IsComplete { get; }

		public MilestoneProgressEventArgs(
			string characterId,
			string milestoneId,
			float progress,
			float target) : base(characterId)
		{
			MilestoneId = milestoneId;
			Progress = progress;
			Target = target;
			IsComplete = progress >= target;
		}
	}

}
