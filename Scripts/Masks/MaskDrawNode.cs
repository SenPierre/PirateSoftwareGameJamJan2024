using Godot;
using System;

[Tool]
public partial class MaskDrawNode : Node2D
{
    public float m_radius = 30f;

    private MaskCalculator m_mask;

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _Ready()
    {
        base._Ready();
        m_mask = MaskCalculator.calculator;
        if (m_mask != null)
        {
            m_mask.OnMaskDraw += OnMaskDraw;
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _ExitTree()
    {
        if (m_mask != null)
        {
            m_mask.OnMaskDraw -= OnMaskDraw;
        }
        base._ExitTree();
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void UpdateRadius(float newRadius)
    {
        m_radius = newRadius;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    void OnMaskDraw()
    {
        m_mask.AddMaskDraw(GlobalPosition, m_radius);
    }
}
