using Godot;
using System;

public partial class Item : Node3D
{
    public Rigid_Body _RigidBody;
    public Collision_Body _CollisionBody;
    public AnimationPlayer _AnimationPlayer;
    public bool Grabbed;

    public override void _Ready()
    {
        _RigidBody = Tools.FindRigidBodyFromRoot(this);
        _CollisionBody = GetNodeOrNull<Collision_Body>("Rigid_Body/Collision_Body");
        _AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Grabbed = false;
    }

    public async void Throw(Vector3 Direction)
    {
        Grabbed = false;
        _RigidBody.Enable();
        _RigidBody.AddForce((Direction) * 750);

        await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);

        _CollisionBody.Enable();

        GD.Print(_RigidBody.ToString());
    }
}