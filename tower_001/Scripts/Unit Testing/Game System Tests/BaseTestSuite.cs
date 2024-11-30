using static GlobalEnums;
using System.Collections.Generic;


public abstract class BaseTestSuite
{
	protected readonly TestConfiguration TestConfig;
	protected readonly List<string> TestResults = new();

	protected BaseTestSuite(TestConfiguration testConfig)
	{
		TestConfig = testConfig;
	}

	
}

//public interface IBaseTestSuite
//{

//	TestConfiguration TestConfig { get; set; }
//	List<string> TestResults { get; set;}

//	void BaseTestSuite(TestConfiguration testConfig);
//	bool ShouldRunTest(TestType testType);

//	TestCategory GetCategory();

//}
