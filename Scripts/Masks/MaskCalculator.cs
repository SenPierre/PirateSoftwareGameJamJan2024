using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class MaskDraw {
    public MaskDraw(float r, Vector2 p) {radius = r; pos = p;}
   public float radius;
   public Vector2 pos; 
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
            DrawCircle(mask.pos, mask.radius, new Color(1,1,1,1));
        }
    }

    //----------------------------------------------------------
    //
    //----------------------------------------------------------
    public void AddMaskDraw(Vector2 pos, float radius)
    {
        maskPos.Add(new MaskDraw(radius, pos));
    }
}
