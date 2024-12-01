using Godot;

namespace Tower_001.Scripts.UI.Layout
{
    public partial class MenuPanelContainer : VBoxContainer
    {
        [Export] private Panel _topPanel;
        [Export] private Panel _middlePanel;
        [Export] private VBoxContainer _middleCenterContainer;
        [Export] private Panel _bottomPanel;

        public Panel TopPanel => _topPanel;
        public Panel MiddlePanel => _middlePanel;
        public VBoxContainer MiddleCenterContainer => _middleCenterContainer;
        public Panel BottomPanel => _bottomPanel;

        public override void _Ready()
        {
            base._Ready();
            
            // Apply styles to panels
            //if (_topPanel != null) ScalableMenuLayout.ApplyPanelStyle(_topPanel);
            //if (_middlePanel != null) ScalableMenuLayout.ApplyPanelStyle(_middlePanel);
            //if (_bottomPanel != null) ScalableMenuLayout.ApplyPanelStyle(_bottomPanel);

            // Set up default properties if not already set in scene
            if (_topPanel != null)
            {
                _topPanel.CustomMinimumSize = new Vector2(0, 40);
                _topPanel.SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand;
            }
        }

        public void AddButton(Button button)
        {
			_middleCenterContainer?.AddChild(button);
        }

        public void AddToTop(Control control)
        {
            _topPanel?.AddChild(control);
        }

		public void AddToMiddle(Control control)
		{
			_middlePanel?.AddChild(control);
		}

		public void AddToBottom(Control control)
        {
            _bottomPanel?.AddChild(control);
        }

        public void ClearButtons()
        {
            foreach (var child in _middleCenterContainer?.GetChildren() ?? new Godot.Collections.Array<Node>())
            {
                child.QueueFree();
            }
        }

        public void ClearTop()
        {
            foreach (var child in _topPanel?.GetChildren() ?? new Godot.Collections.Array<Node>())
            {
                child.QueueFree();
            }
        }
		public void ClearMiddle()
		{
			foreach (var child in _middlePanel?.GetChildren() ?? new Godot.Collections.Array<Node>())
			{
				child.QueueFree();
			}
		}
		public void ClearBottom()
        {
            foreach (var child in _bottomPanel?.GetChildren() ?? new Godot.Collections.Array<Node>())
            {
                child.QueueFree();
            }
        }
    }
}
