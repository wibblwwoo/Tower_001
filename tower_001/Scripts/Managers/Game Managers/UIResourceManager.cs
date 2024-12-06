using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GlobalEnums;



public partial class UIResourceManager : BaseManager
{
	private Dictionary<UIElementType, Dictionary<string, PackedScene>> _uiResources;
	private const string UI_SCENE_BASE_PATH = "res://Scene/UI/";

	public override IEnumerable<Type> Dependencies => new[]
	{
		typeof(EventManager)  // Only needs events now
    };

	public UIResourceManager()
	{
		_uiResources = new Dictionary<UIElementType, Dictionary<string, PackedScene>>();
	}

	public override void Setup()
	{
		base.Setup();
		LoadAllUIResources();
	}

	protected override void RegisterEventHandlers()
	{
		// Only handle resource-related events
		if (EventManager != null)
		{
			// Could handle events like resource reload, cache clear, etc.
		}
	}

	private void LoadAllUIResources()
	{
		foreach (UIElementType elementType in Enum.GetValues(typeof(UIElementType)))
		{
			_uiResources[elementType] = new Dictionary<string, PackedScene>();
			LoadUIResourcesForType(elementType);
		}
	}

	private void LoadUIResourcesForType(UIElementType elementType)
	{
		string typePath = Path.Combine(UI_SCENE_BASE_PATH, elementType.ToString());
		var dir = DirAccess.Open(typePath);

		if (dir != null)
		{
			dir.ListDirBegin();
			string fileName = dir.GetNext();

			while (fileName != "")
			{
				if (!dir.CurrentIsDir() && fileName.EndsWith(".tscn"))
				{
					string resourcePath = Path.Combine(typePath, fileName);
					var scene = GD.Load<PackedScene>(resourcePath);
					if (scene != null)
					{
						string resourceName = Path.GetFileNameWithoutExtension(fileName);
						_uiResources[elementType][resourceName] = scene;
						GD.Print($"Loaded UI Resource: {elementType}/{resourceName}");
					}
				}
				fileName = dir.GetNext();
			}
			dir.ListDirEnd();
		}
	}

	public PackedScene GetUIResource(UIElementType elementType, string resourceName)
	{
		if (_uiResources.TryGetValue(elementType, out var typeResources))
		{
			if (typeResources.TryGetValue(resourceName, out var resource))
			{
				return resource;
			}
			GD.PrintErr($"UI Resource not found: {elementType}/{resourceName}");
		}
		GD.PrintErr($"UI Element Type not found: {elementType}");
		return null;
	}

	public List<string> GetAvailableResources(UIElementType type)
	{
		if (_uiResources.TryGetValue(type, out var typeResources))
		{
			return typeResources.Keys.ToList();
		}
		return new List<string>();
	}
}


//remove due to refactoring

/*
public partial class UIResourceManager : BaseManager
{
	private Dictionary<UIElementType, Dictionary<string, PackedScene>> _uiResources;

	private List<IUIObject> _uiResourcesPanels;
	private const string UI_SCENE_BASE_PATH = "res://Scene/UI/";

	public UIResourceManager()
	{

		
	}

	public override void Setup()
	{

		_uiResources = new Dictionary<UIElementType, Dictionary<string, PackedScene>>();
		_uiResourcesPanels = new List<IUIObject>();
		base.Setup();
		LoadAllUIResources();

		//find all resource managers and add it to my list.
		var myList = new List<ResourcePanelManager>();
		List<IUIObject> myList2 = Globals.Instance.gameMangers.UI.GetListofAllIUIObjectByType(myList);
		_uiResourcesPanels.AddRange(myList2);

		foreach(IUIObject _ui in _uiResourcesPanels)
		{
			if (_ui is ResourcePanelManager _t)
			{
				_t.Setup();
			}

		}
	}

	protected override void RegisterEventHandlers()
	{
		// Register any events this manager needs to listen to
		if (EventManager != null)
		{
			Globals.Instance.gameMangers.Events.AddHandler<UIControlEventArgs>(EventType.UIControlRegister, OnUIControlRegister);
		}
	}


	//we only care about resource panles. here
	private void OnUIControlRegister(UIControlEventArgs args)
	{
		try
		{
			if (args.UIControlItem != null)
			{
				
				var _item =  args.UIControlItem;
                if (_item is ResourcePanelManager _t)
				{

					_uiResourcesPanels.Add(_t);

				}

            }
		}
		catch (Exception ex)
		{
			GD.Print(ex.Message);

		}
	}

	private void LoadAllUIResources()
	{
		foreach (UIElementType elementType in Enum.GetValues(typeof(UIElementType)))
		{
			string typePath = Path.Combine(UI_SCENE_BASE_PATH, elementType.ToString());
			_uiResources[elementType] = new Dictionary<string, PackedScene>();

			// Get all .tscn files in the directory
			var dir = DirAccess.Open(typePath);
			if (dir != null)
			{
				dir.ListDirBegin();
				string fileName = dir.GetNext();

				while (!string.IsNullOrEmpty(fileName))
				{
					if (fileName.EndsWith(".tscn"))
					{
						string fullPath = Path.Combine(typePath, fileName);
						PackedScene scene = GD.Load<PackedScene>(fullPath);

						if (ValidateUIScene(scene))
						{
							string resourceKey = fileName.Replace(".tscn", "");
							_uiResources[elementType].Add(resourceKey, scene);
							GD.Print($"Loaded UI Resource: {elementType}/{resourceKey}");
						}
					}
					fileName = dir.GetNext();
				}
				dir.ListDirEnd();
			}
		}
	}

	private bool ValidateUIScene(PackedScene scene)
	{
		if (scene == null) return false;

		// Instance the scene to check its root node
		Node instance = scene.Instantiate();
		if (instance == null) return false;

		bool isValid = instance is IUISceneElement;
		instance.QueueFree(); // Clean up the temporary instance

		return isValid;
	}

	public PackedScene GetUIResource(UIElementType type, string resourceKey)
	{
		if (_uiResources.TryGetValue(type, out var typeResources))
		{
			if (typeResources.TryGetValue(resourceKey, out var scene))
			{
				return scene;
			}
			GD.PrintErr($"UI Resource not found: {type}/{resourceKey}");
		}
		GD.PrintErr($"UI Element Type not found: {type}");
		return null;
	}

	public List<string> GetAvailableResources(UIElementType type)
	{
		if (_uiResources.TryGetValue(type, out var typeResources))
		{
			return typeResources.Keys.ToList();
		}
		return new List<string>();
	}
}

*/