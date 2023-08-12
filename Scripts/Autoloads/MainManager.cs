using Godot;
using KayEth.Mancala.Players;
using KayEth.Mancala.Tools;
using KayEth.Mancala.UI;
using System;

namespace KayEth.Mancala.Autoloads
{
    public class MainManager : Node
    {
        public static MainManager Instance { get; private set; }
        public Player playerOne;
        public Player playerTwo;
        public bool random;

        public override void _Ready()
        {
            Instance = this;

            Logger.Info("MainManager ready");

            StoneContainer.StoneTexture = ResourceLoader.Load<Texture>("res://Assets/UI/Stone.png");
        }

        public void PlayGame(Player playerOne, Player playerTwo, bool random)
        {
            this.playerOne = playerOne;
            this.playerTwo = playerTwo;
            this.random = random;

            GetTree().ChangeScene("res://Resources/Scenes/Game.scn");
        }
    }
}

