
using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Base class for all managers that need event handling
/// </summary>
/// <summary>
/// Base class for all managers that need event handling
/// </summary>
public abstract class BaseManager : IManager
{
	protected EventManager EventManager;

	public virtual IEnumerable<Type> Dependencies => new[] { typeof(EventManager) };
	protected ManagerState CurrentState { get; private set; } = ManagerState.Uninitialized;
	public virtual void Setup()
	{
		if (!TryTransitionTo(ManagerState.Initializing))
			return;

		try
		{
			// Get EventManager reference after it's been initialized
			EventManager = Globals.Instance.gameMangers.Events;
			RegisterEventHandlers();
			TryTransitionTo(ManagerState.Ready);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Setup failed for {GetType().Name}: {ex.Message}");
			TryTransitionTo(ManagerState.Error);
			throw;
		}


	}
	protected abstract void RegisterEventHandlers();

	// Track allowed state transitions
	private static readonly Dictionary<ManagerState, HashSet<ManagerState>> AllowedTransitions = new()
	{
		{
			ManagerState.Uninitialized, new HashSet<ManagerState>
			{
				ManagerState.Initializing,
				ManagerState.Error,
				ManagerState.Disabled
			}
		},
		{
			ManagerState.Initializing, new HashSet<ManagerState>
			{
				ManagerState.Ready,
				ManagerState.Error,
				ManagerState.Disabled
			}
		},
		{
			ManagerState.Ready, new HashSet<ManagerState>
			{
				ManagerState.ShuttingDown,
				ManagerState.Error,
				ManagerState.Disabled
			}
		},
		{
			ManagerState.Error, new HashSet<ManagerState>
			{
				ManagerState.Initializing,
				ManagerState.Disabled
			}
		},
		{
			ManagerState.ShuttingDown, new HashSet<ManagerState>
			{
				ManagerState.Uninitialized,
				ManagerState.Error,
				ManagerState.Disabled
			}
		},
		{
			ManagerState.Disabled, new HashSet<ManagerState>
			{
				ManagerState.Initializing
			}
		}
	};

	protected bool TryTransitionTo(ManagerState newState)
	{
		if (!IsTransitionAllowed(newState))
		{
			GD.PrintErr($"Invalid state transition from {CurrentState} to {newState} in {GetType().Name}");
			return false;
		}

		var oldState = CurrentState;
		CurrentState = newState;

		OnStateChanged(oldState, newState);
		return true;
	}

	private bool IsTransitionAllowed(ManagerState newState)
	{
		return AllowedTransitions.ContainsKey(CurrentState) &&
			   AllowedTransitions[CurrentState].Contains(newState);
	}

	protected virtual void OnStateChanged(ManagerState oldState, ManagerState newState)
	{
		DebugLogger.Log($"Manager {GetType().Name} state changed from {oldState} to {newState}", DebugLogger.LogCategory.UI_States);
		// Derived classes can override to handle state changes
	}

	//protected void HandleManagerError(Exception ex, ManagerErrorType errorType, bool transitionToError = true)
	//{
	//	var managerError = new ManagerException(
	//		ex.Message,
	//		GetType().Name,
	//		CurrentState,
	//		errorType,
	//		ex
	//	);

	//	_errorHistory.Add((managerError, DateTime.UtcNow));
	//	if (_errorHistory.Count > MAX_ERROR_HISTORY)
	//		_errorHistory.RemoveAt(0);

	//	// Notify via event system
	//	if (EventManager != null)
	//	{
	//		var errorArgs = new ManagerErrorEventArgs(GetType(), errorType, managerError);
	//		EventManager.RaiseEvent(EventType.System_ManagerError, errorArgs);
	//	}

	//	if (transitionToError)
	//		TryTransitionTo(ManagerState.Error);

	//	GD.PrintErr($"Manager Error [{errorType}] in {GetType().Name}: {ex.Message}");
	//}
}