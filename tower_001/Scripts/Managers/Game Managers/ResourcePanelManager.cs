using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class ResourcePanelManager : UIControlItem, IMenu 
{
	[Export]
	private int MAX_RESOURCES = 5;
	private Dictionary<int, ResourceControl> _resourceControls;

	[Export]
	private HBoxContainer _resourceContainer;

	[Export]
	private PackedScene _ResourceItem;

	public string menuParentID { get; set; }
	public bool ShowOnStartup { get; set; }


	public void Setup()
	{
		InitializeResourceControls();
	}
	

	private void InitializeResourceControls()
	{

		var _resourceItem = Globals.Instance.gameMangers.ResourceManager.GetUIResource(GlobalEnums.UIElementType.ResourcePanel, "resource_item_top");

		for (int i = 1; i <= MAX_RESOURCES; i++)
		{

			var newResource = _resourceItem.Instantiate();
			_resourceContainer.AddChild(newResource);
			_resourceControls.Add(i, (ResourceControl)newResource);
			((ResourceControl)newResource).Setup(this);
		}


	}

	public void UpdateResource(int slot, int value, string iconPath = null)
	{
		if (_resourceControls.TryGetValue(slot, out var control))
		{
			control.UpdateValue(value);
			if (iconPath != null)
			{
				control.UpdateIcon(iconPath);
			}
		}
	}

	public void SetResourceVisibility(int slot, bool visible)
	{
		if (_resourceControls.TryGetValue(slot, out var control))
		{
			control.Visible = visible;
		}
	}

	public void SetAllResourcesVisibility(bool visible)
	{
		foreach (var control in _resourceControls.Values)
		{
			control.Visible = visible;
		}
	}

	public override void RegisterControl()
	{

			base.RegisterControl();
			menuParentID = "Resource_Panel_Top";
			_resourceControls = new Dictionary<int, ResourceControl>();
			
	}

	
}