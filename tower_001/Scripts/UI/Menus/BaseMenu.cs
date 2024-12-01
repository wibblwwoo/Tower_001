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
            _uiRoot.GetNode<Control>("Control2").AddChild(_container);
            
            // Set container properties
            _container.AnchorRight = 1;
            _container.AnchorBottom = 1;
            
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
            _container.Visible = true;
        }

        public virtual void Hide()
        {
            if (_container != null)
            {
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
