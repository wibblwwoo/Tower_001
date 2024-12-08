using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using static GlobalEnums;

/// <summary>
/// Centralized logging utility for controlling debug output throughout the application
/// </summary>
/// DebugLogger.Log($"Some message", LogCategory.Progress | LogCategory.Heartbeat);
public static class DebugLogger
{
	// Controls whether logging is enabled
	private static bool _isEnabled = false;
	private static bool _fileLoggingEnabled = false;
	private static readonly object _fileLock = new object();

	// Logging paths
	public static string BaseLogPath { get; private set; } = @"D:\Project_Output\Logging";
	private static string _currentLogFile = string.Empty;

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
		UI_States = 1 << 15,
		All = ~0,
		UI_ManagerInitializing = 32769,
		Stats_Debug = 32770
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
	/// Sets the base directory path for log files
	/// </summary>
	public static void SetBaseLogPath(string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			BaseLogPath = path;
			if (_fileLoggingEnabled)
			{
				InitializeLogFile(); // Reinitialize with new base path
			}
		}
	}

	/// <summary>
	/// Enables or disables file logging
	/// </summary>
	public static void SetFileLogging(bool enabled)
	{
		_fileLoggingEnabled = enabled;
		if (_fileLoggingEnabled)
		{
			InitializeLogFile();
		}
	}

	/// <summary>
	/// Initializes a new log file with timestamp in filename
	/// </summary>
	private static void InitializeLogFile()
	{
		try
		{
			// Create base directory if it doesn't exist
			if (!Directory.Exists(BaseLogPath))
			{
				Directory.CreateDirectory(BaseLogPath);
			}

			// Generate timestamp for filename
			string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
			_currentLogFile = Path.Combine(BaseLogPath, $"Debug-{timestamp}.log");

			// Create the file and write header
			using (StreamWriter writer = File.CreateText(_currentLogFile))
			{
				writer.WriteLine($"=== Debug Log Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to initialize log file: {ex.Message}");
			_fileLoggingEnabled = false;
		}
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
			string logMessage = $"[{timestamp}][{categoryName}] {message}";
			
			// Console logging
			GD.Print(logMessage);
			System.Diagnostics.Debug.WriteLine($"Godot: {message}");
			// File logging
			if (_fileLoggingEnabled)
			{
				WriteToLogFile(logMessage);
			}
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
		
		// Console logging
		GD.PrintErr(errorMessage);
		
		// File logging
		if (_fileLoggingEnabled)
		{
			WriteToLogFile(errorMessage);
		}
	}

	/// <summary>
	/// Logs a warning message if logging is enabled
	/// </summary>
	public static void LogWarning(string message)
	{
		if (_isEnabled)
		{
			string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
			string warningMessage = $"[{timestamp}][WARNING] {message}";
			
			// Console logging
			GD.PrintErr(warningMessage, "DebugLogger", "");
			
			// File logging
			if (_fileLoggingEnabled)
			{
				WriteToLogFile(warningMessage);
			}
		}
	}

	/// <summary>
	/// Writes a message to the log file
	/// </summary>
	private static void WriteToLogFile(string message)
	{
		if (string.IsNullOrEmpty(_currentLogFile))
		{
			InitializeLogFile();
		}

		try
		{
			lock (_fileLock)
			{
				File.AppendAllText(_currentLogFile, message + System.Environment.NewLine);
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to write to log file: {ex.Message}");
			_fileLoggingEnabled = false;
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