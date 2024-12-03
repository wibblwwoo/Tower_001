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

	[Export]
	public MenuPanelContainer MenuPanelContainer_Left;
	[Export]
	public MenuPanelContainer MenuPanelContainer_Right;
	public ManagerContainer gameMangers;


	[Export]
	public GameSystemTests _eventSystemTests;

	[Export]
	public PackedScene PanelType_Single;
	[Export]
	public PackedScene PanelType_Dual_Horizontal;
	[Export]
	public PackedScene PanelType_Multi_Horizontal;
	[Export]
	public PackedScene PanelType_Dual_Vertical;
	[Export]
	public PackedScene PanelType_Multi_Vertical;

	private DynamicPanelController _panelController;
	private PanelMenuIntegration _panelMenuIntegration;

	// Add properties for new panel system
	public DynamicPanelController PanelController => _panelController;
	public PanelMenuIntegration PanelMenuIntegration => _panelMenuIntegration;

	public override void _Ready()
	{
		Setup();
	}

	public void Setup()
	{
		Globals.Instance = this;
		//set the rootnode to this object
		RootNode = this;

		_panelController = new DynamicPanelController();
		_panelMenuIntegration = new PanelMenuIntegration();

		_panelController.RegisterContainer("LeftPanel", MenuPanelContainer_Left.ParentPanel);
		_panelController.RegisterContainer("RightPanel", MenuPanelContainer_Right.ParentPanel);

		// Initialize manager container
		gameMangers = new ManagerContainer();
		gameMangers.Setup();


		if (_eventSystemTests != null)
		{
			_eventSystemTests.Setup();
		}

		PrintNodeHierarchy(RootNode);
	}


	private void PrintNodeHierarchy(Node node, string indent = "")
	{
		GD.Print($"{indent}{node.Name} ({node.GetType()})");
		foreach (var child in node.GetChildren())
		{
			PrintNodeHierarchy(child, indent + "  ");
		}
	}

	private void CreateLayout()
	{

		var gameAreaControl = new Control
		{
			Name = "GameAreaControl",
			AnchorLeft = 0.13f,
			AnchorRight = 1.0f,
			AnchorBottom = 1.0f,
			OffsetLeft = 0.23999f,
			GrowHorizontal = Control.GrowDirection.Both,
			GrowVertical = Control.GrowDirection.Both
		};

		// Let's modify the left control settings

		// Margin container for game area
		var gameMarginContainer = new MarginContainer
		{
			Name = "GameMarginContainer"
		};
		gameMarginContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);

		// Main panel for game area
		var gameMainPanel = new Panel
		{
			Name = "GameMainPanel",
			CustomMinimumSize = new Vector2(150, 0)
		};

		// Game HBox
		var gameHBox = new HBoxContainer
		{
			Name = "GameHBoxContainer"
		};
		gameHBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);

		// Game content panel
		var gameContentPanel = new Panel
		{
			Name = "GameContentPanel",
			CustomMinimumSize = new Vector2(150, 0),
			SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
		};

		// Game VBox container
		var gameVBox = new VBoxContainer
		{
			Name = "GameVBoxContainer"
		};
		gameVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);

		// Game Top Panel
		var gameTopPanel = new Panel
		{
			Name = "GameTopPanel",
			CustomMinimumSize = new Vector2(0, 40)
		};

		// Game Middle Panel
		var gameMiddlePanel = new Panel
		{
			Name = "GameMiddlePanel",
			CustomMinimumSize = new Vector2(0, 80),
			SizeFlagsVertical = Control.SizeFlags.Expand
		};

		// Game Middle Center Container
		var gameMiddleCenter = new CenterContainer
		{
			Name = "GameMiddleCenterContainer"
		};
		gameMiddleCenter.SetAnchorsPreset(Control.LayoutPreset.FullRect);

		// Game Bottom Panel
		var gameBottomPanel = new Panel
		{
			Name = "GameBottomPanel",
			CustomMinimumSize = new Vector2(0, 40),
			ClipContents = true
		};

		// Game Bottom Button
		var gameBottomButton = new Button
		{
			Text = "Game Bottom midddle"
		};
		gameBottomButton.SetAnchorsPreset(Control.LayoutPreset.FullRect);

		// Game Bottom Center Container
		var gameBottomCenter = new CenterContainer
		{
			Name = "GameBottomCenterContainer"
		};


		var gameTopButton = new Button
		{
			Text = "Game Top midddle"
		};
		gameTopButton.SetAnchorsPreset(Control.LayoutPreset.FullRect);


		// Build the hierarchy for game area
		gameBottomPanel.AddChild(gameBottomButton);
		gameMiddlePanel.AddChild(gameMiddleCenter);
		gameTopPanel.AddChild(gameTopButton);

		gameVBox.AddChild(gameTopPanel);
		gameVBox.AddChild(gameMiddlePanel);
		gameVBox.AddChild(gameBottomPanel);
		gameVBox.AddChild(gameBottomCenter);

		gameContentPanel.AddChild(gameVBox);
		gameHBox.AddChild(gameContentPanel);
		gameMainPanel.AddChild(gameHBox);
		gameMarginContainer.AddChild(gameMainPanel);
		gameAreaControl.AddChild(gameMarginContainer);

		var leftControl = new Control
		{
			Name = "LeftSideControl",
			// Change these values to position it correctly
			AnchorLeft = 0f,  // Start from left edge
			AnchorRight = 0.13f,  // Take up 13% of width (matching where game area starts)
			AnchorBottom = 1.0f,
			GrowHorizontal = Control.GrowDirection.Both,
			GrowVertical = Control.GrowDirection.Both
		};
		// Main game area control (the middle section)

		// Create the left side with the same structure as game area
		var leftMarginContainer = new MarginContainer { Name = "LeftMarginContainer" };
		leftMarginContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);

		var leftMainPanel = new Panel
		{
			Name = "LeftMainPanel",
			CustomMinimumSize = new Vector2(150, 0)
		};

		var leftHBox = new HBoxContainer { Name = "LeftHBoxContainer" };
		leftHBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);

		var leftContentPanel = new Panel
		{
			Name = "LeftContentPanel",
			CustomMinimumSize = new Vector2(150, 0),
			SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
		};

		var leftVBox = new VBoxContainer { Name = "LeftVBoxContainer" };
		leftVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);

		// Build left side panels similar to game area
		var leftTopPanel = new Panel
		{
			Name = "LeftTopPanel",
			CustomMinimumSize = new Vector2(0, 40)
		};

		var leftMiddlePanel = new Panel
		{
			Name = "LeftMiddlePanel",
			CustomMinimumSize = new Vector2(0, 80),
			SizeFlagsVertical = Control.SizeFlags.Expand
		};

		var leftMiddleCenter = new CenterContainer { Name = "LeftMiddleCenterContainer" };
		leftMiddleCenter.SetAnchorsPreset(Control.LayoutPreset.FullRect);

		var leftBottomPanel = new Panel
		{
			Name = "LeftBottomPanel",
			CustomMinimumSize = new Vector2(0, 40),
			ClipContents = true
		};

		var leftBottomButton = new Button { Text = "Left Bottom" };
		leftBottomButton.SetAnchorsPreset(Control.LayoutPreset.FullRect);

		var leftBottomCenter = new CenterContainer { Name = "LeftBottomCenterContainer" };


		var LeftMiddlButton = new Button
		{
			Text = "Menu Left midddle"
		};
		LeftMiddlButton.SetAnchorsPreset(Control.LayoutPreset.FullRect);


		leftMiddleCenter.AddChild(LeftMiddlButton);
		
		// Build the hierarchy for left side
		leftBottomPanel.AddChild(leftBottomButton);
		leftMiddlePanel.AddChild(leftMiddleCenter);

		leftVBox.AddChild(leftTopPanel);
		leftVBox.AddChild(leftMiddlePanel);
		leftVBox.AddChild(leftBottomPanel);
		leftVBox.AddChild(leftBottomCenter);

		leftContentPanel.AddChild(leftVBox);
		leftHBox.AddChild(leftContentPanel);
		leftMainPanel.AddChild(leftHBox);
		leftMarginContainer.AddChild(leftMainPanel);
		leftControl.AddChild(leftMarginContainer);



		// Add everything to the root
		RootNode.AddChild(gameAreaControl);
		RootNode.AddChild(leftControl);
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
