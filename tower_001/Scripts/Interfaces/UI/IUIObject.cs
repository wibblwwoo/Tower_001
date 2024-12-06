using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GlobalEnums;

public interface IUIObject
{
	string ObjectID { get; set; }
	PanelPosition PanelLocation { get; set; }
	PanelSection PanelSection { get; set; }
	UIControlType ControlType { get; set; }

	bool IsVisibleByDefault { get; set; }

	void RegisterControl();
	void RegisterControl(IMenu menu);
}

