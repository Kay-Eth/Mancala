using Godot;
using KayEth.Mancala.Players.Heuristics;
using KayEth.Mancala.Tools;
using System;

namespace KayEth.Mancala.Research
{
    public class AlphaBetaResearchPlayer : ResearchPlayer
    {
        public override string PlayerType => "AlphaBetaResearchPlayer";

        readonly Heuristic _heuristic;
        readonly int _depth;

        public AlphaBetaResearchPlayer(int id, Heuristic heuristic, int depth) : base(id)
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

            int alpha = int.MinValue;
            int beta = int.MaxValue;

            foreach (int legalMove in MancalaController.GetLegalMoves(mbd, PlayerId))
            {
                var mancalaBoardData = mbd.Copy();
                var next = MancalaController.MakeMove(mancalaBoardData, PlayerId, legalMove);

                int value = AlphaBeta(mancalaBoardData, next == PlayerId, _depth - 1, alpha, beta, next == PlayerId);

                if (value > maxVal)
                {
                    maxVal = value;
                    nextMove = legalMove;
                }
            }

            Logger.SetLogLevel(previous);
            return nextMove;
        }

        int AlphaBeta(MancalaBoardData mbd, bool maximize, int depth, int alpha, int beta, bool skip = false)
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
                    value = Math.Max(value, AlphaBeta(mancalaBoardData, next == PlayerId, depth - 1, alpha, beta));
                    // value = Math.Max(value, AlphaBeta(mancalaBoardData, false, depth - 1, alpha, beta, next == PlayerId));
                    alpha = Math.Max(alpha, value);
                    if (beta <= alpha)
                    {
                        break;
                    }
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
                    value = Math.Min(value, AlphaBeta(mancalaBoardData, next == PlayerId, depth - 1, alpha, beta));
                    // value = Math.Min(value, AlphaBeta(mancalaBoardData, true, depth - 1, alpha, beta, next != PlayerId));
                    beta = Math.Min(beta, value);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
                return value;
            }
        }
    }
}