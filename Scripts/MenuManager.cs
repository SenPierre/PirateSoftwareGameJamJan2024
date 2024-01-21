using Godot;
using System;

public partial class MenuManager : Node2D
{
    [Export] public Node2D mainPanel;
    [Export] public Node2D jamPanel;

    void StartGame()
    {
        GameManager.manager.GoToGame();
    }

    void QuitGame()
    {
        GameManager.manager.Quit();
    }

    void DisplayJam()
    {
        mainPanel.Visible = false;
        jamPanel.Visible = true;
    }

    void HideJam()
    {
        mainPanel.Visible = true;
        jamPanel.Visible = false;
    }
}
