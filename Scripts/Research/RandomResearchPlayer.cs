using Godot;
using KayEth.Mancala.Tools;
using System;

namespace KayEth.Mancala.Research
{
    public class RandomResearchPlayer : ResearchPlayer
    {
        public override string PlayerType => "RandomResearchPlayer";

        public RandomResearchPlayer(int id) : base(id)
        {

        }

        public override int GetMove(MancalaBoardData mbd)
        {
            var moves = MancalaController.GetLegalMoves(mbd, PlayerId);
            var index = GD.Randi() % moves.Length;
            return moves[index];
        }
    }
}