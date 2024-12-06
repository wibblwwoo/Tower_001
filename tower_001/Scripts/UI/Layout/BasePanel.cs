using Godot;
using System;

public abstract partial class BasePanel : Control, IPanel
{
	protected string _panelId;
	protected bool _isInitialized;
	protected bool _isVisible;

	public string PanelId => _panelId;
	public bool IsVisible => _isVisible;
	protected BasePanel()
	{
		_isInitialized = false;
		_isVisible = false;
	}

	public void SetPanelID(string panelID)
	{
		_panelId = panelID;
	}

	protected BasePanel(string panelId)
	{
		_panelId = panelId;
		_isInitialized = false;
		_isVisible = false;
	}

	public virtual void Initialize()
	{
		if (_isInitialized) return;


		_isInitialized = true;
	}

	public virtual void Show()
	{
		if (!_isInitialized)
		{
			Initialize();
		}

		Visible = true;
		_isVisible = true;
		EnableEvents();
	}

	public virtual void Hide()
	{
		Visible = false;
		_isVisible = false;
		DisableEvents();
	}

	public virtual void Cleanup()
	{
		DisableEvents();
		QueueFree();
	}

	protected virtual void EnableEvents()
	{
		// Override in derived classes to enable specific events
	}

	protected virtual void DisableEvents()
	{
		// Override in derived classes to disable specific events
	}
}
