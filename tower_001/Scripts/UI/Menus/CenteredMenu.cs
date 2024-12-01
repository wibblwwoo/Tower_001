using Godot;

namespace Tower_001.Scripts.UI.Menus
{
    public abstract class CenteredMenu : BaseMenu
    {
        protected MarginContainer _container;
        protected VBoxContainer _buttonContainer;

        protected CenteredMenu(string menuName) : base(menuName)
        {
        }

        protected override Control CreateMenuRoot()
        {
            // Create the margin container as the menu root
            _container = new MarginContainer
            {
                Name = $"{_menuName}Container"
            };

            // Set container to fill the entire space
            _container.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            
            // Create centered button container
            _buttonContainer = new VBoxContainer
            {
                Name = $"{_menuName}Buttons"
            };
            _container.AddChild(_buttonContainer);
            
            // Center the buttons
            _buttonContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
            _buttonContainer.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;

            return _container;
        }

        protected void AddButton(Button button)
        {
            _buttonContainer.AddChild(button);
        }

        protected virtual void ClearButtons()
        {
            if (_buttonContainer == null) return;
            
            foreach (var child in _buttonContainer.GetChildren())
            {
                child.QueueFree();
            }
        }

        public override void Cleanup()
        {
            ClearButtons();
            base.Cleanup();
            _container = null;
            _buttonContainer = null;
        }
    }
}
