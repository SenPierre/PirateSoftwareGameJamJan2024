using Godot;
using System;

public partial class MushroomManager : Node2D
{
    public static MushroomManager manager;
    
    [Export] public Texture2D m_maskField;

    private Mushroom m_currentMushroom;
    public float cooldown = 0.5f;

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
        if (cooldown > 0.0f)
        {
            cooldown -= (float)delta;
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void SetCurrentMushroom(Mushroom mush)
    {
        if (m_currentMushroom != null)
        {
            m_currentMushroom.SetSelected(false);
        }

        m_currentMushroom = mush;
        
        if (m_currentMushroom != null)
        {
            m_currentMushroom.SetSelected(true);
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void InputEvent(Node viewport, InputEvent generatedEvent, int shapeIdx)
    {
        InputEventMouse mouseEvent = (InputEventMouse)generatedEvent;
        int buttonLeftPressed = ((int)mouseEvent.ButtonMask) & ((int)MouseButton.Left);
        int buttonRightPressed = ((int)mouseEvent.ButtonMask) & ((int)MouseButton.Right);
        if (m_currentMushroom != null && cooldown <= 0.0f)
        {
            if (buttonRightPressed != 0)
            {
                SetCurrentMushroom(null);
                cooldown = 0.5f;
            }
            else if (Input.IsActionJustPressed("DROP"))
            {
                GD.Print("DROP");
                Vector2 pos = GetViewport().GetMousePosition();
                Color maskColor = m_maskField.GetImage().GetPixel((int)pos.X, (int)pos.Y);
                if (maskColor.R > 0.0)
                {
                    m_currentMushroom.SpawnAnOffspring(pos);
                }
                
                SetCurrentMushroom(null);
                cooldown = 0.5f;
            }
        }
    }
}
