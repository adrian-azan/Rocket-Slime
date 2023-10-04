using Godot;
using System;
using static System.Net.Mime.MediaTypeNames;

public partial class promptable : Node
{
	[ExportGroup("Prompt")]
	[Export(PropertyHint.File)]
	public Resource JsonFile;

	public Node3D _Accent;

	bool _isBeingPercieved = false;
	public override void _Process(double delta)
	{
	   if (_isBeingPercieved && Input.IsActionJustPressed("Interact"))
		{
			//FilePath.
			var file = FileAccess.Open(JsonFile.ResourcePath, FileAccess.ModeFlags.Read);
			 var content = Json.ParseString(file.GetAsText());
			GD.Print(content.AsGodotDictionary()["details"]);

		   // GD.Print(FilePath.()["details"]);
		}
	   
	   
	}

	public void BeingPercieved(Node3D other)
	{
		var target = other.GetOwnerOrNull<Node3D>();

		if (target.Name == "Player")
		{
			_isBeingPercieved = true;
			var  test = (Player)target;
			test.Check(true);
			//GD.Print("PLAYER {}",other);    
		}

	}
	  public void NotBeingPercieved(Node3D other)
	{
		var target = other.GetOwnerOrNull<Node3D>();

		if (target.Name == "Player")
		{         
			_isBeingPercieved = false;
						var  test = (Player)target;

						test.Check(false);

			//GD.Print("PLAYER {}",other);    
		}
   
	}

	public override string ToString()
	{
		return "aass";//file.GetParsedText();
	}
}
