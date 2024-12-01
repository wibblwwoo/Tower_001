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

        public static void ApplyPanelStyle(Panel panel)
        {
            var stylebox = new StyleBoxFlat();
            stylebox.BorderWidthTop = 1;
            stylebox.BorderWidthRight = 1;
            stylebox.BorderWidthBottom = 1;
            stylebox.BorderWidthLeft = 1;
            stylebox.BorderColor = new Color(0.7f, 0.7f, 0.7f); // Light gray
            stylebox.BgColor = new Color(0, 0, 0, 0); // Transparent background
            
            panel.AddThemeStyleboxOverride("panel", stylebox);
        }

        //private void BuildLeftPanelHierarchy(Control leftPanel)
        //{
        //    var leftHBox = new HBoxContainer 
        //    { 
        //        Name = "LeftHBoxContainer",
        //        LayoutMode = 1,
        //        AnchorsPreset = (int)Control.LayoutPreset.FullRect,  // 15
        //        AnchorRight = 1.0f,
        //        AnchorBottom = 1.0f,
        //        GrowHorizontal = Control.GrowDirection.Both,
        //        GrowVertical = Control.GrowDirection.Both,
        //        SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
        //    };

        //    var leftContentPanel = new Panel
        //    {
        //        Name = "LeftContentPanel",
        //        CustomMinimumSize = new Vector2(150, 0),
        //        SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
        //    };
        //    ApplyPanelStyle(leftContentPanel);

        //    var leftVBox = new VBoxContainer 
        //    { 
        //        Name = "LeftVBoxContainer",
        //        LayoutMode = 1,
        //        AnchorsPreset = (int)Control.LayoutPreset.FullRect,  // 15
        //        AnchorRight = 1.0f,
        //        AnchorBottom = 1.0f,
        //        GrowHorizontal = Control.GrowDirection.Both,
        //        GrowVertical = Control.GrowDirection.Both,
        //        SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
        //    };

        //    _leftTopPanel = new Panel
        //    {
        //        Name = "LeftTopPanel",
        //        CustomMinimumSize = new Vector2(0, 40),
        //        SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
        //    };
        //    ApplyPanelStyle(_leftTopPanel);

        //    _leftMiddlePanel = new Panel
        //    {
        //        Name = "LeftMiddlePanel",
        //        CustomMinimumSize = new Vector2(0, 80),
        //        SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand,
        //        SizeFlagsVertical = Control.SizeFlags.Expand,
        //        ClipContents = false
        //    };
        //    ApplyPanelStyle(_leftMiddlePanel);

        //    _leftBottomPanel = new Panel
        //    {
        //        Name = "LeftBottomPanel",
        //        CustomMinimumSize = new Vector2(0, 40),
        //        SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand,
        //        ClipContents = false  // Prevent clipping at bottom
        //    };
        //    ApplyPanelStyle(_leftBottomPanel);

        //    var leftBottomCenter = new CenterContainer
        //    {
        //        Name = "LeftBottomCenterContainer",
        //        SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand,
        //        SizeFlagsVertical = Control.SizeFlags.Fill | Control.SizeFlags.Expand
        //    };

        //    _leftMiddleCenter = new CenterContainer
        //    {
        //        Name = "LeftMiddleCenterContainer",
        //        LayoutMode = 1,
        //        AnchorsPreset = (int)Control.LayoutPreset.FullRect,  // 15
        //        AnchorRight = 1.0f,
        //        AnchorBottom = 1.0f,
        //        GrowHorizontal = Control.GrowDirection.Both,
        //        GrowVertical = Control.GrowDirection.Both,
        //        SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand,
        //        SizeFlagsVertical = Control.SizeFlags.Fill | Control.SizeFlags.Expand
        //    };

        //    // Build the hierarchy
        //    _leftMiddlePanel.AddChild(_leftMiddleCenter);

        //    leftVBox.AddChild(_leftTopPanel);
        //    leftVBox.AddChild(_leftMiddlePanel);
        //    leftVBox.AddChild(_leftBottomPanel);
        //    leftVBox.AddChild(leftBottomCenter);

        //    leftContentPanel.AddChild(leftVBox);
        //    leftHBox.AddChild(leftContentPanel);
        //    leftPanel.AddChild(leftHBox);
        //}

        private void BuildRightPanelHierarchy(Control gameAreaControl)
        {
            //var gameMarginContainer = new MarginContainer 
            //{ 
            //    Name = "GameMarginContainer",
            //    CustomMinimumSize = new Vector2(154, 0),
            //    SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
            //};
            //gameMarginContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);

            //var gameMainPanel = new Panel
            //{
            //    Name = "GameMainPanel",
            //    CustomMinimumSize = new Vector2(154, 0),
            //    SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
            //};
            //ApplyPanelStyle(gameMainPanel);

            //var gameHBox = new HBoxContainer 
            //{ 
            //    Name = "GameHBoxContainer",
            //    SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
            //};
            //gameHBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);

            //var gameContentPanel = new Panel
            //{
            //    Name = "GameContentPanel",
            //    CustomMinimumSize = new Vector2(154, 0),
            //    SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
            //};
            //ApplyPanelStyle(gameContentPanel);

            //var gameVBox = new VBoxContainer 
            //{ 
            //    Name = "GameVBoxContainer",
            //    SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
            //};
            //gameVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);

            //_rightTopPanel = new Panel
            //{
            //    Name = "GameTopPanel",
            //    CustomMinimumSize = new Vector2(154, 40),
            //    SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
            //};
            //ApplyPanelStyle(_rightTopPanel);

            //_rightMiddlePanel = new Panel
            //{
            //    Name = "GameMiddlePanel",
            //    CustomMinimumSize = new Vector2(154, 80),
            //    SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand,
            //    SizeFlagsVertical = Control.SizeFlags.Expand
            //};
            //ApplyPanelStyle(_rightMiddlePanel);

            //_rightBottomPanel = new Panel
            //{
            //    Name = "GameBottomPanel",
            //    CustomMinimumSize = new Vector2(154, 40),
            //    SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand,
            //    ClipContents = true
            //};
            //ApplyPanelStyle(_rightBottomPanel);

            //var gameMiddleCenter = new CenterContainer 
            //{ 
            //    Name = "GameMiddleCenterContainer",
            //    SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
            //};
            //gameMiddleCenter.SetAnchorsPreset(Control.LayoutPreset.FullRect);

            //var gameBottomCenter = new CenterContainer 
            //{ 
            //    Name = "GameBottomCenterContainer",
            //    SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
            //};

            //// Build the hierarchy
            //_rightMiddlePanel.AddChild(gameMiddleCenter);

            //gameVBox.AddChild(_rightTopPanel);
            //gameVBox.AddChild(_rightMiddlePanel);
            //gameVBox.AddChild(_rightBottomPanel);
            //gameVBox.AddChild(gameBottomCenter);

            //gameContentPanel.AddChild(gameVBox);
            //gameHBox.AddChild(gameContentPanel);
            //gameMainPanel.AddChild(gameHBox);
            //gameMarginContainer.AddChild(gameMainPanel);
            //gameAreaControl.AddChild(gameMarginContainer);
        }

        private void CreateLayout()
        {
            //// Create main control that will contain both left and game area
            //var mainControl = new Control
            //{
            //    Name = "MainControl",
            //    LayoutMode = 1,
            //    AnchorsPreset = (int)Control.LayoutPreset.FullRect,  // 15
            //    AnchorRight = 1.0f,
            //    AnchorBottom = 1.0f,
            //    GrowHorizontal = Control.GrowDirection.Both,
            //    GrowVertical = Control.GrowDirection.Both
            //};

            //// Create left control as child of main control with negative anchor
            //var leftControl = new Control
            //{
            //    Name = "LeftControl",
            //    LayoutMode = 1,
            //    AnchorLeft = -0.148f,
            //    AnchorRight = 0.002f,
            //    AnchorBottom = 1.0f,
            //    OffsetLeft = 0.296005f,
            //    OffsetRight = -0.00400019f,
            //    GrowHorizontal = Control.GrowDirection.Both,
            //    GrowVertical = Control.GrowDirection.Both
            //};

            //var leftMarginContainer = new MarginContainer
            //{
            //    Name = "LeftMarginContainer",
            //    LayoutMode = 1,
            //    AnchorsPreset = (int)Control.LayoutPreset.FullRect,  // 15
            //    AnchorRight = 1.0f,
            //    AnchorBottom = 1.0f,
            //    GrowHorizontal = Control.GrowDirection.Both,
            //    GrowVertical = Control.GrowDirection.Both,
            //    CustomMinimumSize = new Vector2(150, 0)
            //};

            //var leftPanel = new Panel
            //{
            //    Name = "LeftMainPanel",
            //    CustomMinimumSize = new Vector2(150, 0),
            //    SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand
            //};

            //// Create game area control as child of main control
            //var gameAreaControl = new Control
            //{
            //    Name = "GameAreaControl",
            //    LayoutMode = 1,
            //    AnchorLeft = 0f,
            //    AnchorRight = 1.0f,
            //    AnchorBottom = 1.0f,
            //    GrowHorizontal = Control.GrowDirection.Both,
            //    GrowVertical = Control.GrowDirection.Both
            //};

            //// Build the hierarchies
            //BuildRightPanelHierarchy(gameAreaControl);
            
            //// Build left panel hierarchy with exact TSCN values
            //leftControl.AddChild(leftMarginContainer);
            //leftMarginContainer.AddChild(leftPanel);
            //BuildLeftPanelHierarchy(leftPanel);

            //// Add controls to main control
            //mainControl.AddChild(leftControl);
            //mainControl.AddChild(gameAreaControl);

            //// Add main control to this node
            //AddChild(mainControl);
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

        public virtual void Show()
        {
            GD.Print($"ScalableMenuLayout: Showing");
            Visible = true;
        }

        public virtual void Hide()
        {
            GD.Print($"ScalableMenuLayout: Hiding");
            Visible = false;
        }
    }
}
