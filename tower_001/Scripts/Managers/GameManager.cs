using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static System.Formats.Asn1.AsnWriter;


public static class Globals
{
	public static GameManager Instance;
}

public partial class GameManager : Control, IManager
{

	public Node RootNode;
	public ManagerContainer gameMangers;

	[Export]
	public GameSystemTests _eventSystemTests;

	public override void _Ready()
	{
		Setup();
	}

	public void Setup()
	{
		Globals.Instance = this;
		//set the rootnode to this object
		RootNode = this;


		if (Globals.Instance.RootNode != null)
		{

		}

		//CreateLayout();

		// Initialize manager container
		gameMangers = new ManagerContainer();
		gameMangers.Setup();


		if (_eventSystemTests != null)
		{
			_eventSystemTests.Setup();
		}
	}



}

