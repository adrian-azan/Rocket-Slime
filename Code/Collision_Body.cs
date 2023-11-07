using Godot;
using System;

public partial class Collision_Body : Node3D
{
	public Node3D _Accent;

	bool _isBeingPercieved = false;
	Rigid_Body _rb;

	Area3D _Area;


	public override void _Ready()
	{
		_rb = Tools.FindRigidBodyFromRoot(this);
		if (_rb == null)
		{
			GD.Print(ToString() + " has no rigid body");
		}
		_Area = GetNodeOrNull<Area3D>("Area3D");
    }

	public override void _Process(double delta)
	{
	   if (_isBeingPercieved && Input.IsActionJustPressed("Interact"))
		{

		}	   
	}

	public void Enable()
	{
		_Area.ProcessMode = ProcessModeEnum.Inherit;
    }

	public void Disable()
	{
        _Area.ProcessMode = ProcessModeEnum.Disabled;
    }

	public void BeingPercieved(Node3D other)
	{
		var target = other.GetOwnerOrNull<Node3D>();

		if (target is Player)
		{
			GD.Print("PERCIEVED {}",other);    
		}

	}
	  public void NotBeingPercieved(Node3D other)
	{
		var target = other.GetOwnerOrNull<Node3D>();

		if (target.Name == "Player")
		{         
			_isBeingPercieved = false;
			var  test = (Player)target;


		}
   
	}

	public override string ToString()
	{
		return "Collision_Body";//file.GetParsedText();
	}
}


