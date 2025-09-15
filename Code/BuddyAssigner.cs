using Godot;
using Godot.Collections;
using System;
using System.Reflection.Metadata.Ecma335;

public partial class BuddyAssigner : Node3D
{
    [Export]
    public Array<Path3D> _paths;

    private Dictionary<Node3D, Node> _buddies;

    public override void _Ready()
    {
        _paths = Tools.GetChildren<Path3D>(GetParent());
        _buddies = new Dictionary<Node3D, Node>();
    }

    public override void _Process(double delta)
    {
        float felta = (float)delta;
        foreach (var followAgent in _buddies.Values)
        {
            (followAgent as Node3D).RotateObjectLocal(Vector3.Up, (float)delta);
            (followAgent as PathFollow3D).ProgressRatio += .05f * felta;
            (followAgent as PathFollow3D).GetNode<PathFollow3D>("Path3D/Node3D").ProgressRatio += .15f * felta;
        }
    }

    public void Assign(Area3D buddyArea)
    {
        LilBuddy buddy = Tools.GetRoot<LilBuddy>(buddyArea);

        if (_buddies.ContainsKey(buddy))
            return;

        Node3D follow = ResourceLoader.Load<PackedScene>("res://Scenes/DEBUG/PathAgent.tscn").Instantiate() as Node3D;
        _paths.PickRandom().AddChild(follow);

        (follow as PathFollow3D).ProgressRatio = Tools.rng.RandfRange(-.1f, .1f);
        (follow as PathFollow3D).GetNode<PathFollow3D>("Path3D/Node3D").ProgressRatio = Tools.rng.RandfRange(0f, .8f);
        buddy._Follow = follow.GetNode<Node3D>("Path3D/Node3D");

        _buddies.Add(buddy, follow);
        buddy.ClearBreadCrumbs();
    }
}