using Godot;
using System;
using static System.Net.Mime.MediaTypeNames;

public partial class promptable : Node3D
{
	[ExportGroup("Prompt")]
	[Export(PropertyHint.File)]
	public Resource JsonFile;

	public Node3D _Accent;

	bool _isBeingPercieved = false;
	Rigid_Body _rb;


	public override void _Ready()
	{
		_rb = Tools.FindRigidBodyFromRoot(this);
		if (_rb == null)
		{
			GD.Print(ToString() + " has no rigid body");
		}
    }

	public override void _Process(double delta)
	{
	   if (_isBeingPercieved && Input.IsActionJustPressed("Interact"))
		{
			//FilePath.
			var file = FileAccess.Open(JsonFile.ResourcePath, FileAccess.ModeFlags.Read);
			 var content = Json.ParseString(file.GetAsText());

		}
	   
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
		return "promptable";//file.GetParsedText();
	}
}
