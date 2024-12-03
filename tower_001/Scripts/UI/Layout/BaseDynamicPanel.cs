using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

// Abstract base class for all panel types
public abstract partial class BaseDynamicPanel : Control, IPanelEventHandler
{
	protected PanelConfig _config;
	protected bool _isInitialized;
	protected bool _isVisible;
	protected Dictionary<string, Node> _contentNodes;
	protected Dictionary<string, Control> _sections;

	public string PanelId => _config.PanelId;
	public PanelType Type => _config.Type;
	public bool IsVisible => _isVisible;

	protected BaseDynamicPanel(PanelConfig config)
	{
		_config = config;
		_isInitialized = false;
		_isVisible = false;
		_contentNodes = new Dictionary<string, Node>();
		_sections = new Dictionary<string, Control>();
	}

	public virtual void Initialize()
	{
		if (_isInitialized) return;

		LoadPanelScene();
		SetupSections();
		RegisterEvents();

		_isInitialized = true;
	}

	protected abstract void LoadPanelScene();
	protected abstract void SetupSections();

	protected virtual void RegisterEvents()
	{
		// Override in derived classes to register specific events
	}

	protected virtual void UnregisterEvents()
	{
		// Override in derived classes to unregister specific events
	}

	public virtual void Show()
	{
		if (!_isInitialized)
			Initialize();

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
		UnregisterEvents();
		DisableEvents();
		ClearContent();
		QueueFree();
	}

	protected virtual void EnableEvents()
	{
		// Override in derived panels
	}

	protected virtual void DisableEvents()
	{
		// Override in derived panels
	}

	public virtual void HandlePanelEvent(string eventName, object data)
	{
		if (!_isVisible) return;
		// Override in derived panels for specific event handling
	}

	public void AddContent(string sectionId, Control content)
	{
		if (!_sections.ContainsKey(sectionId))
		{
			GD.PrintErr($"Section {sectionId} not found in panel {PanelId}");
			return;
		}

		_sections[sectionId].AddChild(content);
		_contentNodes[content.Name] = content;
	}

	public void RemoveContent(string contentName)
	{
		if (_contentNodes.TryGetValue(contentName, out var node))
		{
			node.QueueFree();
			_contentNodes.Remove(contentName);
		}
	}

	public void ClearContent()
	{
		foreach (var node in _contentNodes.Values)
		{
			node.QueueFree();
		}
		_contentNodes.Clear();
	}

	protected void RegisterSection(string sectionId, Control section)
	{
		if (!_sections.ContainsKey(sectionId))
		{
			_sections.Add(sectionId, section);
		}
	}
}