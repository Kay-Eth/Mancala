using Godot;
using KayEth.Mancala.Tools;
using System;

namespace KayEth.Mancala.Players.Heuristics
{
    public class PointsDiffHeuristic : Heuristic
    {
        public override int EvaluateBoard(int player, MancalaBoardData mancalaBoardData)
        {
            return
                MancalaController.GetHoleStonesCount(mancalaBoardData, MancalaController.GetPlayerWellIndex(mancalaBoardData, player))
                - MancalaController.GetHoleStonesCount(mancalaBoardData, MancalaController.GetPlayerWellIndex(mancalaBoardData, MancalaController.GetOpponent(player)));
        }
    }
}