using Godot;
using System;

public partial class Chute : Node3D
{
    [Export]
    public PackedScene _Item;

    [Export]
    public bool ON;

    private int _MinTime;
    private int _MaxTime;

    public bool _Prepared { get; private set; }

    public override void _Ready()
    {
        Random rng = new Random();
        _MinTime = rng.Next(1, 5);
        _MaxTime = rng.Next(5, 8);

        _Prepared = true;
        SpawnItem();
    }

    public async void SpawnItem()
    {
        if (!ON)
            return;
        //Lock/unlock
        //        _Prepared = false;
        //      await ToSignal(GetTree().CreateTimer(new Random().Next(_MinTime, _MaxTime)), SceneTreeTimer.SignalName.Timeout);
        //    _Prepared = true;

        //Setup
        var item = _Item.Instantiate<Node3D>();
        AddChild(item);

        //Manipulate
        item.Position = new Vector3(0, 0, 0);
        var rb = item.GetNode<RigidBody3D>("RigidBody3D");

        if (RotationDegrees.Y == 90)
        {
            rb.ApplyImpulse((Vector3.Forward + Vector3.Up) * 4);
        }
        else
        {
            rb.ApplyImpulse((Vector3.Left + Vector3.Up) * 4);
        }
    }
}