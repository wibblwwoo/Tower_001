using Godot;
using System;
using System.Collections.Generic;

public static class Utils {

	public static void FindNodesWithClass<T>(Node currentNode, List<T> resultList) where T : class
	{
		if (currentNode is T nodeWithClass)
		{
			resultList.Add(nodeWithClass);
		}

		foreach (Node child in currentNode.GetChildren())
		{
			FindNodesWithClass(child, resultList);
		}
	}

}

public static class EnumExtensions
{
	// Extension method to get all flags from an enum
	public static IEnumerable<T> GetFlags<T>(this T flags) where T : Enum
	{
		foreach (Enum value in Enum.GetValues(flags.GetType()))
		{
			if (flags.HasFlag(value) && Convert.ToInt32(value) != 0) // Skip None (0) value
			{
				yield return (T)value;
			}
		}
	}
}