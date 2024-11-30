using static GlobalEnums;
using System;
using System.Collections.Generic;

public partial class TestConfiguration
{
	private Dictionary<TestCategory, bool> _enabledCategory = new();

	public TestConfiguration()
	{
		// Default all categories to run all tests
		foreach (TestCategory category in Enum.GetValues(typeof(TestCategory)))
		{
			_enabledCategory[category] = false;
		}
	}

	public void SetCategory(TestCategory category, bool Enabled)
	{
		_enabledCategory[category] = Enabled;
	}

	public bool IsCategoryEnabled(TestCategory category)
	{
		return _enabledCategory[category];
			   
	}
}