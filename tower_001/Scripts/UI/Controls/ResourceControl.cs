using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// The individual resource control that combines icon and value
public partial class ResourceControl : Control, IUIObject, IUISceneElement
{
	[Export] private TextureRect _icon;
	[Export] private Label _valueLabel;
	[Export] public string ObjectID { get; set; }

	[Export] public GlobalEnums.PanelPosition PanelLocation { get; set; }
	[Export] public GlobalEnums.PanelSection PanelSection { get; set; }
	[Export] public GlobalEnums.UIControlType ControlType { get; set; }
	[Export] public bool IsVisibleByDefault { get; set; }
	public GlobalEnums.UIElementType ElementType { get; }
	public string ResourceKey { get; }

	public ResourceControl()
	{
		ResourceKey = "resource_item_top";
		ElementType = GlobalEnums.UIElementType.ResourcePanel;
	}


	public override void _Ready()
	{
		try
		{
			

		}
		catch( Exception ex)
		{
			GD.Print(ex.Message);

		}
	}

	public void Setup(IMenu ParentMenu)
	{

		
		UpdateIcon("res://icon.svg");
		_valueLabel.Text = "1212122";
		
		RegisterControl(ParentMenu);
	}

	public void UpdateValue(int value)
	{
		_valueLabel.Text = value.ToString();
	}

	public void UpdateIcon(string path)
	{
		_icon.Texture = GD.Load<Texture2D>(path);
	}

	public void RegisterControl()
	{
		var uiControlEvent = new UIControlEventArgs(
			GlobalEnums.UIPanelEvent.Register,this,ObjectID,PanelLocation,PanelSection,ControlType);
		Globals.Instance.gameMangers.Events.RaiseEvent(GlobalEnums.EventType.UIControlRegister, uiControlEvent);
	}

	public void RegisterControl(IMenu menu)
	{
		var uiControlEvent = new UIControlEventArgs(
			GlobalEnums.UIPanelEvent.Register,
			this,
			ObjectID,
			PanelLocation,
			PanelSection,
			ControlType
		);
		uiControlEvent.ControlParent = menu;
		Globals.Instance.gameMangers.Events.RaiseEvent(GlobalEnums.EventType.UIControlRegister, uiControlEvent);
	}
}
