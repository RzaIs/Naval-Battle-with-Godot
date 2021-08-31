using Godot;
using System;

public class MainScene : Node2D
{
    public Map leftMap;
    public Map rightMap;
    public UserInterface ui;

    public char turn;

    public override void _Ready()
    {
        leftMap = GetNode<Map>("LEFT");
        rightMap = GetNode<Map>("RIGHT");
        ui = GetNode<UserInterface>("UI");
        leftMap.isActive = true;
    }

    public override void _Process(float delta)
    {

    }

    public void NextTurn()
    {
        ui.ChangeSide();

        if(turn == 'R')
        {
            turn = 'L';
            UpdateMap();
            leftMap.isActive = true;
            rightMap.isActive = false;
        }
        else
        {
            turn = 'R';
            UpdateMap();
            rightMap.isActive = true;
            leftMap.isActive = false;
        }
    }

    public void RestartGame()
    {
        leftMap.ClearAllTileMaps();
        rightMap.ClearAllTileMaps();

        leftMap.Init();
        rightMap.Init();

        leftMap.navalMap.Visible = true;
        rightMap.navalMap.Visible = true;

        leftMap.isActive = true;
        rightMap.isActive = false;
        
        ui.lButton.Visible = true;
        ui.rButton.Visible = false;
        
        ui.lLabel.Visible = false;
        ui.rLabel.Visible = false;
    }

    public void UpdateMap()
    {
        rightMap.RevealSinkNavals();
        leftMap.RevealSinkNavals();

        if(rightMap.IsLostGame() || leftMap.IsLostGame())
        {
            rightMap.isActive = false;
            leftMap.isActive = false;
            ui.restart.Visible = true;
            ui.side.Visible = false;
            if (rightMap.IsLostGame())
            {
                ui.lLabel.Visible = true;
            }
            else
            {
                ui.rLabel.Visible = true;
            }
        }
    }

    public void ReadyLeft()
    {
        leftMap.HideNavals();
        leftMap.isActive = false;
        rightMap.isActive = true;
    }

    public void StartGame()
    {
        rightMap.HideNavals();
        leftMap.editMode = false;
        rightMap.editMode = false;
        turn = 'R';

        if (ui.isSideRotated)
            ui.ChangeSide();
    }
}
