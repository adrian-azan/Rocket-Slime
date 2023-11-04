using Godot;
using System;

public partial class Item : Node3D
{

    public void Touched(Node3D other)
    {
        if (other is Player)
        {
            GD.Print("NO CONSENT");
        }
    }

}
