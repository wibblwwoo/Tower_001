using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Core heartbeat system for managing game ticks and time-based progression
/// </summary>
public partial class HeartbeatManager : BaseManager
{
	// Core timing settings
	private float _baseTickRate = 1.0f;  // Base tick rate in seconds
	private float _timeAccumulator = 0.0f;
	private bool _isPaused = false;

	// Performance tracking
	private Stopwatch _tickStopwatch;
	private float _lastTickDuration;
	private int _tickCount;

	// Time multipliers
	private float _globalSpeedMultiplier = 1.0f;
	private Dictionary<string, float> _systemSpeedMultipliers = new();

	// Time tracking
	private DateTime _gameStartTime;
	private TimeSpan _totalGameTime;
	private TimeSpan _sessionTime;

	public HeartbeatManager()
	{
		_tickStopwatch = new Stopwatch();
		_gameStartTime = DateTime.UtcNow;
		Pause();
		base.Setup();
	}

	protected override void RegisterEventHandlers()
	{
		_tickStopwatch.Start();
		_sessionTime = TimeSpan.Zero;
	}
	public override void Setup()
	{
	}

	/// <summary>
	/// Process game tick. Should be called from _Process in Godot
	/// </summary>
	public void Update(double delta)
	{
		if (_isPaused) return;

		_timeAccumulator += (float)delta * _globalSpeedMultiplier;

		// Check if enough time has accumulated for a tick
		while (_timeAccumulator >= _baseTickRate)
		{
			ProcessTick();
			_timeAccumulator -= _baseTickRate;
		}

		// Update time tracking
		_sessionTime = DateTime.UtcNow - _gameStartTime;
	}

	private void ProcessTick()
	{
		_tickStopwatch.Restart();

		// Raise tick event through event manager
		if (Globals.Instance?.gameMangers?.Events != null)
		{
			var tickEventArgs = new TickEventArgs(_tickCount,_baseTickRate,_globalSpeedMultiplier,_systemSpeedMultipliers);

			Globals.Instance.gameMangers.Events.RaiseEvent(GlobalEnums.EventType.GameTick,tickEventArgs);
		}

		_tickStopwatch.Stop();
		_lastTickDuration = (float)_tickStopwatch.Elapsed.TotalSeconds;
		_tickCount++;
	}

	/// <summary>
	/// Sets a speed multiplier for a specific system
	/// </summary>
	public void SetSystemSpeedMultiplier(string systemId, float multiplier)
	{
		_systemSpeedMultipliers[systemId] = multiplier;
	}

	/// <summary>
	/// Sets the global speed multiplier for all systems
	/// </summary>
	public void SetGlobalSpeedMultiplier(float multiplier)
	{
		_globalSpeedMultiplier = Math.Max(0.0f, multiplier);
	}

	/// <summary>
	/// Gets the current effective speed multiplier for a system
	/// </summary>
	public float GetEffectiveSpeedMultiplier(string systemId)
	{
		float systemMultiplier = _systemSpeedMultipliers.GetValueOrDefault(systemId, 1.0f);
		return systemMultiplier * _globalSpeedMultiplier;
	}

	/// <summary>
	/// Pauses the heartbeat system
	/// </summary>
	public void Pause()
	{
		_isPaused = true;
		_tickStopwatch.Stop();
	}

	/// <summary>
	/// Resumes the heartbeat system
	/// </summary>
	public void Resume()
	{
		_isPaused = false;
		_tickStopwatch.Start();
	}

	/// <summary>
	/// Gets performance metrics for monitoring
	/// </summary>
	public (int tickCount, float lastTickDuration, TimeSpan sessionTime) GetMetrics()
	{
		return (_tickCount, _lastTickDuration, _sessionTime);
	}

	/// <summary>
	/// Sets the base tick rate
	/// </summary>
	public void SetBaseTickRate(float tickRate)
	{
		_baseTickRate = Math.Max(0.01f, tickRate);
	}

	
}