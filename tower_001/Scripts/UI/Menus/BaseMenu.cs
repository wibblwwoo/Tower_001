using Godot;

namespace Tower_001.Scripts.UI.Menus
{
    public abstract class BaseMenu
    {
        protected Control _uiRoot;
        protected MarginContainer _container;
        protected VBoxContainer _buttonContainer;
        protected string _menuName;

        protected BaseMenu(string menuName)
        {
            _menuName = menuName;
        }

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
            
            // OLD CODE - For rollback if needed
            /*
            // Set container properties
            _container.AnchorRight = 1;
            _container.AnchorBottom = 1;
            */

            // NEW CODE - Full anchors and margin removal
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

        // OLD CODE - For rollback if needed
        /*
        public virtual void Show()
        {
            _container.Visible = true;
        }

        public virtual void Hide()
        {
            if (_container != null)
            {
                _container.Visible = false;
            }
        }
        */

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

        protected virtual void ClearButtons()
        {
            foreach (var child in _buttonContainer.GetChildren())
            {
                child.QueueFree();
            }
        }
    }
}
