using Godot;
using KayEth.Mancala.Players.Heuristics;
using KayEth.Mancala.Tools;
using System;
using System.Collections.Generic;

namespace KayEth.Mancala.Players
{
    public class MinMaxPlayer : Player
    {
        [Signal]
        public delegate void FinishedCalculation(double time);
        
        public override string PlayerType => "MIN_MAX_PLAYER";
        
        readonly Heuristic _heuristic;
        readonly int _depth;

        System.Threading.Thread _moveThread;

        public MinMaxPlayer() : base()
        {
        }

        public MinMaxPlayer(int playerId, Heuristic heuristic, int depth) : base(playerId)
        {
            _heuristic = heuristic;
            _depth = depth;

            Logger.Info($"MinMax Player created. Player id: {playerId}, Depth: {depth}, Heuristic: {heuristic.GetType().Name}");
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

        //     int count = 0;
        //     foreach (int legalMove in MancalaController.GetLegalMoves(mbd, PlayerId))
        //     {
        //         var mancalaBoardData = mbd.Copy();
        //         var next = MancalaController.MakeMove(mancalaBoardData, PlayerId, legalMove);

        //         var watch = System.Diagnostics.Stopwatch.StartNew();
        //         int value = MinMax(mancalaBoardData, false, _depth - 1, next == PlayerId);
        //         count++;

        //         if (value > maxVal)
        //         {
        //             maxVal = value;
        //             nextMoves = new List<int>();
        //             nextMoves.Add(legalMove);
        //         }
        //         else if (value == maxVal)
        //         {
        //             nextMoves.Add(legalMove);
        //         }

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

            int count = 0;
            foreach (int legalMove in MancalaController.GetLegalMoves(mbd, PlayerId))
            {
                var mancalaBoardData = mbd.Copy();
                var next = MancalaController.MakeMove(mancalaBoardData, PlayerId, legalMove);

                var watch = System.Diagnostics.Stopwatch.StartNew();
                int value = MinMax(mancalaBoardData, false, _depth - 1, next == PlayerId);
                count++;

                if (value > maxVal)
                {
                    maxVal = value;
                    nextMove = legalMove;
                }

                watch.Stop();
                EmitSignal(nameof(FinishedCalculation), watch.Elapsed.TotalMilliseconds);
            }
            
            Logger.SetLogLevel(previous);

            // ExecuteMove(nextMove);
            Random rand = new Random();
            this.CallDeferred("PassMove", nextMove);
        }

        int MinMax(MancalaBoardData mbd, bool maximize, int depth, bool skip = false)
        {
            if (mbd.winner != MancalaBoardData.PLAYER_NONE || depth == 0)
            {
                int val = _heuristic.EvaluateBoard(PlayerId, mbd);
                return val;
            }

            // if (skip)
            // {
            //     return MinMax(mbd, !maximize, depth - 1);
            // }
            
            if (maximize)
            {
                int value = int.MinValue;
                foreach (var move in MancalaController.GetLegalMoves(mbd, PlayerId))
                {
                    var mancalaBoardData = mbd.Copy();
                    int next = MancalaController.MakeMove(mancalaBoardData, PlayerId, move);
                    value = Math.Max(value, MinMax(mancalaBoardData, next == PlayerId, depth - 1, next == PlayerId));
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
                    value = Math.Min(value, MinMax(mancalaBoardData, next == PlayerId, depth - 1, next != PlayerId));
                }
                return value;
            }
        }

        // int MinMax(MancalaBoardData mbd, bool maximize, int depth)
        // {
        //     if (mbd.winner != MancalaBoardData.PLAYER_NONE || depth == 0)
        //     {
        //         int val = _heuristic.EvaluateBoard(PlayerId, mbd);
        //         return val;
        //     }
            
        //     if (maximize)
        //     {
        //         int value = int.MinValue;
        //         foreach (var move in MancalaController.GetLegalMoves(mbd, PlayerId))
        //         {
        //             var mancalaBoardData = mbd.Copy();
        //             int next = MancalaController.MakeMove(mancalaBoardData, PlayerId, move);
        //             value = Math.Max(value, MinMax(mancalaBoardData, next == PlayerId, depth - 1));
        //         }
        //         return value;
        //     }
        //     else
        //     {
        //         int opponent = MancalaController.GetOpponent(PlayerId);
        //         int value = int.MaxValue;
        //         foreach (var move in MancalaController.GetLegalMoves(mbd, opponent))
        //         {
        //             var mancalaBoardData = mbd.Copy();
        //             int next = MancalaController.MakeMove(mancalaBoardData, opponent, move);
        //             value = Math.Min(value, MinMax(mancalaBoardData, next == PlayerId, depth - 1));
        //         }
        //         return value;
        //     }
        // }
    }
}