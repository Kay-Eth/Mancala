using Godot;
using KayEth.Mancala.Tools;
using System;

namespace KayEth.Mancala.Players.Heuristics
{
    public class MovesDiffCountHeuristic : Heuristic
    {
        public override int EvaluateBoard(int player, MancalaBoardData mancalaBoardData)
        {
            return MancalaController.MovesCountOfPlayer(mancalaBoardData, player) - MancalaController.MovesCountOfPlayer(mancalaBoardData, MancalaController.GetOpponent(player));
        }
    }
}