using Godot;
using KayEth.Mancala.Tools;
using System;

namespace KayEth.Mancala.UI
{
    public class ResultDialog : PopupDialog
    {
        public void Configure(string player0, string player1, MancalaBoardData mbd)
        {
            GetNode<Label>("Margin/PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/PlayerLabel").Text = player0;
            GetNode<Label>("Margin/PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer2/PlayerLabel").Text = player1;

            GetNode<Label>("Margin/PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/ScoreLabel").Text = GD.Str(
                MancalaController.GetHoleStonesCount(mbd, MancalaController.GetPlayerWellIndex(mbd, 0))
            );

            GetNode<Label>("Margin/PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer2/ScoreLabel").Text = GD.Str(
                MancalaController.GetHoleStonesCount(mbd, MancalaController.GetPlayerWellIndex(mbd, 1))
            );
        }

        public void OnButtonDown()
        {
            GetNode<AudioStreamPlayer>("ButtonClick").Play();
        }
    }
}