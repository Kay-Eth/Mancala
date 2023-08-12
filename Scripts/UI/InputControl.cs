using Godot;
using KayEth.Mancala.Managers;
using KayEth.Mancala.Players;
using KayEth.Mancala.Tools;
using System;
using System.Linq;

namespace KayEth.Mancala.UI
{
    public class InputControl : Control
    {
        public static InputControl Instance { get; set; }

        HumanPlayer _currentPlayer = null;

        [Export] NodePath _boardViewManagerNodePath = null;
        BoardViewManager BoardViewManager { get { return GetNode<BoardViewManager>(_boardViewManagerNodePath); } }

        public override void _Ready()
        {
            if (Instance == null)
                Instance = this;
            else
                Logger.Fatal("There cannot be two game input controls.");
            
        }

        public override void _Input(InputEvent @event)
        {
            if (_currentPlayer != null && @event is InputEventScreenTouch iest)
            {
                if (_currentPlayer.PlayerId == 0)
                {
                    int count = 0;
                    foreach (StoneContainer st in BoardViewManager.Player0Holes.GetChildren())
                    {
                        if (IsPointWithinObject(st, iest.Position))
                        {
                            SendMove(count);
                            return;
                        }
                        count++;
                    }
                }
                else if (_currentPlayer.PlayerId == 1)
                {
                    int count = 5;
                    foreach (StoneContainer st in BoardViewManager.Player1Holes.GetChildren())
                    {
                        if (IsPointWithinObject(st, iest.Position))
                        {
                            SendMove(count);
                            return;
                        }
                        count--;
                    }
                }
            }
        }

        public override void _ExitTree()
        {
            _currentPlayer = null;
            Instance = null;
        }

        public void RegisterMove(HumanPlayer player)
        {
            _currentPlayer = player;
        }

        void SendMove(int hole)
        {
            if (MancalaController.GetLegalMoves(_currentPlayer.BoardData, _currentPlayer.PlayerId).Contains(hole))
            {
                _currentPlayer.ExecuteMove(hole);
                _currentPlayer = null;
            }
        }

        bool IsPointWithinObject(Control control, Vector2 point)
        {
            return control.GetGlobalRect().HasPoint(point);
        }
    }
}