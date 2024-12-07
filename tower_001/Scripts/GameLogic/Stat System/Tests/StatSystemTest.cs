using Godot;
using System;
using Tower_001.Scripts.GameLogic.StatSystem.Tests;

public partial class StatSystemTest : Node
{
    private StatSystemValidator _validator;


    public void Run()
    {
		// Create validator with a test character ID
		_validator = new StatSystemValidator("test_character_001");

		// Run full validation suite
		bool allTestsPassed = _validator.RunFullValidation();

		if (allTestsPassed)
		{
			GD.Print("All stat system validation tests passed!");
		}
		else
		{
			GD.PrintErr("Some stat system validation tests failed. Check the output log for details.");
		}

	}
	public override void _Ready()
    {
        // Add a slight delay to ensure all systems are initialized
        var timer = GetTree().CreateTimer(0.5f);
        timer.Timeout += () =>
        {
            GD.Print("\n=== Starting Stat System Validation Tests ===\n");
            Run();
        };
    }
}
