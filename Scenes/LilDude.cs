using Godot;
using System.Collections.Generic;

public partial class LilDude : Node3D
{
    private float speed = 50f;
    private Rigid_Body _RigidBody;

    [Export]
    private Node3D _Follow;

    private QueueB<(Node3D body, Vector3 pos)> _breadCrumbs;
    private Stack<Node3D> _Items;

    public override void _Ready()

    {
        base._Ready();
        _RigidBody = GetNode<Rigid_Body>("RigidBody3D");

        _Items = new Stack<Node3D>();
        _breadCrumbs = new QueueB<(Node3D, Vector3)>();
    }

    public override void _PhysicsProcess(double delta)
    {
        float felta = (float)(delta);

        foreach (Item item in _Items)
        {
            item._AnimationPlayer.Play("Bounce");
        }

        if (_breadCrumbs.Count > 0 &&
            Tools.distanceFromPointFlat(_RigidBody.GlobalPosition, _breadCrumbs.Peek().pos) > .3 &&
            _RigidBody.LinearVelocity.Length() < 5)
        {
            _RigidBody.ApplyCentralForce(new Vector3(
            _breadCrumbs.Peek().pos.X - _RigidBody.GlobalPosition.X,
            0,
            _breadCrumbs.Peek().pos.Z - _RigidBody.GlobalPosition.Z
            ).Normalized() * speed);
        }

        if (_breadCrumbs.Count > 0 &&
           Tools.distanceFromPointFlat(_RigidBody.GlobalPosition, _breadCrumbs.Peek().pos) <= .5)
        {
            var test = _breadCrumbs.Dequeue();
            test.body.QueueFree();
        }
    }

    public override void _Process(double delta)
    {
    }

    public void Pickup(Node3D other)
    {
        var target = Tools.GetRoot<Item>(other);

        if (target is Item && _RigidBody.ChildrenSize() - 3 < 1)
        {
            var t = target as Item;
            _RigidBody.AddChild(t, Vector3.Up * (_RigidBody.ChildrenSize() - 2));
            _Items.Push(t);

            var t_rigid = t.GetNode<Rigid_Body>("RigidBody3D");
            t_rigid.Disable();

            t._AnimationPlayer.Play("Bounce");
        }
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

    public void AddBreadCrumb()
    {
        if (_Follow == null || _Follow is not Player)
            return;

        Player player = _Follow as Player;

        var newTotem = ResourceLoader.Load<PackedScene>("res://Scenes/LilDudePathFollow.tscn").Instantiate<Node3D>();
        var newPos = player._RigidBody.GlobalPosition +
            new Vector3(Tools.rng.RandfRange(-3, 3), 0, Tools.rng.RandfRange(-3, 3));

        if (Tools.distanceFromPointFlat(player._RigidBody.GlobalPosition, _RigidBody.GlobalPosition) < 5)
            return;

        _breadCrumbs.Enqueue((newTotem, newPos));
        AddChild(newTotem);
        newTotem.GlobalPosition = newPos;

        if (_breadCrumbs.Count >= 10)
        {
            foreach (var item in _breadCrumbs)
            {
                item.body.QueueFree();
            }

            _breadCrumbs.Clear();
        }
    }
}