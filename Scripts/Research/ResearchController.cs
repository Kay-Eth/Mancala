using Godot;
using KayEth.Mancala.Players.Heuristics;
using KayEth.Mancala.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace KayEth.Mancala.Research
{
    public class ResearchController : Control
    {
        [Export] public NodePath OutputRichTextLabelNodePath { get; set; }

        [Export] public NodePath Player1TypeNodePath { get; set; }
        [Export] public NodePath Player1DepthNodePath { get; set; }
        [Export] public NodePath Player1HeuristicNodePath { get; set; }

        [Export] public NodePath Player2TypeNodePath { get; set; }
        [Export] public NodePath Player2DepthNodePath { get; set; }
        [Export] public NodePath Player2HeuristicNodePath { get; set; }

        [Export] public NodePath FirstMoveRandomNodePath { get; set; }

        [Export] public NodePath PlayButtonNodePath { get; set; }
        [Export] public NodePath ResetButtonNodePath { get; set; }

        public RichTextLabel OutputRTL { get { return GetNode<RichTextLabel>(OutputRichTextLabelNodePath); } }

        public OptionButton Player1TypeOB { get { return GetNode<OptionButton>(Player1TypeNodePath); } }
        public SpinBox Player1DepthSB { get { return GetNode<SpinBox>(Player1DepthNodePath); } }
        public OptionButton Player1HeuristicOB { get { return GetNode<OptionButton>(Player1HeuristicNodePath); } }

        public OptionButton Player2TypeOB { get { return GetNode<OptionButton>(Player2TypeNodePath); } }
        public SpinBox Player2DepthSB { get { return GetNode<SpinBox>(Player2DepthNodePath); } }
        public OptionButton Player2HeuristicOB { get { return GetNode<OptionButton>(Player2HeuristicNodePath); } }

        public CheckButton FirstMoveRandomCB { get { return GetNode<CheckButton>(FirstMoveRandomNodePath); } }

        public Button PlayButton { get { return GetNode<Button>(PlayButtonNodePath); } }
        public Button ResetButton { get { return GetNode<Button>(ResetButtonNodePath); } }

        bool _stopPlay = false;

        ResearchPlayer _player1;
        ResearchPlayer _player2;

        MancalaBoardData _board;
        System.Threading.Thread _thread;

        List<double> _player1Times;
        List<double> _player2Times;

        public override void _Ready()
        {
            ProjectSettings.SetSetting("input_devices/pointing/emulate_touch_from_mouse", false);
            GetTree().SetScreenStretch(SceneTree.StretchMode.Disabled, SceneTree.StretchAspect.Ignore, Vector2.Zero);
        }

        public void PlayGame()
        {
            Reset();

            _stopPlay = false;

            _player1 = CreatePlayer(
                0,
                Player1TypeOB.Selected,
                (int)Player1DepthSB.Value,
                Player1HeuristicOB.Selected
            );

            _player2 = CreatePlayer(
                1,
                Player2TypeOB.Selected,
                (int)Player2DepthSB.Value,
                Player2HeuristicOB.Selected
            );

            _player1Times = new List<double>();
            _player2Times = new List<double>();

            PrintInfo("Creating board data (6, 4)...");
            _board = new MancalaBoardData(6, 4);

            PrintInfo("Starting thread...\n");
            _thread = new System.Threading.Thread(() => GameThread(FirstMoveRandomCB.Pressed));
            _thread.Start();
        }

        public void GameThread(bool firstMoveRandom)
        {
            PrintInfo("Game Start");

            int next = 0;

            if  (firstMoveRandom)
            {
                Random random = new Random();
                int move = random.Next(6);
                PrintInfo($"Random move. PlayerId: {0}, Hole: {move}");
                next = MancalaController.MakeMove(_board, 0, move);

                // Random random = new Random();
                // int move = 5;
                // PrintInfo($"Random move. PlayerId: {0}, Hole: {move}");
                // next = MancalaController.MakeMove(_board, 0, move);
            }

            while (!_stopPlay)
            {
                if (next == 0)
                {
                    PrintInfo("[color=#b2eded]Move of Player 1...[/color]\t", false);
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    int move = _player1.GetMove(_board.Copy());
                    watch.Stop();
                    var time = watch.Elapsed.TotalMilliseconds;
                    _player1Times.Add(time);

                    next = MancalaController.MakeMove(_board, 0, move);
                    PrintInfo($"PlayerId: {0}, Hole: {move}, Score: {MancalaController.GetHoleStonesCount(_board, MancalaController.GetPlayerWellIndex(_board, 0))} - {MancalaController.GetHoleStonesCount(_board, MancalaController.GetPlayerWellIndex(_board, 1))}, Time: {time}ms");
                }
                else
                {
                    PrintInfo("[color=#ff82ac]Move of Player 2...[/color]\t", false);
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    int move = _player2.GetMove(_board.Copy());
                    watch.Stop();
                    var time = watch.Elapsed.TotalMilliseconds;
                    _player2Times.Add(time);

                    next = MancalaController.MakeMove(_board, 1, move);
                    PrintInfo($"PlayerId: {1}, Hole: {move}, Score: {MancalaController.GetHoleStonesCount(_board, MancalaController.GetPlayerWellIndex(_board, 0))} - {MancalaController.GetHoleStonesCount(_board, MancalaController.GetPlayerWellIndex(_board, 1))}, Time: {time}ms");
                }

                if (_board.winner != -1)
                    _stopPlay = true;
            }

            CallDeferred(nameof(EndGame));
        }

        public void EndGame()
        {
            PlayButton.Disabled = false;

            _thread.Join();
            _thread = null;
            PrintInfo($"\nEnd of game. Winner: {_board.winner}, Score: {MancalaController.GetHoleStonesCount(_board, MancalaController.GetPlayerWellIndex(_board, 0))} - {MancalaController.GetHoleStonesCount(_board, MancalaController.GetPlayerWellIndex(_board, 1))}\n");
            PlayButton.Disabled = false;

            PrintInfo("[color=#b2eded]Player1 Stats:[/color]");
            PrintInfo($"Score: {MancalaController.GetHoleStonesCount(_board, MancalaController.GetPlayerWellIndex(_board, 0))}");
            PrintInfo($"Moves count: {MancalaController.MovesCountOfPlayer(_board, 0)}");
            PrintInfo($"Avg. time: {_player1Times.Sum() / _player1Times.Count}");
            PrintInfo("Times:");

            foreach (var time in _player1Times)
            {
                PrintInfo($"[right]{time}[/right]");
            }

            PrintInfo("[color=#ff82ac]Player2 Stats:[/color]");
            PrintInfo($"Score: {MancalaController.GetHoleStonesCount(_board, MancalaController.GetPlayerWellIndex(_board, 1))}");
            PrintInfo($"Moves count: {MancalaController.MovesCountOfPlayer(_board, 1)}");
            PrintInfo($"Avg. time: {_player2Times.Sum() / _player2Times.Count}");
            PrintInfo("Times:");
            
            foreach (var time in _player2Times)
            {
                PrintInfo($"[right]{time}[/right]");
            }
        }

        public void Reset()
        {
            _player1 = null;
            _player2 = null;
            _board = null;

            OutputRTL.Clear();
        }

        public void OnPlayButtonPressed()
        {
            PlayGame();
            PlayButton.Disabled = true;
        }

        public void OnResetButtonPressed()
        {
            if (_thread != null)
            {
                PrintInfo("Stopping game...");
                _stopPlay = true;
            }
        }

        public void PrintInfo(string message, bool newline = true)
        {
            Logger.Info(message);
            if (newline)
                OutputRTL.AppendBbcode(message + "\n");
            else
                OutputRTL.AppendBbcode(message);
        }

        public ResearchPlayer CreatePlayer(int playerId, int playerType, int depth, int heuristicType)
        {
            PrintInfo($"Create player. PlayerId: {playerId}, PlayerType: {playerType}, HeuristicType: {heuristicType}, Depth: {depth}");
            switch (playerType)
            {
                case 0: // RANDOM PLAYER
                    return new RandomResearchPlayer(playerId);
                case 1: // MINIMAX PLAYER
                    Heuristic min_max_heuristic = null;
                    switch (heuristicType)
                    {
                        case 0: // POINTS DIFF
                            min_max_heuristic = new PointsDiffHeuristic();
                            break;
                        case 1: // MOVES COUNT DIFF
                            min_max_heuristic = new MovesDiffCountHeuristic();
                            break;
                        default:
                            Logger.Fatal("Unknown heuristic");
                            break;
                    }
                    return new MinMaxResearchPlayer(playerId, min_max_heuristic, depth);
                case 2: // ALPHABETA PLAYER
                    Heuristic alpha_beta_heuristic = null;
                    switch (heuristicType)
                    {
                        case 0: // POINTS DIFF
                            alpha_beta_heuristic = new PointsDiffHeuristic();
                            break;
                        case 1: // MOVES COUNT DIFF
                            alpha_beta_heuristic = new MovesDiffCountHeuristic();
                            break;
                        default:
                            Logger.Fatal("Unknown heuristic");
                            break;
                    }
					return new AlphaBetaResearchPlayer(playerId, alpha_beta_heuristic, depth);
                default:
                    Logger.Fatal("Unknown player type");
                    break;
            }

            return null;
        }
    }
}