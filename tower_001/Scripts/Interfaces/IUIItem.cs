using Godot;
using System;

public interface IUIItem 
{

	[Export]
	bool IsEnabled { get; set; }

	[Export]
	bool IsVisibleonStartup { get; set; }


	/// <summary>
	/// does this item need any setup
	/// </summary>
	void Setup();
	/// <summary>
	/// Event has fired that has called this UI Item
	/// </summary>
	void EventFire();
	/// <summary>
	/// data has been update. so refresh the item
	/// </summary>
	void Refresh();

	/// <summary>
	/// Is this control Visible
	/// </summary>
	/// <param name="visible"></param>
	void Visiblity(bool visible);

	/// <summary>
	/// We need to call an event, call this function
	/// </summary>
	void CallEvent();
}
