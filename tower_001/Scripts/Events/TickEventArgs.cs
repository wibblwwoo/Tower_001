
using System.Collections.Generic;

/// <summary>
/// Event arguments for game ticks
/// </summary>
public class TickEventArgs : GameEventArgs
{
	public int TickCount { get; }
	public float TickRate { get; }
	public float GlobalSpeedMultiplier { get; }
	public IReadOnlyDictionary<string, float> SystemSpeedMultipliers { get; }

	public TickEventArgs(int tickCount,float tickRate,float globalSpeedMultiplier,Dictionary<string, float> systemSpeedMultipliers){
		TickCount = tickCount;
		TickRate = tickRate;
		GlobalSpeedMultiplier = globalSpeedMultiplier;
		SystemSpeedMultipliers = systemSpeedMultipliers;
	}
}