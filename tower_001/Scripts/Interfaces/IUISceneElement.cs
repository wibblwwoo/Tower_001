using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GlobalEnums;

public interface IUISceneElement
{
	UIElementType ElementType { get; }
	string ResourceKey { get; }
}