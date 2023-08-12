using Godot;
using KayEth.Mancala.Players.Heuristics;
using KayEth.Mancala.Tools;
using System;
using System.Collections.Generic;

namespace KayEth.Mancala.Players
{
    public class AlphaBetaPlayer : Player
    {
        [Signal]
        public delegate void FinishedCalculation(double time);

        public override string PlayerType => "ALPHA_BETA_PLAYER";

        readonly Heuristic _heuristic;
        readonly int _depth;

        System.Threading.Thread _moveThread;

        public AlphaBetaPlayer() : base()
        {
        }

        public AlphaBetaPlayer(int playerId, Heuristic heuristic, int depth) : base(playerId)
        {
            _heuristic = heuristic;
            _depth = depth;

            Logger.Info($"AlphaBeta Player created. Player id: {playerId}, Depth: {depth}, Heuristic: {heuristic.GetType().Name}");
        }

        public override void MakeMove(MancalaBoardData mbd)
        {
            _moveThread = new System.Threading.Thread(() => this.MakeMoveInThread(mbd));
            _moveThread.Start();
        }

        public void PassMove(int hole)
        {
            _moveThread.Join();
            _moveThread = null;
            ExecuteMove(hole);
        }

        // public void MakeMoveInThread(MancalaBoardData mbd)
        // {
        //     LogLevel previous = Logger.GetLogLevel();
        //     Logger.SetLogLevel(LogLevel.Error);

        //     int maxVal = int.MinValue;
        //     List<int> nextMoves = new List<int>();

        //     int alpha = int.MinValue;
        //     int beta = int.MaxValue;

        //     foreach (int legalMove in MancalaController.GetLegalMoves(mbd, PlayerId))
        //     {
        //         var mancalaBoardData = mbd.Copy();
        //         var next = MancalaController.MakeMove(mancalaBoardData, PlayerId, legalMove);

        //         var watch = System.Diagnostics.Stopwatch.StartNew();
        //         int value = AlphaBeta(mancalaBoardData, next == PlayerId, _depth - 1, alpha, beta, next == PlayerId);

        //         // GD.PrintRaw($"{legalMove}: {value} -");

        //         if (value > maxVal)
        //         {
        //             // GD.Print(" >");
        //             maxVal = value;
        //             nextMoves = new List<int>();
        //             nextMoves.Add(legalMove);
        //         }
        //         else if (value == maxVal)
        //         {
        //             // GD.Print(" ==");
        //             nextMoves.Add(legalMove);
        //         }
        //         else
        //         {
        //             // GD.Print(" <");
        //         }
        //         // alpha = Math.Max(alpha, maxVal);

        //         watch.Stop();
        //         EmitSignal(nameof(FinishedCalculation), watch.Elapsed.TotalMilliseconds);
        //     }

        //     Logger.SetLogLevel(previous);

        //     // ExecuteMove(nextMove);
        //     Random rand = new Random();
        //     this.CallDeferred("PassMove", nextMoves[rand.Next(nextMoves.Count)]);
        // }

        public void MakeMoveInThread(MancalaBoardData mbd)
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

                var watch = System.Diagnostics.Stopwatch.StartNew();
                int value = AlphaBeta(mancalaBoardData, next == PlayerId, _depth - 1, alpha, beta, next == PlayerId);

                // GD.PrintRaw($"{legalMove}: {value} -");

                if (value > maxVal)
                {
                    maxVal = value;
                    nextMove = legalMove;
                }
                // alpha = Math.Max(alpha, maxVal);

                watch.Stop();
                EmitSignal(nameof(FinishedCalculation), watch.Elapsed.TotalMilliseconds);
            }

            Logger.SetLogLevel(previous);

            // ExecuteMove(nextMove);
            Random rand = new Random();
            this.CallDeferred("PassMove", nextMove);
        }

        int AlphaBeta(MancalaBoardData mbd, bool maximize, int depth, int alpha, int beta, bool skip = false)
        {
            if (mbd.winner != MancalaBoardData.PLAYER_NONE || depth == 0)
            {
                int val = _heuristic.EvaluateBoard(PlayerId, mbd);
                return val;
            }

            // if (skip)
            // {
            //     if (maximize)
            //     {
            //         int value = AlphaBeta(mbd.Copy(), false, depth - 1, alpha, beta, false);
            //         alpha = Math.Max(alpha, value);
            //         return value;
            //     }
            //     else
            //     {
            //         int value = AlphaBeta(mbd.Copy(), true, depth - 1, alpha, beta, false);
            //         beta = Math.Min(beta, value);
            //         return value;
            //     }
            // }
            
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