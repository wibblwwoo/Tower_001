using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Centralized logging utility for controlling debug output throughout the application
/// </summary>
/// DebugLogger.Log($"Some message", LogCategory.Progress | LogCategory.Heartbeat);
public static class DebugLogger
{
	// Controls whether logging is enabled
	private static bool _isEnabled = false;


	[Flags]
	public enum LogCategory
	{
		None = 0,
		General = 1 << 0,
		Character = 1 << 1,
		Combat = 1 << 2,
		Tower = 1 << 3,
		Floor = 1 << 4,
		Room = 1 << 5,
		Stats = 1 << 6,
		Events = 1 << 7,
		Tests = 1 << 8,
		Heartbeat = 1 << 9,  // New category
		Progress = 1 << 10,   // New category for progression tracking
		Rewards = 1 << 11,   // New category for progression tracking
		Resources = 1 << 12, // Resouce Storage system
		Resources_Collectors = 1 << 13, // resouce collection system
		Error = 1 << 14,
		All = ~0
	}

	// Current active log categories
	private static LogCategory _activeCategories = LogCategory.None;

	/// <summary>
	/// Enables or disables all logging
	/// </summary>
	public static void SetLogging(bool enabled)
	{
		_isEnabled = enabled;
	}

	/// <summary>
	/// Sets which categories of logs should be shown
	/// </summary>
	public static void SetLogCategories(LogCategory categories)
	{
		_activeCategories = categories;
	}

	/// <summary>
	/// Logs a message if logging is enabled and category is active
	/// </summary>
	public static void Log(string message, LogCategory category = LogCategory.General)
	{
		if (_isEnabled && _activeCategories.HasFlag(category))
		{
			string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
			string categoryName = category.ToString();
			GD.Print($"[{timestamp}][{categoryName}] {message}");
		}
	}

	/// <summary>
	/// Logs an error message regardless of logging state
	/// </summary>
	public static void LogError(string message, Exception ex = null)
	{
		string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
		string errorMessage = $"[{timestamp}][ERROR] {message}";
		if (ex != null)
		{
			errorMessage += $"\nException: {ex.Message}\nStackTrace: {ex.StackTrace}";
		}
		GD.PrintErr(errorMessage);
	}

	/// <summary>
	/// Logs a warning message if logging is enabled
	/// </summary>
	public static void LogWarning(string message)
	{
		if (_isEnabled)
		{
			string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
			GD.PrintErr($"[{timestamp}][WARNING] {message}", "DebugLogger", "");
		}
	}

	/// <summary>
	/// Logs a test result with specific formatting
	/// </summary>
	public static void LogTestResult(string testName, bool passed, string details = null)
	{
		if (_isEnabled && _activeCategories.HasFlag(LogCategory.Tests))
		{
			string result = passed ? "PASSED" : "FAILED";
			string message = $"Test {testName}: {result}";
			if (details != null)
			{
				message += $"\n    {details}";
			}
			GD.Print(message);
		}
	}
}