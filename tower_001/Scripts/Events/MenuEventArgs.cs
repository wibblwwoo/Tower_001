using System;
using static GlobalEnums;

public class MenuEventArgs : GameEventArgs
{
    public MenuAction Action { get; }

    public MenuEventArgs(MenuAction action)
    {
        Action = action;
    }
}
