using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static GlobalEnums;

/// <summary>
/// Core event manager responsible for handling all game events.
/// Implements the observer pattern with type safety and error handling.
/// </summary>
public partial class EventManager : IManager
{
	/// <summary>
	/// Dictionary storing all event handlers mapped to their event types
	/// </summary>
	private readonly Dictionary<EventType, List<Delegate>> _eventHandlers = new();

	public IEnumerable<Type> Dependencies { get; }


	/// <summary>
	/// Queue for events that need to be processed asynchronously
	/// </summary>
	private readonly Queue<(EventType type, GameEventArgs args)> _eventQueue = new();

	/// <summary>
	/// Lock object for thread safety
	/// </summary>
	private readonly object _lockObject = new();

	/// <summary>
	/// Flag to track if the manager is currently processing events
	/// </summary>
	private bool _isProcessing;

	/// <summary>
	/// Initializes the event manager and registers core event handlers
	/// </summary>
	public void Setup()
	{
		RegisterCoreEvents();
	}

	/// <summary>
	/// Registers core system event handlers
	/// </summary>
	private void RegisterCoreEvents()
	{
		// Register system-level events
		AddHandler<GameEventArgs>(EventType.None, OnSystemEvent);

		
		//// Register tower events
		//RegisterTowerEvents();

		//// Register floor events
		//RegisterFloorEvents();

		//// Register room events
		//RegisterRoomEvents();

		//// Register character events
		//RegisterCharacterEvents();
	}


	///// <summary>
	///// Adds a new event handler for the specified event type
	///// </summary>
	///// <typeparam name="T">Type of event arguments</typeparam>
	///// <param name="eventType">Type of event to handle</param>
	///// <param name="handler">Event handler delegate</param>
	public void AddHandler<T>(EventType eventType, Action<T> handler) where T : GameEventArgs
	{
		lock (_lockObject)
		{
			if (!_eventHandlers.ContainsKey(eventType))
			{
				_eventHandlers[eventType] = new List<Delegate>();
			}
			_eventHandlers[eventType].Add(handler);
		}
	}

	/// <summary>
	/// Removes an event handler for the specified event type
	/// </summary>
	/// <typeparam name="T">Type of event arguments</typeparam>
	/// <param name="eventType">Type of event</param>
	/// <param name="handler">Handler to remove</param>
	public void RemoveHandler<T>(EventType eventType, Action<T> handler) where T : GameEventArgs
	{
		lock (_lockObject)
		{
			if (_eventHandlers.ContainsKey(eventType))
			{
				_eventHandlers[eventType].Remove(handler);
			}
		}
	}

	/// <summary>
	/// Raises an event synchronously
	/// </summary>
	/// <typeparam name="T">Type of event arguments</typeparam>
	/// <param name="eventType">Type of event to raise</param>
	/// <param name="args">Event arguments</param>
	/// <returns>True if event was handled, false otherwise</returns>
	public bool RaiseEvent<T>(EventType eventType, T args) where T : GameEventArgs
	{
		if (!_eventHandlers.ContainsKey(eventType))
			return false;

		bool handled = false;
		foreach (var handler in _eventHandlers[eventType].ToList())
		{
			try
			{
				if (handler is Action<T> typedHandler)
				{
					typedHandler(args);
					handled = true;
				}
			}
			catch (Exception ex)
			{
				HandleEventError(eventType, args, ex);
			}
		}

		return handled;
	}



	
	/// <summary>
	/// Debug method to print all registered handlers
	/// </summary>
	public void PrintRegisteredHandlers()
	{
		DebugLogger.Log("\n=== Registered Event Handlers ===", DebugLogger.LogCategory.Events);
		foreach (var kvp in _eventHandlers)
		{
			DebugLogger.Log(
				$"Event Type: {kvp.Key} - Handlers: {kvp.Value.Count}",
				DebugLogger.LogCategory.Events
			);
			foreach (var handler in kvp.Value)
			{
				DebugLogger.Log(
					$"  - Handler: {handler.Method.Name} ({handler.Target?.GetType().Name})",
					DebugLogger.LogCategory.Events
				);
			}
		}
		DebugLogger.Log("===============================\n", DebugLogger.LogCategory.Events);
	}


	/// <summary>
	/// Queues an event for asynchronous processing
	/// </summary>
	/// <typeparam name="T">Type of event arguments</typeparam>
	/// <param name="eventType">Type of event to queue</param>
	/// <param name="args">Event arguments</param>
	/// </summary>
	public void QueueEvent<T>(EventType eventType, T args) where T : GameEventArgs
	{
		lock (_lockObject)
		{
			_eventQueue.Enqueue((eventType, args));
		}
	}

	/// <summary>
	/// Processes all queued events
	/// </summary>
	public void ProcessEvents()
	{
		if (_isProcessing) return;

		_isProcessing = true;
		try
		{
			while (_eventQueue.Count > 0)
			{
				(EventType type, GameEventArgs args) = _eventQueue.Dequeue();
				RaiseEvent(type, args);
			}
		}
		finally
		{
			_isProcessing = false;
		}
	}

	/// <summary>
	/// Handles errors that occur during event processing
	/// </summary>
	private void HandleEventError(EventType eventType, GameEventArgs args, Exception ex)
	{
		GD.PrintErr($"Error processing event {eventType}: {ex.Message}");
		QueueEvent(EventType.None, new ErrorEventArgs
		{
			OriginalEventType = eventType,
			OriginalEventArgs = args,
			Error = ex
		});
	}

	/// <summary>
	/// System event handler for internal events
	/// </summary>
	private void OnSystemEvent(GameEventArgs args)
	{
		// Handle system-level events
		if (args is ErrorEventArgs errorArgs)
		{
			// Handle error events
			GD.PrintErr($"System Error: {errorArgs.Error.Message}");
		}
	}

}
	
