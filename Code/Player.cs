
using Godot;
using System;
using System.Reflection.Metadata.Ecma335;


public partial class Player : Node3D
{
	private RigidBody3D _rigidBody;
	private float _acceleration = 50f;
	private float _speed = 4f;
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

		if (Input.IsActionJustReleased("Dash"))
		{
			velocity *= 3.0f; // Jump
			Dash();
		}
	
	 _rigidBody.ApplyCentralForce(velocity);

	//TODO: confirm why linearvelocity is never 0
	_rigidBody.LinearVelocity = _rigidBody.LinearVelocity.LimitLength(_speed);

}

	public async void Dash()
	{
		_speed *= 2;
		await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);		
		_speed /= 2;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_rigidBody = GetNode<RigidBody3D>("RigidBody3D");
		GD.Print(_rigidBody);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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







