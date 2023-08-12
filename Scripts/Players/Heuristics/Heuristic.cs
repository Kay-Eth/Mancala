using Godot;
using KayEth.Mancala.Tools;
using System;

namespace KayEth.Mancala.Players.Heuristics
{
    public abstract class Heuristic
    {
        public abstract int EvaluateBoard(int player, MancalaBoardData mancalaBoardData);
    }
}