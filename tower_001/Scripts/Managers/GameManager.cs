using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Tower_001.Scripts.UI.Layout;
using static System.Formats.Asn1.AsnWriter;


public static class Globals
{
	public static GameManager Instance;

}

public partial class GameManager : Control, IManager
{

	public Node RootNode;

	public ManagerContainer gameMangers;

	public IEnumerable<Type> Dependencies { get; }

	[Export]
	public GameSystemTests _eventSystemTests;

	[Export]
	public StatSystemTest _statsTest;
	public override void _Ready()
	{
		Setup();
	}

	public void Setup()
	{
		Globals.Instance = this;
		//set the rootnode to this object
		RootNode = this;


		// Initialize manager container
		gameMangers = new ManagerContainer();
		gameMangers.Setup();


		if (_eventSystemTests != null)
		{
			_eventSystemTests.Setup();
		}


		_statsTest = new StatSystemTest();
		_statsTest.Run();

	}


	public void PrintNodeHierarchy(Node node, string indent = "")
	{
		string nodeInfo = $"{indent}{node.Name} ({node.GetType()})";

		// Add control-specific information if this is a Control node
		if (node is Control control)
		{
			nodeInfo += "\n" + indent + "  └─ Control Properties:";
			nodeInfo += $"\n{indent}     ├─ MouseFilter: {control.MouseFilter}";
			nodeInfo += $"\n{indent}     ├─ MouseDefaultCursorShape: {control.MouseDefaultCursorShape}";
			nodeInfo += $"\n{indent}     ├─ FocusMode: {control.FocusMode}";
			nodeInfo += $"\n{indent}     ├─ ProcessMode: {control.ProcessMode}";
			nodeInfo += $"\n{indent}     ├─ Visible: {control.Visible}";
			nodeInfo += $"\n{indent}     └─ Size: {control.Size}";
			nodeInfo += $"\n{indent}     └─ GrowHorizontal: {control.GrowHorizontal}";
			nodeInfo += $"\n{indent}     └─ GrowVertical: {control.GrowVertical}";
			nodeInfo += $"\n{indent}     └─ Position: {control.Position}";
		}

		// Add button-specific information if this is a Button
		if (node is Button button)
		{
			nodeInfo += "\n" + indent + "  └─ Button Properties:";
			nodeInfo += $"\n{indent}     ├─ Disabled: {button.Disabled}";
			nodeInfo += $"\n{indent}     ├─ ToggleMode: {button.ToggleMode}";
			nodeInfo += $"\n{indent}     └─ ButtonPressed: {button.ButtonPressed}";
		}

		GD.Print(nodeInfo);

		foreach (var child in node.GetChildren())
		{
			PrintNodeHierarchy(child, indent + "  ");
		}
	}

	
}

