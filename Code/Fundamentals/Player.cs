using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

public partial class Player : Node3D
{
    public Rigid_Body _RigidBody;
    private float _acceleration = 50f;
    private float _speed = 4f;

    private Stack<Node3D> _Items;

    public bool _Prepared { get; private set; }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = new Vector3(0, 0, 0);

        if (Input.IsActionPressed("MoveForward"))
        {
            velocity.Z -= 1.0f; // Move forward
        }
        if (Input.IsActionPressed("MoveBackward"))
        {
            velocity.Z += 1.0f; // Move backward
        }
        if (Input.IsActionPressed("MoveLeft"))
        {
            velocity.X += 1.0f; // Move left
        }
        if (Input.IsActionPressed("MoveRight"))
        {
            velocity.X -= 1.0f; // Move right
        }

        // Normalize the velocity vector to ensure consistent speed when moving diagonally.
        if (velocity.Length() > 0)
        {
            velocity = velocity.Normalized() * _acceleration;
        }

        if (_Prepared && Input.IsActionJustReleased("Dash"))
        {
            velocity *= 3.0f;
            Dash();
        }

        if (velocity.Length() != 0)
        {
            foreach (Item item in _Items)
            {
                item._AnimationPlayer.Play("Bounce");
            }
        }

        _RigidBody.ApplyCentralForce(velocity);
        _RigidBody.LinearVelocity = _RigidBody.LinearVelocity.LimitLength(_speed);

        if (Input.IsActionJustPressed("Throw"))
        {
            Throw(_RigidBody.LinearVelocity.LimitLength(1));
        }
    }

    public override void _Ready()
    {
        _RigidBody = GetNode<Rigid_Body>("RigidBody3D");

        _Items = new Stack<Node3D>();
        _Prepared = true;
    }

    public void Throw(Vector3 Direction)
    {
        if (_Items.Count > 0)
        {
            var item = _Items.Pop() as Item;
            _RigidBody.RemoveChild(item);
            item._AnimationPlayer.Stop();
            item.Throw(Direction);
        }
    }

    public void Pickup(Node other)
    {
        var target = Tools.GetRoot<Item>(other);

        if (target is Item && _RigidBody.ChildrenSize() - 3 <= 3)
        {
            var t = target as Item;
            _RigidBody.AddChild(t, Vector3.Up * (_RigidBody.ChildrenSize() - 2));
            _Items.Push(t);

            var t_rigid = t.GetNode<Rigid_Body>("RigidBody3D");
            t_rigid.Disable();

            t._AnimationPlayer.Play("Bounce");
        }
    }

    public async void Dash()
    {
        _speed *= 2;
        _Prepared = false;
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
        _Prepared = true;
        _speed /= 2;
    }

    public void Check(bool state)
    {
        for (int i = 0; i < GetChildCount(); i++)
        {
            var child = GetChild<Node3D>(i);
            GD.Print(child.Name);
            if (child.Name == "Exclamation")
            {
                child.Visible = state;
            }
            else
            {
                for (int j = 0; j < child.GetChildCount(); j++)
                {
                    var grandchild = child.GetChild<Node3D>(j);
                    GD.Print(grandchild.Name);
                    if (grandchild.Name == "Exclamation")
                    {
                        grandchild.Visible = state;
                    }
                }
            }
        }
    }
}