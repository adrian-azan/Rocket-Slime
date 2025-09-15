using Godot;
using System.Collections.Generic;

public partial class LilBuddy : Node3D
{
    private float speed = 50f;
    private float agentSpeed = .05f;
    private Rigid_Body _RigidBody;

    [Export]
    public Node3D _Follow;

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
        if (_Follow is PathFollow3D)
        {
            (_Follow as PathFollow3D).ProgressRatio += agentSpeed * felta;
        }

        var player = _Follow as Player;

        //_PhysicsProcessTightFollow(felta);
    }

    public void Move()
    {
        if (_Follow is PathFollow3D)
        {
            FollowAgent();
            ClearBreadCrumbs();
        }
        else
        {
            FollowBreadCrumbs();
        }
    }

    public void FollowBreadCrumbs()
    {
        if (_breadCrumbs.Count == 0)
            return;

        var impulseViaDistance = Tools.distanceFromPointFlat(_RigidBody.GlobalPosition, _breadCrumbs.Peek().pos);

        _RigidBody.ApplyCentralImpulse(new Vector3(
            _breadCrumbs.Peek().pos.X - _RigidBody.GlobalPosition.X,
            0,
            _breadCrumbs.Peek().pos.Z - _RigidBody.GlobalPosition.Z
       ).Normalized() * Mathf.Clamp(impulseViaDistance * .8f, 0, 2));

        if (_breadCrumbs.Count > 0 &&
            Tools.distanceFromPointFlat(_RigidBody.GlobalPosition, _breadCrumbs.Peek().pos) <= .5)
        {
            Node3D removedBreadCrumb = _breadCrumbs.Dequeue().body;
            removedBreadCrumb.QueueFree();
        }
    }

    public void FollowAgent()
    {
        if (_Follow == null)
            return;

        if (Tools.distanceFromPointFlat(_RigidBody.GlobalPosition, _Follow.GlobalPosition) < .1f)
            return;

        var impulseViaDistance = Tools.distanceFromPointFlat(_RigidBody.GlobalPosition, _Follow.GlobalPosition);

        _RigidBody.ApplyCentralImpulse(new Vector3(
             _Follow.GlobalPosition.X - _RigidBody.GlobalPosition.X,
             0,
             _Follow.GlobalPosition.Z - _RigidBody.GlobalPosition.Z
        ).Normalized() * impulseViaDistance * .4f);
    }

    public void _PhysicsProcessTightFollow(float felta)
    {
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

        if (_Follow is PathFollow3D)
        {
            (_Follow as PathFollow3D).ProgressRatio += agentSpeed * felta;

            if (Tools.distanceFromPointFlat(_RigidBody.GlobalPosition, _Follow.GlobalPosition) > 1 &&
            _RigidBody.LinearVelocity.Length() < 3)
            {
                _RigidBody.ApplyCentralForce(new Vector3(
             _Follow.GlobalPosition.X - _RigidBody.GlobalPosition.X,
             0,
                _Follow.GlobalPosition.Z - _RigidBody.GlobalPosition.Z
                ).Normalized() * speed);
            }
        }
    }

    public override void _Process(double delta)
    {
        Player player = _Follow as Player;
        // GetNode<Label3D>("RigidBody3D/Label3D").Text = (Mathf.RoundToInt(Tools.distanceFromPointFlat(player._RigidBody.GlobalPosition, _RigidBody.GlobalPosition) * 100) / 100f).ToString();
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

        float followRadiusX = Tools.rng.RandfRange(-2, 2);
        float followRadiusZ = Tools.rng.RandfRange(-2, 2);
        float followRadius = Mathf.Sqrt((followRadiusX * followRadiusX) + (followRadiusZ * followRadiusZ));
        var newPos = player._RigidBody.GlobalPosition + new Vector3(followRadiusX, 0, followRadiusZ);

        //If there is a breadcrumb AND the player is within 2 units of it, don't put down anymore breadcrumbs
        //OR
        //If LilBuddy is within 1 unit of player, don't put down anymore breadcrumbs
        if (
            (_breadCrumbs.Count > 0 && Tools.distanceFromPointFlat(player._RigidBody.GlobalPosition, _breadCrumbs._Back.body.GlobalPosition) < followRadius)
            || Tools.distanceFromPointFlat(player._RigidBody.GlobalPosition, _RigidBody.GlobalPosition) < followRadius
            )
            return;

        var newBreadCrumb = ResourceLoader.Load<PackedScene>("res://Scenes/DEBUG/LilDudePathFollow.tscn").Instantiate<Node3D>();
        _breadCrumbs.Enqueue((newBreadCrumb, newPos));
        AddChild(newBreadCrumb);
        newBreadCrumb.GlobalPosition = newPos;

        if (_breadCrumbs.Count >= 100)
        {
            foreach (var item in _breadCrumbs)
            {
                item.body.QueueFree();
            }

            _breadCrumbs.Clear();
        }
    }

    public void ClearBreadCrumbs()
    {
        for (int i = 0; i < _breadCrumbs.Count; i++)
        {
            _breadCrumbs.Dequeue().body.QueueFree();
        }
    }
}