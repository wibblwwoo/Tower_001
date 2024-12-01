using Godot;

namespace Tower_001.Scripts.UI.Layout
{
    public partial class SceneMenuPanelLayout : Node, IMenuPanelLayout
    {
        [Export]
        private Panel _topPanel;

        [Export]
        private CenterContainer _middleCenterContainer;

        [Export]
        private Panel _bottomPanel;

        public Panel TopPanel => _topPanel;
        public CenterContainer MiddleCenterContainer => _middleCenterContainer;
        public Panel BottomPanel => _bottomPanel;
    }
}
