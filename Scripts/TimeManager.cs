using Godot;
using System;

[Tool]
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
        g_Manager = this;
        base._Ready();
	}

	// -----------------------------------------------------------------
	//
	// -----------------------------------------------------------------
    public void Pause()
    {
        m_TimeMult = 0.0f;
    }

	// -----------------------------------------------------------------
	//
	// -----------------------------------------------------------------
    public void Unpause()
    {
        m_TimeMult = 1.0f;
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
