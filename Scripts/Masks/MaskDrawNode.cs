using Godot;
using System;

[Tool]
public partial class MaskDrawNode : Node2D
{
    private float m_radius = 30.0f;
    private Color m_color = new Color(0.0f, 0.0f, 0.0f, 0.0f);

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
    public void UpdateColor(Color newcolor)
    {
        m_color = newcolor;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    void OnMaskDraw()
    {
        m_mask.AddMaskDraw(GlobalPosition, m_radius, m_color);
    }
}
