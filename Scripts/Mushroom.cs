using Godot;
using System;

[Tool]
public partial class Mushroom : Node2D
{
    [Export] public float m_BaseRadius
    {
      get => m_pBaseRadius;
      set => SetBaseRadius(value);
    }
    
    private MaskDrawNode m_Mask;
    private float m_pBaseRadius;

    private float m_Radius = 30.0f;

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _Ready()
    {
        m_Mask = GetNode<MaskDrawNode>("MaskDrawer");
        base._Ready();
        UpdateRadius(m_pBaseRadius);
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    private void SetBaseRadius(float val)
    {
        m_pBaseRadius = val;
        UpdateRadius(m_pBaseRadius);
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void UpdateRadius(float newRadius)
    {
        m_Radius = newRadius;
        if (m_Mask != null) // set of members are done before the _ready.so we need to test the mask validity
        {
            m_Mask.UpdateRadius(m_Radius);
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void InputEvent(Node vp, InputEvent ev, int shapeIndx)
    {
        InputEventMouse mouseEvent = (InputEventMouse)ev;

        if ((((int)mouseEvent.ButtonMask) & ((int)MouseButton.Left)) != 0)
        {
            // Shroom selected
            MushroomManager.manager.m_currentMushroom = this;
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void SpawnAnOffspring(Vector2 pos)
    {
        PackedScene shroomPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Mushroom.tscn");
        
        Mushroom newShroom = shroomPrefab.Instantiate<Mushroom>();
        GetOwner<Node2D>().AddChild(newShroom);
        newShroom.GlobalPosition = pos;
        m_Radius /= 2.0f;
        newShroom.UpdateRadius(m_Radius);
    }
}
