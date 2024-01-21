using Godot;
using System;

public partial class MenuButton : TextureRect
{
    [Export] public AnimationPlayer anim;

    public void OnEnter()
    {
        anim.Play("Over");
    }
    public void OnExit()
    {
        anim.Play("Idle");
    }
    public void OnClick(InputEvent ev)
    {
        InputEventMouse mouseEvent = (InputEventMouse)ev;
        int buttonLeftPressed = ((int)mouseEvent.ButtonMask) & ((int)MouseButton.Left);
        if (buttonLeftPressed != 0)
        {
            anim.Play("Selected");
        }
    }
}
