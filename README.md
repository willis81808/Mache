# Mache

Mache is a modding framework designed for Sons of the Forest. As a utility it provides support for modders, allowing simple creation of new game content and functions to tweak existing elements of the game. Mache provides access to a unified set of tools, menus, and actions that make the process of creating and implementing mods easier and more streamlined.

## Usage
Press `F1` to open the **Mache** mod menu

## Modding API
Easily register your own menu action with **Mache**
```cs
Mache.RegisterMod(() => new ModDetails
{
	Name		= "Example Mod",
	Id 			= "your.unique.mod.id",
	Version		= "1.0.0",
	Description	= "Lorem ipsum dolor sit amet...",
	OnMenuShow	= () => Log("Player clicked the 'Open Settings' button for your mod")
});
```

## Expanded Item Database
Easily access the IDs and data of all in-game items
```cs
ItemData meatData = GameItem.Meat.GetData();
int meatId = GameItem.Meat.GetId();
```