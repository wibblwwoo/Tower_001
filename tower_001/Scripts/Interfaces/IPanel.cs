using Godot;
using System;

public interface IPanel
{
	void Initialize();
	void Show();
	void Hide();
	void Cleanup();
	bool IsVisible { get; }
	string PanelId { get; }
}
