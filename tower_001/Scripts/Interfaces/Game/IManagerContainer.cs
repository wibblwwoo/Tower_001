using Godot;
using System;

public interface IManagerContainer 
{
		EventManager Events { get; }
		//WorldManager World { get; }
		UIManager UI { get; }
}
