using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class Rigid_Body : RigidBody3D
{
    private Dictionary<Node3D, Vector3> _ChildrenAndPos;

    private Collision_Body _Area;

    public override void _Ready()
    {
        Array<Node> _Children = this.GetChildren();
        _ChildrenAndPos = new Dictionary<Node3D, Vector3>();

        foreach (Node3D child in _Children)
        {
            if (child is not RigidBody3D)
            {
                _ChildrenAndPos.Add(child, child.Position);
            }

            if (child is Collision_Body)
            {
                _Area = child as Collision_Body;
            }
        }
    }

    public override void _Process(double delta)
    {
        foreach (var child in _ChildrenAndPos)
        {
            child.Key.GlobalPosition = GlobalPosition + _ChildrenAndPos[child.Key];
        }
    }

    public void AddChild(Node3D child)
    {
        AddChild(child, Vector3.Zero);
    }

    public void AddChild(Node3D child, Vector3 localPos)
    {
        var rb = Tools.FindRigidBodyFromRoot(child);

        child.Position = Vector3.Zero;
        rb.SetPosition(Vector3.Zero);

        try
        {
            _ChildrenAndPos.Add(child as Node3D, localPos);
        }
        catch (ArgumentException ae)
        {
            GD.Print("Child already added");
        }
    }

    public void RemoveChild(Node3D child)
    {
        GD.Print(_ChildrenAndPos.Remove(child));

        _ChildrenAndPos.Remove(child);
        var rb = Tools.FindRigidBodyFromRoot(child);
        rb.ProcessMode = ProcessModeEnum.Inherit;
        rb.Sleeping = false;
    }

    public uint ChildrenSize()
    {
        return (uint)_ChildrenAndPos.Count;
    }

    public void CollisionLayer(uint layer)
    {
        CollisionLayer(layer);
    }

    public void CollisionMask(uint mask)
    {
        CollisionMask(mask);
    }

    public void Disable()
    {
        ProcessMode = ProcessModeEnum.Disabled;
        Sleeping = true;
        //_Area.ProcessMode = ProcessModeEnum.Disabled;
    }

    public void Enable()
    {
        ProcessMode = ProcessModeEnum.Inherit;
        Sleeping = false;
        //_Area.ProcessMode = ProcessModeEnum.Inherit;
    }

    public void AddImpulse(Vector3 dir)
    {
        ApplyImpulse(dir, Vector3.Zero);
    }

    public void AddForce(Vector3 dir)
    {
        ApplyForce(dir, Vector3.Zero);
    }

    public new Vector3 GetPosition()
    {
        return Position;
    }

    public new void SetPosition(Vector3 newPos)
    {
        Position = newPos;
    }

    public new string ToString()
    {
        return LinearVelocity.ToString();
    }
}