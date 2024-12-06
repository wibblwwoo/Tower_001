using Godot;
using System;

public interface IMenu
{
	string menuParentID { get; set; }

	bool ShowOnStartup { get; set; }

	void Setup();
	void Show();
	void Hide();
}
