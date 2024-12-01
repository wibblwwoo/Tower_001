using Godot;

namespace Tower_001.Scripts.UI.Layout
{
    public interface IMenuPanelLayout
    {
        Panel TopPanel { get; }
        CenterContainer MiddleCenterContainer { get; }
        Panel BottomPanel { get; }
    }
}
