using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class MaskDraw {
    public MaskDraw(float r, Vector2 p, Color c) {radius = r; pos = p; color = c;}
   public float radius;
   public Vector2 pos; 
   public Color color;
}

[Tool]
public partial class MaskCalculator : Node2D
{
    static public MaskCalculator calculator;

    List<MaskDraw> maskPos = new List<MaskDraw>();

    [Signal]
    public delegate void OnMaskDrawEventHandler();

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    MaskCalculator()
    {
        calculator = this;
    }

    public override void _Ready()
    {
        base._Ready();
        GetParent<SubViewport>().RenderTargetUpdateMode = SubViewport.UpdateMode.WhenParentVisible;
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _Process(double delta)
    {
        base._Process(delta);
        QueueRedraw();
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public override void _Draw()
    {
        base._Draw();

        maskPos.Clear();
        EmitSignal(SignalName.OnMaskDraw);

        foreach(MaskDraw mask in maskPos)
        {
            DrawCircle(mask.pos, mask.radius, mask.color);
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void AddMaskDraw(Vector2 pos, float radius, Color color)
    {
        maskPos.Add(new MaskDraw(radius, pos, color));
    }
}
