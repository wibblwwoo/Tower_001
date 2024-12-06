using Godot;
using System;
using static GlobalEnums;

public partial class ManagerException : Exception
{
	public ManagerState ManagerState { get; }
	public string ManagerName { get; }
	public ManagerErrorType ErrorType { get; }

	public ManagerException(string message, string managerName, ManagerState state, ManagerErrorType errorType, Exception innerException = null)
		: base(message, innerException)
	{
		ManagerName = managerName;
		ManagerState = state;
		ErrorType = errorType;
	}
}