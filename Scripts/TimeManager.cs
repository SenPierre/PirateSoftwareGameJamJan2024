using Godot;
using System;

public partial class TimeManager : Node2D
{
    // STATIC PART ----------------------------
    static TimeManager g_Manager;

    static public TimeManager GetManager()
    {
        return g_Manager;
    } 
    static public float GetDeltaTime()
    {
        return g_Manager.m_FrameDelta;
    }
    // STATIC PART ----------------------------

    float m_FrameDelta = 0.0f;
    float m_TimeMult = 1.0f;

	// -----------------------------------------------------------------
	// Called when the node enters the scene tree for the first time.
	// -----------------------------------------------------------------
	public override void _Ready()
	{
        GD.Print("Huh");
        g_Manager = this;
        base._Ready();
	}

	// -----------------------------------------------------------------
	//
	// -----------------------------------------------------------------
    public override void _Process(double delta)
    {
        base._Process(delta);
        m_FrameDelta = (float)delta * m_TimeMult;
    }
}
