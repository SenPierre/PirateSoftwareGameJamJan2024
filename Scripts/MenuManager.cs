using Godot;
using System;

public partial class MenuManager : Node2D
{
    [Export] public Node2D mainPanel;
    [Export] public Node2D jamPanel;
    [Export] public Node2D levelPanel;

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

    void DisplayLevel()
    {
        mainPanel.Visible = false;
        levelPanel.Visible = true;
    }

    void HideLevel()
    {
        mainPanel.Visible = true;
        levelPanel.Visible = false;
    }
}
