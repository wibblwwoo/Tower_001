using Godot;
using Tower_001.Scripts.UI.Layout;

namespace Tower_001.Scripts.UI.Menus
{
    public abstract class GameLayoutMenu : BaseMenu
    {
        protected ScalableMenuLayout _layout;

        protected GameLayoutMenu(string menuName) : base(menuName)
        {
        }

        protected override Control CreateMenuRoot()
        {
            // Create and initialize the scalable layout
            _layout = new ScalableMenuLayout
            {
                Name = $"{_menuName}Layout"
            };

            // Set the layout to fill the entire space
            _layout.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            
            return _layout;
        }

        // Helper methods for adding controls to specific panels
        protected void AddToLeftPanelTop(Control node)
        {
            _layout?.AddToLeftPanelTop(node);
        }

        protected void AddToLeftPanelMiddle(Control node)
        {
            _layout?.AddToLeftPanelMiddle(node);
        }

        protected void AddToLeftPanelBottom(Control node)
        {
            _layout?.AddToLeftPanelBottom(node);
        }

        protected void AddToRightPanelTop(Control node)
        {
            _layout?.AddToRightPanelTop(node);
        }

        protected void AddToRightPanelMiddle(Control node)
        {
            _layout?.AddToRightPanelMiddle(node);
        }

        protected void AddToRightPanelBottom(Control node)
        {
            _layout?.AddToRightPanelBottom(node);
        }

        protected void ClearLeftPanel()
        {
            _layout?.ClearLeftPanel();
        }

        protected void ClearRightPanel()
        {
            _layout?.ClearRightPanel();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            _layout = null;
        }
    }
}
