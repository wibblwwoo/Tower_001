using Godot;

namespace Tower_001.Scripts.UI.Layout
{
    public partial class TestProgrammaticLayout : Control
    {
        private HBoxContainer _leftPanel;
        private Panel _topPanel;
        private Panel _middlePanel;
        private CenterContainer _middleCenterContainer;
        private Panel _bottomPanel;

        public override void _Ready()
        {
            // Set up the root control (this node)
            AnchorsPreset = (int)LayoutPreset.FullRect;
            GrowHorizontal = GrowDirection.Both;
            GrowVertical = GrowDirection.Both;

            CreateLeftPanel();
        }

        private void CreateLeftPanel()
        {
            // Create main HBoxContainer for left panel
            _leftPanel = new HBoxContainer
            {
                Name = "HBoxContainer_Left_Panel",
                LayoutMode = 1,
                AnchorsPreset = (int)LayoutPreset.LeftWide,
                GrowVertical = GrowDirection.Both
            };
            
            // Create the main panel inside HBoxContainer
            var mainPanel = new Panel
            {
                Name = "Panel",
                CustomMinimumSize = new Vector2(150, 0), // Fixed width of 150
                LayoutMode = 1,
                AnchorsPreset = (int)LayoutPreset.FullRect,
                GrowHorizontal = GrowDirection.Both,
                GrowVertical = GrowDirection.Both
            };
            
            // Create VBoxContainer to hold the three sections
            var vBoxContainer = new VBoxContainer
            {
                Name = "VBoxContainer",
                LayoutMode = 1,
                AnchorsPreset = (int)LayoutPreset.FullRect,
                GrowHorizontal = GrowDirection.Both,
                GrowVertical = GrowDirection.Both
            };

            // Create top panel
            _topPanel = new Panel
            {
                Name = "Left_Top_Panel",
                CustomMinimumSize = new Vector2(0, 40),
                LayoutMode = 2
            };

            // Create middle panel
            _middlePanel = new Panel
            {
                Name = "Middle",
                CustomMinimumSize = new Vector2(0, 80),
                LayoutMode = 2,
                SizeFlagsVertical = Control.SizeFlags.Fill | Control.SizeFlags.Expand
            };

            // Create middle center container
            _middleCenterContainer = new CenterContainer
            {
                Name = "Left_Middle_CenterContainer",
                LayoutMode = 1,
                AnchorsPreset = (int)LayoutPreset.FullRect,
                GrowHorizontal = GrowDirection.Both,
                GrowVertical = GrowDirection.Both
            };

            // Create bottom panel
            _bottomPanel = new Panel
            {
                Name = "Left_Bottom",
                CustomMinimumSize = new Vector2(0, 40),
                ClipContents = true,
                LayoutMode = 2
            };

            // Add middle center container to middle panel
            _middlePanel.AddChild(_middleCenterContainer);

            // Add all panels to VBoxContainer
            vBoxContainer.AddChild(_topPanel);
            vBoxContainer.AddChild(_middlePanel);
            vBoxContainer.AddChild(_bottomPanel);

            // Add VBoxContainer to main panel
            mainPanel.AddChild(vBoxContainer);

            // Add main panel to left HBoxContainer
            _leftPanel.AddChild(mainPanel);

            // Add left panel to root
            AddChild(_leftPanel);

            // For testing - add some colored panels to see the layout
            AddTestContent();
        }

        private void AddTestContent()
        {
            // Add colored rectangles to help visualize the layout
            var topColor = new ColorRect
            {
                Color = Colors.Red.Darkened(0.5f),
                CustomMinimumSize = new Vector2(150, 40)
            };
            _topPanel.AddChild(topColor);

            var middleColor = new ColorRect
            {
                Color = Colors.Green.Darkened(0.5f),
                CustomMinimumSize = new Vector2(150, 80)
            };
            _middleCenterContainer.AddChild(middleColor);

            var bottomColor = new ColorRect
            {
                Color = Colors.Blue.Darkened(0.5f),
                CustomMinimumSize = new Vector2(150, 40)
            };
            _bottomPanel.AddChild(bottomColor);
        }
    }
}
