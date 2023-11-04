using Godot;
using System;
using System.Collections;

public partial class Tank_Brain : Node3D
{

	ArrayList _Chutes;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_Chutes = new ArrayList();
		FindNodes("./",0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		foreach (Chute chute in _Chutes)
		{
			if (chute._Prepared)
			{
                chute.SpawnItem();
            }
		}
	}


	public void FindNodes(string path, int limit)
	{
		var root = GetNode(path);
		var nodes = root.GetChildren();
		if (limit >= 6 || nodes.Count == 0)
		{
			return;
		}
		
		foreach (var node in nodes)
		{
			var chute = node.GetNodeOrNull<Chute>(path);
			string name = node.Name.ToString();
			if (node is Chute)
			{
				_Chutes.Add(node);
			}

			FindNodes(path+"/"+node.Name, limit +1);
		}
	}
}
