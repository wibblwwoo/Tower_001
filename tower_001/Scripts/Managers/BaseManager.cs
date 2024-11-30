
/// <summary>
/// Base class for all managers that need event handling
/// </summary>
/// <summary>
/// Base class for all managers that need event handling
/// </summary>
public abstract class BaseManager : IManager
{
	protected EventManager EventManager;

	public virtual void Setup()
	{
		// Get EventManager reference after it's been initialized
		EventManager = Globals.Instance.gameMangers.Events;
		RegisterEventHandlers();
	}

	protected abstract void RegisterEventHandlers();
}