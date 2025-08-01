using Godot;
using System;

public partial class ThrowWall : Node3D
{
    public override void _Ready()
    {
    }

    public void ForceThrow(Node other)
    {
        LilDude lilBuddy = Tools.GetRoot<LilDude>(other) as LilDude;
        if (lilBuddy != null)
        {
            lilBuddy.Throw((Vector3.Right * .35f) + (Vector3.Up * .8f));
        }
    }
}