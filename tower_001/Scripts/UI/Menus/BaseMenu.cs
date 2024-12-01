using Godot;

namespace Tower_001.Scripts.UI.Menus
{
    public abstract class BaseMenu
    {
        // OLD CODE - For rollback if needed
        /*
        protected Control _uiRoot;
        protected MarginContainer _container;
        protected VBoxContainer _buttonContainer;
        protected string _menuName;
        */

        // NEW CODE - Core menu functionality
        protected Control _uiRoot;
        protected Control _menuRoot;
        protected string _menuName;

        protected BaseMenu(string menuName)
        {
            _menuName = menuName;
        }

        // OLD CODE - For rollback if needed
        /*
        public virtual void Initialize(Control uiRoot)
        {
            _uiRoot = uiRoot;
            
            // Create a unique container for this menu
            _container = new MarginContainer();
            _container.Name = $"{_menuName}Container";

            // Get the Control2 node and verify its state
            var control2 = _uiRoot.GetNode<Control>("Control2");
            if (control2 == null)
            {
                GD.PrintErr($"{_menuName}: Control2 node not found in UI root!");
                return;
            }
            
            if (!control2.Visible)
            {
                GD.PrintErr($"{_menuName}: Control2 node is disabled! Enabling it...");
                control2.Show();
            }

            GD.Print($"{_menuName}: Adding container to Control2 node");
            control2.AddChild(_container);
            
            // Set container properties
            _container.AnchorRight = 1;
            _container.AnchorBottom = 1;
            _container.AnchorLeft = 0;
            _container.AnchorTop = 0;
            
            // Remove any default margins
            _container.AddThemeConstantOverride("margin_left", 0);
            _container.AddThemeConstantOverride("margin_right", 0);
            _container.AddThemeConstantOverride("margin_top", 0);
            _container.AddThemeConstantOverride("margin_bottom", 0);
            
            // Create button container
            _buttonContainer = new VBoxContainer();
            _buttonContainer.Name = $"{_menuName}Buttons";
            _container.AddChild(_buttonContainer);
            
            // Center the buttons
            _buttonContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
            _buttonContainer.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
            
            // Hide by default
            Hide();
        }

        public virtual void Show()
        {
            GD.Print($"Showing menu: {_menuName}");
            _container.Visible = true;
        }

        public virtual void Hide()
        {
            if (_container != null)
            {
                GD.Print($"Hiding menu: {_menuName}");
                _container.Visible = false;
            }
        }
        */

        // NEW CODE - Core menu functionality
        public virtual void Initialize(Control uiRoot)
        {
            _uiRoot = uiRoot;
            
            // Get the Control2 node and verify its state
            var control2 = _uiRoot.GetNode<Control>("Control2");
            if (control2 == null)
            {
                GD.PrintErr($"{_menuName}: Control2 node not found in UI root!");
                return;
            }
            
            if (!control2.Visible)
            {
                GD.PrintErr($"{_menuName}: Control2 node is disabled! Enabling it...");
                control2.Show();
            }

            // Create menu's root control
            _menuRoot = CreateMenuRoot();
            GD.Print($"{_menuName}: Adding menu root to Control2 node");
            control2.AddChild(_menuRoot);
            
            // Hide by default
            Hide();
        }

        protected abstract Control CreateMenuRoot();

        public virtual void Show()
        {
            GD.Print($"Showing menu: {_menuName}");
            _menuRoot?.Show();
        }

        public virtual void Hide()
        {
            if (_menuRoot != null)
            {
                GD.Print($"Hiding menu: {_menuName}");
                _menuRoot.Hide();
            }
        }

        public virtual void Cleanup()
        {
            if (_menuRoot != null)
            {
                _menuRoot.QueueFree();
                _menuRoot = null;
            }
        }
    }
}
