using Godot;
using System;

public partial class Item : Node3D
{
    public Rigid_Body _RigidBody;
    public Collision_Body _CollisionBody;

    public bool Grabbed;

    public override void _Ready()
    {
        _RigidBody = Tools.FindRigidBodyFromRoot(this);
        _CollisionBody = GetNodeOrNull<Collision_Body>("Rigid_Body/Collision_Body");
        Grabbed = false;
    }

    public void Touched(Node3D other)
    {
        if (other is Player)
        {
            GD.Print("NO CONSENT");
        }
    }
     
    public async void Throw(Vector3 Direction)
    {
        Grabbed = false;
        _RigidBody.Enable();
        _RigidBody.AddForce((Vector3.Up/3+Direction) * 500);


        await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);		

        _CollisionBody.Enable();

        GD.Print(_RigidBody.ToString());
    }
}

