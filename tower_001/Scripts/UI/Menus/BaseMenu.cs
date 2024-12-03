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
            
            // Ensure menu starts hidden
            _menuRoot.Hide();
            GD.Print($"{_menuName}: Menu initialized and hidden");
        }

        protected abstract Control CreateMenuRoot();

        public virtual void Show()
        {
            if (_menuRoot == null)
            {
                GD.PrintErr($"{_menuName}: Cannot show menu - menu root is null!");
                return;
            }

            GD.Print($"Showing menu: {_menuName}");
            _menuRoot.Show();
            _menuRoot.MoveToFront(); // Ensure this menu is on top
        }

        public virtual void Hide()
        {
            if (_menuRoot == null)
            {
                GD.PrintErr($"{_menuName}: Cannot hide menu - menu root is null!");
                return;
            }

            GD.Print($"Hiding menu: {_menuName}");
            _menuRoot.Hide();
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
