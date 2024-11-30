using Godot;
using System;

public static partial class GlobalEnums
{
	// Called when the node enters the scene tree for the first time.

	[Flags]
	public enum TestCategory
	{
		None = 0,
		// Core Systems
		Character = 1 <<0,
		Events = 1 << 1,
		Progression = 1 << 2,

		// Progression Systems
		BasicLeveling = 1 << 3,
		Prestige = 1 << 4,
		Ascension = 1 << 5,

		// Content Systems
		Tower = 1 << 6,
		Floor = 1 << 7,
		Room = 1 << 8,
		Resources = 1 << 9,
		Resources_Collectors = 1 << 10,

		Tower_Progress = 1 << 11,
		Performance	 = 1 << 12,
	}

	[Flags]
	public enum TestType
	{
		None = 0,

		// Character Tests
		Character = 1 << 0,
		BuffSystem = 1 << 1,

		// Prestige Tests
		Prestige = 1 << 2,
		// Ascension Tests
		Ascension = 1 << 3,
		AscensionReset = 1 << 4,
		AscensionBonuses = 1 << 5,

		// Common Test Types
		Creation = 1 << 6,
		Basic = 1 << 7,
		Advanced = 1 << 8,
		Integration = 1 << 9,
		Performance = 1 << 10,
		Stress = 1 << 11,

		ElementalInteractions = 1 << 12,


		//Tower Tests
		Tower = 1 << 13,
		Floor = 1 << 14,
		Room = 1 << 15,

		All = ~None
	}

	// Log categories for filtering specific types of messages
	
}
