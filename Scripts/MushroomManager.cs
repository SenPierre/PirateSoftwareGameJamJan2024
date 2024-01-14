using Godot;
using System;

public partial class MushroomManager : Node2D
{
    public static MushroomManager manager;
    
    [Export] public Texture2D m_maskField;

    public Mushroom m_currentMushroom;

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _Ready()
    {
        manager = this;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Engine.IsEditorHint() == false)
        {
            if (m_currentMushroom != null)
            {
                if (((int)Input.GetMouseButtonMask() & (int)MouseButtonMask.Right) != 0)
                {
                    m_currentMushroom = null;
                }
                else if (((int)Input.GetMouseButtonMask() & (int)MouseButtonMask.Left) != 0)
                {
                    Vector2 pos = GetViewport().GetMousePosition();
                    Color maskColor = m_maskField.GetImage().GetPixel((int)pos.X, (int)pos.Y);
                    if (maskColor.R > 0.0)
                    {
                        m_currentMushroom.SpawnAnOffspring(pos);
                    }
                    
                    m_currentMushroom = null;
                }
            }
        }
    }
}
