using Godot;
using Godot.Collections;

public partial class Tank_Brain : Node3D
{
    private Array<Node> _Chutes;
    private Array<Path3D> _Paths;

    private bool fire = true;

    public override void _Ready()
    {
        _Chutes = GetTree().GetNodesInGroup("Chute");
        _Paths = Tools.GetChildren<Path3D>(this);
        Array<Vector3> points = new Array<Vector3>();

        foreach (var path in _Paths)
        {
            for (int i = 0; i < path.Curve.PointCount; i++)
            {
                var first = path.Curve.Sample(i, .5f);
                GD.Print($"{i} {first}");
                first.X += Tools.rng.RandfRange(-1f, 1f);
                first.Z += Tools.rng.RandfRange(-1f, 1f);

                points.Add(first);
            }

            GD.Print("\n\n");

            for (int i = 0; i < path.Curve.PointCount; i++)
            {
                GD.Print($"{i} {path.Curve.GetPointPosition(i)}");
            }
            GD.Print("\n\n");

            int j = 1;
            for (int i = 0; i < points.Count; i++)
            {
                path.Curve.AddPoint(points[i], null, null, j);
                j += 2;
            }
            GD.Print("\n\n");

            for (int i = 0; i < path.Curve.PointCount; i++)
            {
                GD.Print($"{i} {path.Curve.GetPointPosition(i)}");
            }
            GD.Print("\n\n");

            var visual = ResourceLoader.Load("res://Scenes/Environment/Tank Pieces/PathVisual.tscn") as PackedScene;

            for (float i = 0; i < 400; i++)
            {
                var test = visual.Instantiate<PathFollow3D>();
                path.AddChild(test);

                test.ProgressRatio = i / 400;
            }

            points.Clear();
        }
    }

    public override void _Process(double delta)
    {
        if (fire)
        {
            foreach (Chute chute in _Chutes)
            {
                if (chute._Prepared)
                {
                    //          chute.SpawnItem();
                }
                fire = false;
            }
        }
    }
}