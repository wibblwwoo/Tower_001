using Godot;
using System;
using System.Collections.Generic;

public class ManagerDependencyResolver
{
	private Dictionary<Type, IManager> _managers = new();

	public void AddManager(IManager manager)
	{
		_managers[manager.GetType()] = manager;
	}

	public List<IManager> GetInitializationOrder()
	{
		var initialized = new HashSet<Type>();
		var result = new List<IManager>();

		foreach (var manager in _managers.Values)
		{
			InitializeManager(manager, initialized, result);
		}

		return result;
	}

	private void InitializeManager(IManager manager, HashSet<Type> initialized, List<IManager> result)
	{
		var type = manager.GetType();
		if (initialized.Contains(type)) return;

		foreach (var dependency in manager.Dependencies)
		{
			if (_managers.TryGetValue(dependency, out var dependencyManager))
			{
				InitializeManager(dependencyManager, initialized, result);
			}
			else
			{
				throw new Exception($"Missing dependency {dependency} for {type}");
			}
		}

		initialized.Add(type);
		result.Add(manager);
	}
}