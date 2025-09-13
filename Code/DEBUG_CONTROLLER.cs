using Godot;
using System;

public partial class DEBUG_CONTROLLER : Node3D
{
    public override void _Ready()
    {
        GetNode<Camera3D>("World/Camera3D").MakeCurrent();
    }
}