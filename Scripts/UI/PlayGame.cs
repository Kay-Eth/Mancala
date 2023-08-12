using Godot;
using KayEth.Mancala.Autoloads;
using KayEth.Mancala.Players;
using KayEth.Mancala.Players.Heuristics;
using System;

namespace KayEth.Mancala.UI
{
    public class PlayGame : Control
    {
        [Signal]
        public delegate void Finished();

        [Export] NodePath _player0TypeNodePath = null;
        [Export] NodePath _player0DepthNodePath = null;
        [Export] NodePath _player0HeuristicNodePath = null;

        [Export] NodePath _player1TypeNodePath = null;
        [Export] NodePath _player1DepthNodePath = null;
        [Export] NodePath _player1HeuristicNodePath = null;

        public void OnPlayButtonPressed()
        {
            Player one = CreatePlayer(0,
                GetNode<OptionButton>(_player0TypeNodePath).Selected,
                GetNode<OptionButton>(_player0HeuristicNodePath).Selected,
                (int)(GetNode<SpinBox>(_player0DepthNodePath).Value)
            );
            

            Player two = CreatePlayer(1,
                GetNode<OptionButton>(_player1TypeNodePath).Selected,
                GetNode<OptionButton>(_player1HeuristicNodePath).Selected,
                (int)(GetNode<SpinBox>(_player1DepthNodePath).Value)
            );

            EmitSignal(nameof(Finished));
            MainManager.Instance.PlayGame(one, two, GetNode<OptionButton>("Center/PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/RandomButton").Selected == 1);
        }

        Player CreatePlayer(int playerId, int playerType, int heuristicType, int depth)
        {
			Logger.Info($"Create player. PlayerId: {playerId}, PlayerType: {playerType}, HeuristicType: {heuristicType}, Depth: {depth}");
            switch (playerType)
            {
                case 0: // HUMAN PLAYER
                    return new HumanPlayer(playerId);
                case 1: // RANDOM PLAYER
                    return new RandomPlayer(playerId);
                case 2: // MINIMAX PLAYER
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
                    return new MinMaxPlayer(playerId, min_max_heuristic, depth);
                case 3: // ALPHABETA PLAYER
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
					return new AlphaBetaPlayer(playerId, alpha_beta_heuristic, depth);
                default:
                    Logger.Fatal("Unknown player type");
                    break;
            }

            return null;
        }

        public void OnReturnButtonPressed()
        {
            EmitSignal(nameof(Finished));
        }

        public void OnPlayer1TypeOptionButtonItemSelected(int selected)
        {
            GetNode<Control>("Center/PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/Player1Cont/VBoxContainer/1MinMaxPlayerOptions").Visible = selected > 1;
        }

        public void OnPlayer2TypeOptionButtonItemSelected(int selected)
        {
            GetNode<Control>("Center/PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/Player2Cont/VBoxContainer/2MinMaxPlayerOptions").Visible = selected > 1;
        }

        public void OnButtonDown()
        {
            GetNode<AudioStreamPlayer>("ButtonClick").Play();
        }

        public void OnRandomButtonToggled(bool pressed)
        {
            // if (pressed)
            //     GetNode<Button>("Center/PanelContainer/MarginContainer/VBoxContainer/RandomButton").Text = TranslationServer.Translate("RANDOM_MOVE_ON");
            // else
            //     GetNode<Button>("Center/PanelContainer/MarginContainer/VBoxContainer/RandomButton").Text = TranslationServer.Translate("RANDOM_MOVE_OFF");
            
            // GetNode<Button>("Center/PanelContainer/MarginContainer/VBoxContainer/RandomButton").Pressed = pressed;
        }
    }
}
