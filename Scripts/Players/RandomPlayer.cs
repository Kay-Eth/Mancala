using Godot;
using KayEth.Mancala.Tools;
using System;

namespace KayEth.Mancala.Players
{
    public class RandomPlayer : Player
    {
        public override string PlayerType => "RANDOM_PLAYER";

        public RandomPlayer() : base()
        {
        }

        public RandomPlayer(int playerId) : base(playerId)
        {
            Logger.Info($"Random Player created. Player id: {playerId}");
        }

        public override void MakeMove(MancalaBoardData mbd)
        {
            var moves = MancalaController.GetLegalMoves(mbd, PlayerId);
            var index = GD.Randi() % moves.Length;
            ExecuteMove(moves[index]);
        }
    }
}