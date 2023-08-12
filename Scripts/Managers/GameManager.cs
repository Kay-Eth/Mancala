using Godot;
using KayEth.Mancala.Autoloads;
using KayEth.Mancala.Players;
using KayEth.Mancala.Players.Heuristics;
using KayEth.Mancala.Tools;
using KayEth.Mancala.UI;
using System;
using System.Threading.Tasks;

namespace KayEth.Mancala.Managers
{
    public class GameManager : Node
    {
        public static GameManager Instance { get; private set; }
        public MancalaBoardData BoardData { get; private set; }

        Player PlayerOne { get; set; }
        Player PlayerTwo { get; set; }

        [Export]
        NodePath _playersNodeNodePath = null;
        public Node PlayersNode { get { return GetNode<Node>(_playersNodeNodePath); } }

        public BoardViewManager BoardViewManager { get { return GetNode<BoardViewManager>("BoardViewManager"); } }

        public async override void _Ready()
        {
            if (Instance == null)
                Instance = this;
            else
                Logger.Fatal("There cannot be two game managers.");

            BoardData = new MancalaBoardData(6, 4);

            // PlayerOne = new HumanPlayer(0);
            // PlayerOne = new MinMaxPlayer(0, new PointsDiffHeuristic(), 4);
            // PlayerTwo = new MinMaxPlayer(1, new PointsDiffHeuristic(), 4);
            PlayerOne = MainManager.Instance.playerOne;
            PlayerTwo = MainManager.Instance.playerTwo;

            if (PlayerOne is MinMaxPlayer mmp1)
                mmp1.Connect(nameof(MinMaxPlayer.FinishedCalculation), this, nameof(PrintResult), new Godot.Collections.Array() {0});
            if (PlayerTwo is MinMaxPlayer mmp2)
                mmp2.Connect(nameof(MinMaxPlayer.FinishedCalculation), this, nameof(PrintResult), new Godot.Collections.Array() {1});
            if (PlayerOne is AlphaBetaPlayer abp1)
                abp1.Connect(nameof(MinMaxPlayer.FinishedCalculation), this, nameof(PrintResult), new Godot.Collections.Array() {0});
            if (PlayerTwo is AlphaBetaPlayer abp2)
                abp2.Connect(nameof(MinMaxPlayer.FinishedCalculation), this, nameof(PrintResult), new Godot.Collections.Array() {1});

            MainManager.Instance.playerOne = null;
            MainManager.Instance.playerTwo = null;

            BoardViewManager.ConfigurePlayerLabels(PlayerOne.PlayerType, PlayerTwo.PlayerType);

            PlayersNode.AddChild(PlayerOne);
            PlayersNode.AddChild(PlayerTwo);

            BoardViewManager.SetUp(BoardData);

            BoardViewManager.ShowPlayerTurn(0);

            await ToSignal(GetTree(), "idle_frame");
            await ToSignal(GetTree(), "idle_frame");
            await ToSignal(GetTree(), "idle_frame");

            if (MainManager.Instance.random)
            {
                Random rand = new Random();
                ExecuteMove(0, rand.Next(6));
            }
            else
            {
                PlayerOne.MakeMove(BoardData);
            }
        }

        public override void _ExitTree()
        {
            Instance = null;
        }

        public async void ExecuteMove(int playerId, int hole)
        {
            int result = MancalaController.MakeMove(BoardData, playerId, hole);

            BoardViewManager.UpdateView(playerId, hole, BoardData);
            await ToSignal(BoardViewManager, nameof(BoardViewManager.UpdateFinished));

            BoardViewManager.ShowPlayerTurn(result);

            await ToSignal(GetTree(), "idle_frame");
            await ToSignal(GetTree(), "idle_frame");
            await ToSignal(GetTree(), "idle_frame");
            
            if (result == 0)
            {
                PlayerOne.MakeMove(BoardData);
            }
            else if (result == 1)
            {
                PlayerTwo.MakeMove(BoardData);
            }
            else
            {
                Logger.Info("END GAME");

                ShowEndScreen();
            }
        }

        public void ShowEndScreen()
        {
            GetNode<ResultDialog>("ResultDialog").Configure(PlayerOne.PlayerType, PlayerTwo.PlayerType, BoardData);

            GetNode<AnimationPlayer>("AnimationPlayer").Play("ShowBlur");
            GetNode<ResultDialog>("ResultDialog").PopupCentered(new Vector2(950, 750));
        }

        public void OnReturnButtonPressed()
        {
            GetTree().ChangeScene("res://Resources/Scenes/MainMenu.scn");
        }

        public void PrintResult(double time, int playerId)
        {
            GD.Print($"{playerId}: {time}ms");
        }
    }
}
