using Godot;
using KayEth.Mancala.Tools;
using KayEth.Mancala.UI;
using System;

namespace KayEth.Mancala.Players
{
    public class HumanPlayer : Player
    {
        public override string PlayerType => "HUMAN_PLAYER";

        public HumanPlayer() : base()
        {
        }

        public HumanPlayer(int playerId) : base(playerId)
        {
            Logger.Info($"Human Player created. Player id: {playerId}");
        }

        public override void MakeMove(MancalaBoardData mbd)
        {
            InputControl.Instance.RegisterMove(this);
        }
    }
}