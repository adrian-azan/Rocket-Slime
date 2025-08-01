using Godot;
using System;

public partial class Cannon : Node
{
    private AnimationPlayer _AnimationPlayer;

    public override void _Ready()
    {
        _AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void On_Collision(Node3D other)
    {
        var otherRoot = Tools.GetRoot<Item>(other);

        if (otherRoot == null)
            return;

        otherRoot.QueueFree();

        _AnimationPlayer.Play("Kaboom");
    }
}