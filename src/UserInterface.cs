using Godot;
using System;

public class UserInterface : Control
{
    public MainScene parentMain;
    public Button rButton;
    public Button lButton;
    public Button restart;
    public Timer rTimer;
    public Timer lTimer;
    public Label rLabel;
    public Label lLabel;
    public Sprite side;

    public bool isSideRotated;

    public override void _Ready()
    {
        parentMain = GetParent<MainScene>();
        rButton = GetNode<Button>("RButton");
        lButton = GetNode<Button>("LButton");
        restart = GetNode<Button>("Restart");
        rTimer = GetNode<Timer>("RTimer");
        lTimer = GetNode<Timer>("LTimer");
        rLabel = GetNode<Label>("RLabel");
        lLabel = GetNode<Label>("LLabel");
        side = GetNode<Sprite>("Side");

        isSideRotated = false;
    }

    public void ChangeSide()
    {
        if(isSideRotated)
        {
            side.Rotation = 0;
            isSideRotated = false;
        }
        else
        {
            side.Rotation = (float)Math.PI;
            isSideRotated = true;
        }
    }

    public void OnRestartPressed()
    {
        restart.Visible = false;
        parentMain.RestartGame();
    }

    public void OnLButtonPressed()
    {
        if (parentMain.leftMap.IsReady())
        {
            rButton.Visible = true;
            lButton.Visible = false;
            parentMain.ReadyLeft();
        }
        else
            lTimer.Start();
    }

    public void OnRButtonPressed()
    {
        if (parentMain.rightMap.IsReady())
        {
            rButton.Visible = false;
            side.Visible = true;
            parentMain.StartGame();
        }
        else
            rTimer.Start();
    }

    public void OnLTimerTimeout()
    {
        parentMain.leftMap.UnshowUnreadyNavals();
    }
    public void OnRTimerTimeout()
    {
        parentMain.rightMap.UnshowUnreadyNavals();
    }
}
