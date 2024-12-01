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

		
		// Initialize manager container
		gameMangers = new ManagerContainer();
		gameMangers.Setup();

		

		if (_eventSystemTests != null)
		{
			_eventSystemTests.Setup();
		}
	}
}

//public partial class GameManager : Control, IManager
//{

//	// Called when the node enters the scene tree for the first time.
//	public ManagerContainer gameMangers;
//	//this is the parent node.. this is is used to reverence all ui elements below this point
//	public Node RootNode;

//	[Export]
//	public GameSystemTests _EventSystemTests;
//	public override void _Ready()
//	{
//		Setup();
//	}

//	// Called every frame. 'delta' is the elapsed time since the previous frame.
//	public override void _Process(double delta)
//	{
//	}

//	public void Setup()
//	{
//		//set the instance to this class. 
//		Globals.Instance = this;
//		//set the rootnode to this object
//		RootNode = this;
//		//get started on setting up the various managers
//		gameMangers = new ManagerContainer();

//		gameMangers.Setup();


//		_EventSystemTests.Setup();
//	}
//}
