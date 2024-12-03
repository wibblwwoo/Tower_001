using Godot;
using System.Threading.Tasks;

namespace Tower_001.Scripts.UI.Layout
{
    public partial class MenuPanelContainer : VBoxContainer
    {

		[Export] private Panel _topPanel;
        [Export] private Panel _middlePanel;
        [Export] private VBoxContainer _middleCenterContainer;
        [Export] private Panel _bottomPanel;
		[Export] private Panel _parentPanel; //toshow or hide section


		private const int TOP_PANEL_HEIGHT = 40;
		private const int BOTTOM_PANEL_HEIGHT = 40;
		private Tween _currentTween;

		public Panel TopPanel => _topPanel;
        public Panel MiddlePanel => _middlePanel;
        public VBoxContainer MiddleCenterContainer => _middleCenterContainer;
        public Panel BottomPanel => _bottomPanel;
		public Panel ParentPanel => _parentPanel;
		

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
			ApplyStyle();

		}
		public void AddButton(Button button)
		{
			if (_middleCenterContainer == null)
			{
				GD.PrintErr($"Cannot add button to {Name}: _middleCenterContainer is null");
				return;
			}

			// Ensure consistent button setup
			//button.SizeFlagsHorizontal = SizeFlags.Expand;
			//button.CustomMinimumSize = new Vector2(140, 30);
			_middleCenterContainer.AddChild(button);
		}

		public void AddToTop(Control control, bool center = true)
		{
			if (_topPanel == null)
			{
				GD.PrintErr($"Cannot add to top of {Name}: _topPanel is null");
				return;
			}

			if (center)
			{
				var container = new CenterContainer();
				container.AddChild(control);
				_topPanel.AddChild(container);
			}
			else
			{
				_topPanel.AddChild(control);
			}
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

		public void ApplyStyle(StyleBox styleBox = null)
		{
			if (styleBox == null)
			{
				styleBox = new StyleBoxFlat
				{
					BgColor = new Color(0.2f, 0.2f, 0.2f),
					BorderWidthBottom = 1,
					BorderWidthLeft = 1,
					BorderWidthRight = 1,
					BorderWidthTop = 1,
					BorderColor = new Color(0.3f, 0.3f, 0.3f)
				};
			}

			if (_topPanel != null) _topPanel.AddThemeStyleboxOverride("panel", styleBox);
			if (_middlePanel != null) _middlePanel.AddThemeStyleboxOverride("panel", styleBox);
			if (_bottomPanel != null) _bottomPanel.AddThemeStyleboxOverride("panel", styleBox);
		}
		public async Task ShowWithAnimation()
		{
			if (_parentPanel == null)
			{
				GD.PrintErr($"Cannot animate {Name}: _parentPanel is null");
				return;
			}

			// Kill any existing tween
			_currentTween?.Kill();

			// Create new tween
			_currentTween = CreateTween();

			// Set initial state
			_parentPanel.Modulate = new Color(1, 1, 1, 0);
			_parentPanel.Visible = true;

			// Animate fade in
			_currentTween.TweenProperty(_parentPanel, "modulate:a", 1.0f, 0.2f)
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Cubic);

			await ToSignal(_currentTween, "finished");
		}

		public async Task HideWithAnimation()
		{
			if (_parentPanel == null)
			{
				GD.PrintErr($"Cannot animate {Name}: _parentPanel is null");
				return;
			}

			// Kill any existing tween
			_currentTween?.Kill();

			// Create new tween
			_currentTween = CreateTween();

			// Animate fade out
			_currentTween.TweenProperty(_parentPanel, "modulate:a", 0.0f, 0.2f)
				.SetEase(Tween.EaseType.In)
				.SetTrans(Tween.TransitionType.Cubic);

			await ToSignal(_currentTween, "finished");
			_parentPanel.Visible = false;
		}

		public override void _ExitTree()
		{
			base._ExitTree();
			_currentTween?.Kill();
			_currentTween = null;
		}
	}
}
