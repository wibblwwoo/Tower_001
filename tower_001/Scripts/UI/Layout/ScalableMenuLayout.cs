using Godot;
using System.ComponentModel;
using static Tower_001.Scripts.GameLogic.Balance.GameBalanceConfig.CharacterStats;

namespace Tower_001.Scripts.UI.Layout
{
	public partial class ScalableMenuLayout : Control
	{
		// Fields for the controls we need to access later
		private Panel _leftTopPanel;
		private Panel _leftMiddlePanel;
		private CenterContainer _leftMiddleCenter;
		private Panel _leftBottomPanel;

		private Panel _rightTopPanel;
		private Panel _rightMiddlePanel;
		private Panel _rightBottomPanel;

		private void BuildLeftPanelHierarchy(Control leftControl)
		{
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

			_leftTopPanel = new Panel
			{
				Name = "LeftTopPanel",
				CustomMinimumSize = new Vector2(0, 40)
			};

			_leftMiddlePanel = new Panel
			{
				Name = "LeftMiddlePanel",
				CustomMinimumSize = new Vector2(0, 80),
				SizeFlagsVertical = Control.SizeFlags.Expand
			};

			_leftMiddleCenter = new CenterContainer { Name = "LeftMiddleCenterContainer" };
			_leftMiddleCenter.SetAnchorsPreset(Control.LayoutPreset.FullRect);

			_leftBottomPanel = new Panel
			{
				Name = "LeftBottomPanel",
				CustomMinimumSize = new Vector2(0, 40),
				ClipContents = true
			};

			var leftBottomCenter = new CenterContainer { Name = "LeftBottomCenterContainer" };

			// Build the hierarchy
			_leftMiddlePanel.AddChild(_leftMiddleCenter);

			leftVBox.AddChild(_leftTopPanel);
			leftVBox.AddChild(_leftMiddlePanel);
			leftVBox.AddChild(_leftBottomPanel);
			leftVBox.AddChild(leftBottomCenter);

			leftContentPanel.AddChild(leftVBox);
			leftHBox.AddChild(leftContentPanel);
			leftMainPanel.AddChild(leftHBox);
			leftMarginContainer.AddChild(leftMainPanel);
			leftControl.AddChild(leftMarginContainer);
		}

		private void BuildRightPanelHierarchy(Control gameAreaControl)
		{
			var gameMarginContainer = new MarginContainer { Name = "GameMarginContainer" };
			gameMarginContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);

			var gameMainPanel = new Panel
			{
				Name = "GameMainPanel",
				CustomMinimumSize = new Vector2(150, 0)
			};

			var gameHBox = new HBoxContainer { Name = "GameHBoxContainer" };
			gameHBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);

			var gameContentPanel = new Panel
			{
				Name = "GameContentPanel",
				CustomMinimumSize = new Vector2(150, 0),
				SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
			};

			var gameVBox = new VBoxContainer { Name = "GameVBoxContainer" };
			gameVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);

			_rightTopPanel = new Panel
			{
				Name = "GameTopPanel",
				CustomMinimumSize = new Vector2(0, 40)
			};

			_rightMiddlePanel = new Panel
			{
				Name = "GameMiddlePanel",
				CustomMinimumSize = new Vector2(0, 80),
				SizeFlagsVertical = Control.SizeFlags.Expand
			};

			var gameMiddleCenter = new CenterContainer { Name = "GameMiddleCenterContainer" };
			gameMiddleCenter.SetAnchorsPreset(Control.LayoutPreset.FullRect);

			_rightBottomPanel = new Panel
			{
				Name = "GameBottomPanel",
				CustomMinimumSize = new Vector2(0, 40),
				ClipContents = true
			};

			var gameBottomCenter = new CenterContainer { Name = "GameBottomCenterContainer" };

			// Build the hierarchy
			_rightMiddlePanel.AddChild(gameMiddleCenter);

			gameVBox.AddChild(_rightTopPanel);
			gameVBox.AddChild(_rightMiddlePanel);
			gameVBox.AddChild(_rightBottomPanel);
			gameVBox.AddChild(gameBottomCenter);

			gameContentPanel.AddChild(gameVBox);
			gameHBox.AddChild(gameContentPanel);
			gameMainPanel.AddChild(gameHBox);
			gameMarginContainer.AddChild(gameMainPanel);
			gameAreaControl.AddChild(gameMarginContainer);
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

			var leftControl = new Control
			{
				Name = "LeftSideControl",
				AnchorLeft = 0f,
				AnchorRight = 0.13f,
				AnchorBottom = 1.0f,
				GrowHorizontal = Control.GrowDirection.Both,
				GrowVertical = Control.GrowDirection.Both
			};

			BuildRightPanelHierarchy(gameAreaControl);
			BuildLeftPanelHierarchy(leftControl);

			Globals.Instance.RootNode.AddChild(gameAreaControl);
			Globals.Instance.RootNode.AddChild(leftControl);
		}

		public override void _Ready()
		{
			CreateLayout();
		}

		public void AddToLeftPanelTop(Control node)
		{
			_leftTopPanel.AddChild(node);
		}

		public void AddToLeftPanelMiddle(Control node)
		{
			_leftMiddleCenter.AddChild(node);
		}

		public void AddToLeftPanelBottom(Control node)
		{
			node.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			node.GrowHorizontal = Control.GrowDirection.Both;
			node.GrowVertical = Control.GrowDirection.Both;
			_leftBottomPanel.AddChild(node);
		}

		public void AddToRightPanelTop(Control node)
		{
			_rightTopPanel.AddChild(node);
		}

		public void AddToRightPanelMiddle(Control node)
		{
			_rightMiddlePanel.AddChild(node);
		}

		public void AddToRightPanelBottom(Control node)
		{
			_rightBottomPanel.AddChild(node);
		}
	
	public virtual void ClearLeftPanel()
        {
            if (_leftTopPanel == null) return;
            foreach (Node child in _leftTopPanel.GetChildren())
            {
                child.QueueFree();
            }

            if (_leftMiddleCenter == null) return;
            foreach (Node child in _leftMiddleCenter.GetChildren())
            {
                child.QueueFree();
            }
            if (_leftBottomPanel == null) return;
            foreach (Node child in _leftBottomPanel.GetChildren())
            {
                child.QueueFree();
            }
        }

        public virtual void ClearRightPanel()
        {
            if (_rightTopPanel != null)
            {
                foreach (Node child in _rightTopPanel.GetChildren())
                {
                    child.QueueFree();
                }
            }
        }

        //public override void _Process(double delta)
        //{
        //    if (_leftPanel != null)
        //    {
        //        _leftPanel.Size = Size;
        //    }
        //}

        public virtual void Show()
        {
            Visible = true;
        }

        public virtual void Hide()
        {
            Visible = false;
        }
    }
}
