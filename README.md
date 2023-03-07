# Mache

Mache is a modding framework designed for Sons of the Forest. As a utility it provides support for modders, allowing simple creation of new game content and functions to tweak existing elements of the game. Mache provides access to a unified set of tools, menus, and actions that make the process of creating and implementing mods easier and more streamlined.

# Usage
Press `F1` to open the **Mache** mod menu

# Modding API

## Mod Menu
Easily register your own menu with **Mache**

### Integrated Menu
You can implement interactable UI elements directly into the Mache menu for your mod! When registering your mod, Mache provides an `OnFinishedCreating` event that is executed when your mod's panel is fully initialized and ready to accept additional elements.
Using the `GameObject` passed to this callback as the parent for your own UI elements will incorporate them into the existing scroll view of your mod's panel in the Mache menu. For example:
```cs
Mache.RegisterMod(() => new ModDetails
{
    Name = "Example Mod",
    Id = "your.unique.mod.id",
    Version = "1.0.0",
    Description = "Lorem ipsum dolor sit amet...",
    OnFinishedCreating = (parent) =>
    {
        MenuPanel.Builder()
            .AddComponent(new LabelComponent
            {
                Text = "Hello World!",
                FontSize = 24
            })
            .AddComponent(new ButtonComponent
            {
                Text = "Press Me",
                OnClick = (self) => MachePlugin.Instance.Log.LogMessage("Down the rabbit hole...")
            })
            .BuildToTarget(parent);
    }
});
```

### Custom/Standalone Menu
If you provide a value for `OnMenuShow` when registering your mod Mache will only display the given name, version, and description in its own window.
Instead of placing options there it will create an "Open Settings" button that, when clicked, will execute your `OnMenuShow` callback.
This is intended for use when you want to have a totally independent and fully self-managed menu. It is up to you how your mod responds when the user clicks this button.   
```cs
Mache.RegisterMod(() => new ModDetails
{
	Name = "Example Mod",
	Id = "your.unique.mod.id",
	Version = "1.0.0",
	Description = "Lorem ipsum dolor sit amet...",
	OnMenuShow = () => Log("Player clicked the 'Open Settings' button for your mod")
});
```

## Expanded Item Database
Easily access the IDs and data of all in-game items
```cs
ItemData meatData = GameItem.Meat.GetData();
int meatId = GameItem.Meat.GetId();
```

## Network Event Creation
**NOTE: All network events must use unique identifiers, and the identifier for a given event must be the same across all clients!**

There are two methods for creating and registering a custom network event:
1. Auto serialize/deserialize
2. With custom serialization/deserialization

### Auto (De)Serialization
For auto serialization create a class that extends `SimpleEvent<T>`, for example:
```cs
public class ExampleEvent : SimpleEvent<ExampleEvent>
{
    public string Message { get; set; }
    public int MessageCount { get; set; }

    // called when an event of this type has been receieved
    public override void OnReceived()
    {
        for (int i = 0; i < MessageCount; i++)
        {
            MachePlugin.Instance.Log.LogMessage(Message);
        }
    }
}
```
Then register it with **Mache**:
```cs
EventDispatcher.RegisterEvent<ExampleEvent>();
```

#### Pros:
- Much easier
- Less boilerplate

#### Cons:
- Your event class must only implement fields that can be automatically serialized to JSON (generally value types and collections of value types)
- Only applies to classes that extend `SimpleEvent<T>`

### Custom (De)Serialization
For more complex scenarios it may be necessary to implement custom serialization and deserialization methods for your class, or if you want to create a network event for types you do not control you can define your own method of serializing/deserializng them.
In these cases you must define your own serialize, deserialize, and event recieved callback methods, for example:
```cs
EventDispatcher.RegisterEvent("Some.Unique.Event.Id", SomeEvent.Serialize, SomeEvent.Deserialize, SomeEventHandler.OnReceived);
```

#### Pros:
- Maximum flexibility
- Can register events that serialize any type

#### Cons:
- This method is much more involved
- Significantly more boilerplate

## Raising Network Events
Events can be raised anywhere and sent to all clients. Using the `ExampleEvent` from above:
```cs
EventDispatcher.RaiseEvent(new ExampleEvent
{
    Message = "Hello World!",
    MessageCount = 10
});
```
