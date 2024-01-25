using Godot;
using System;

public partial class SoundPlayer : AudioStreamPlayer
{
    [Export] public AudioStream BouipSound;
    [Export] public AudioStream PopSound;
    [Export] public AudioStream PiuobSound;

    public override void _Ready()
    {
        base._Ready();
    }

    public void PlayPop()
    {
        if (Playing == false)
        {
            Stream = PopSound;
            PitchScale = RandomManager.GetFloatRange(0.8f, 1.2f);
            Play();
        }
    }

    public void PlayBouip()
    {
        if (Playing == false)
        {
            Stream = BouipSound;
            PitchScale = RandomManager.GetFloatRange(0.8f, 1.2f);
            Play();
        }
    }

    public void PlayPiuob()
    {
        if (Playing == false)
        {
            Stream = PiuobSound;
            PitchScale = RandomManager.GetFloatRange(0.8f, 1.2f);
            Play();
        }
    }
}
