using System;
using static GlobalEnums;

public class MenuEventArgs : GameEventArgs
{
    public MenuAction Action { get; }

    public string MenuParent { get; set; }

	public string Menu { get; set; }


	public MenuEventArgs(MenuAction action, string menuParent, string menu)
    {
        Action = action;
        MenuParent = menuParent;
        Menu = menu;
    }
}
