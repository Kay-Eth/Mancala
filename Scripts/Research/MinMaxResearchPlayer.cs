using Godot;
using KayEth.Mancala.Players.Heuristics;
using KayEth.Mancala.Tools;
using System;

namespace KayEth.Mancala.Research
{
    public class MinMaxResearchPlayer : ResearchPlayer
    {
        public override string PlayerType => "MinMaxResearchPlayer";

        readonly Heuristic _heuristic;
        readonly int _depth;

        public MinMaxResearchPlayer(int id, Heuristic heuristic, int depth) : base(id)
        {
            _heuristic = heuristic;
            _depth = depth;
        }

        public override int GetMove(MancalaBoardData mbd)
        {
            LogLevel previous = Logger.GetLogLevel();
            Logger.SetLogLevel(LogLevel.Error);

            int maxVal = int.MinValue;
            int nextMove = 0;

            int count = 0;
            foreach (int legalMove in MancalaController.GetLegalMoves(mbd, PlayerId))
            {
                var mancalaBoardData = mbd.Copy();
                var next = MancalaController.MakeMove(mancalaBoardData, PlayerId, legalMove);

                int value = MinMax(mancalaBoardData, false, _depth - 1);
                count++;

                if (value > maxVal)
                {
                    maxVal = value;
                    nextMove = legalMove;
                }
            }
            
            Logger.SetLogLevel(previous);
            return nextMove;
        }

        int MinMax(MancalaBoardData mbd, bool maximize, int depth)
        {
            if (mbd.winner != MancalaBoardData.PLAYER_NONE || depth == 0)
            {
                int val = _heuristic.EvaluateBoard(PlayerId, mbd);
                return val;
            }
            
            if (maximize)
            {
                int value = int.MinValue;
                foreach (var move in MancalaController.GetLegalMoves(mbd, PlayerId))
                {
                    var mancalaBoardData = mbd.Copy();
                    int next = MancalaController.MakeMove(mancalaBoardData, PlayerId, move);
                    value = Math.Max(value, MinMax(mancalaBoardData, next == PlayerId, depth - 1));
                }
                return value;
            }
            else
            {
                int opponent = MancalaController.GetOpponent(PlayerId);
                int value = int.MaxValue;
                foreach (var move in MancalaController.GetLegalMoves(mbd, opponent))
                {
                    var mancalaBoardData = mbd.Copy();
                    int next = MancalaController.MakeMove(mancalaBoardData, opponent, move);
                    value = Math.Min(value, MinMax(mancalaBoardData, next == PlayerId, depth - 1));
                }
                return value;
            }
        }
    }
}