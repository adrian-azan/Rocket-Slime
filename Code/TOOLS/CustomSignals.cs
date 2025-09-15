using Godot;
using System;

public partial class CustomSignals : Node
{
    public static CustomSignals _Instance;

    public override void _Ready()
    {
        base._Ready();

        _Instance = this;
    }

    [Signal]
    public delegate void UpdateLightsSignalEventHandler();

    [Signal]
    public delegate void UpdateShowTopSignalEventHandler(bool visible);

    [Signal]
    public delegate void SuccesfulAttackSignalEventHandler(int amount);

    [Signal]
    public delegate void SuccesfulAttackEnemySignalEventHandler();

    [Signal]
    public delegate void RecoverStaminaSignalEventHandler();
}