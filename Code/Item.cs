using Godot;
using System;

public partial class Item : Node3D
{
    public Rigid_Body _RigidBody;
    public AnimationPlayer _AnimationPlayer;

    public override void _Ready()
    {
        _RigidBody = GetNodeOrNull<Rigid_Body>("RigidBody3D");
        _AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public async void Throw(Vector3 Direction)
    {
        _RigidBody.SetCollisionLayerValue(9, true);
        _RigidBody.SetCollisionLayerValue(10, false);

        _RigidBody.Enable();
        _RigidBody.AddForce((Direction) * 750);

        await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
        _RigidBody.SetCollisionLayerValue(9, false);
        _RigidBody.SetCollisionLayerValue(10, true);
    }
}