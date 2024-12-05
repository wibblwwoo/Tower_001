using Godot;
using System;
using System.Collections.Generic;

public interface IManager 
{
	// Called when the node enters the scene tree for the first time.
	void Setup();
	IEnumerable<Type> Dependencies { get; }
}
