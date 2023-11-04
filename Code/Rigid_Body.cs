using Godot;
using Godot.Collections;

using System.Linq;

public partial class Rigid_Body : Node3D
{
	// Called when the node enters the scene tree for the first time.
	Array<Node> _Children;
    Dictionary<Node3D, Vector3> _ChildrenPos;

	private RigidBody3D _RigidBody;
	public override void _Ready()
	{
		_Children = this.GetChildren();
		_ChildrenPos = new Dictionary<Node3D, Vector3>();

		foreach(Node3D child in _Children)
		{
            _ChildrenPos.Add(child, child.Position);
        }

		var test = _Children.FirstOrDefault(x => x is RigidBody3D) as RigidBody3D;

		if (test !=null)
		{
			
			_RigidBody = test;
		}
	}

	public override void _Process(double delta)
	{
		foreach (Node3D child in _Children)
		{
			if (child != _RigidBody)
			{
				var localPos = child.GlobalPosition - _RigidBody.GlobalPosition;

				child.GlobalPosition = _RigidBody.GlobalPosition+_ChildrenPos[child];
			}
			
		}
    }

	public Vector3 GetPosition()
	{
		return _RigidBody.Position;
	}
}
