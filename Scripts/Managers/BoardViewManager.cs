using Godot;
using KayEth.Mancala.Tools;
using KayEth.Mancala.UI;
using System;
using System.Threading.Tasks;

namespace KayEth.Mancala.Managers
{
    public class BoardViewManager : Control
    {
        float _move_time = MOVE_TIME;
        const float MOVE_TIME_END = 0.5f;
        const float MOVE_TIME = 0.2f;
        const float MOVE_MARGIN = 1;

        [Signal]
        public delegate void UpdateFinished();

        [Signal]
        public delegate void MovingFinished();

        [Export] NodePath _player0WellNodePath = null;
        [Export] NodePath _player1WellNodePath = null;
        [Export] NodePath _player0HolesNodePath = null;
        [Export] NodePath _player1HolesNodePath = null;

        public StoneContainer Player0Well { get; private set; }
        public StoneContainer Player1Well { get; private set; }

        public HBoxContainer Player0Holes { get; private set; }
        public HBoxContainer Player1Holes { get; private set; }

        MancalaBoardData MancalaBoardData { get; set; }

        TextureRect _moveStone;

        Label _player0turnLabel;
        Label _player1turnLabel;

        AudioStreamPlayer _audioPlayer;

        public override void _Ready()
        {
            _audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

            Player0Well = GetNode<StoneContainer>(_player0WellNodePath);
            Player1Well = GetNode<StoneContainer>(_player1WellNodePath);
            Player0Holes = GetNode<HBoxContainer>(_player0HolesNodePath);
            Player1Holes = GetNode<HBoxContainer>(_player1HolesNodePath);

            _player0turnLabel = GetNode<Label>("CenterContainer/Board/VBoxContainer/Player1TurnLabel");
            _player1turnLabel = GetNode<Label>("CenterContainer/Board/VBoxContainer/Player2TurnLabel");

            _moveStone = GetNode<TextureRect>("MoveStone");
            SetProcess(false);
        }

        public void ConfigurePlayerLabels(string player0, string player1)
        {
            _player0turnLabel.Text = TranslationServer.Translate("PLAYER_TURN").Replace("<PLAYER>", TranslationServer.Translate(player0));
            _player1turnLabel.Text = TranslationServer.Translate("PLAYER_TURN").Replace("<PLAYER>", TranslationServer.Translate(player1));
        }

        public void ShowPlayerTurn(int playerId)
        {
            if (playerId == 0)
            {
                _player0turnLabel.Modulate = new Color(1, 1, 1, 1);
                _player1turnLabel.Modulate = new Color(1, 1, 1, 0);
            }
            else if (playerId == 1)
            {
                _player0turnLabel.Modulate = new Color(1, 1, 1, 0);
                _player1turnLabel.Modulate = new Color(1, 1, 1, 1);
            }
            else
            {
                _player0turnLabel.Modulate = new Color(1, 1, 1, 0);
                _player1turnLabel.Modulate = new Color(1, 1, 1, 0);
            }
        }

        public void SetUp(MancalaBoardData mbd)
        {
            MancalaBoardData = mbd.Copy();

            foreach (StoneContainer st in Player0Holes.GetChildren())
            {
                for (int i = 0; i < mbd.holes[0]; i++)
                {
                    st.AddStone();
                }
            }

            foreach (StoneContainer st in Player1Holes.GetChildren())
            {
                for (int i = 0; i < mbd.holes[0]; i++)
                {
                    st.AddStone();
                }
            }
        }

        Vector2 _goalPosition = Vector2.Zero;

        public override void _Process(float delta)
        {
            if (_moveStone.RectPosition.DistanceTo(_goalPosition) < MOVE_MARGIN)
            {
                SetProcess(false);
                EmitSignal(nameof(MovingFinished));
            }
            else
            {
                _moveStone.RectPosition = _moveStone.RectPosition.LinearInterpolate(_goalPosition, _move_time);
            }
        }

        public async void UpdateView(int player, int hole, MancalaBoardData resultMbd)
        {
            int source_index = MancalaController.GetPlayerHoleIndex(MancalaBoardData, player, hole);
            StoneContainer source_container = GetStoneContainerFromIndex(source_index);
            // Vector2 source_start_pos = source_container.RectGlobalPosition + (source_container.RectSize - _moveStone.RectSize) / 2;

            int count = MancalaController.GetHoleStonesCount(MancalaBoardData, source_index);

            int current_index = source_index;
            while (count > 0)
            {
                current_index = MancalaController.GetNextIndex(MancalaBoardData, player, current_index);

                StoneContainer current_container = GetStoneContainerFromIndex(current_index);
                // var color = source_container.RemoveStone();

                // _moveStone.Modulate = color;
                
                // _moveStone.RectGlobalPosition = source_start_pos;
                // _goalPosition = current_container.RectGlobalPosition + (current_container.RectSize - _moveStone.RectSize) / 2;

                // _moveStone.Visible = true;

                // SetProcess(true);
                // await ToSignal(this, nameof(MovingFinished));
                // SetProcess(false);

                // _moveStone.Visible = false;

                // current_container.AddStone(color);

                await MoveStone(source_container, current_container);

                count--;
            }

            // MancalaController.MakeMove(MancalaBoardData, player, hole);
            bool IsPlayerHole = false;
            if (
                player == 0
                && current_index > -1 && current_index < 6)
            {
                IsPlayerHole = true;
            }
            else if (
                player == 1
                && current_index > 6 && current_index < 13)
            {
                IsPlayerHole = true;
            }

            if (
                IsPlayerHole
                && GetStoneContainerFromIndex(current_index).StonesCount == 1)
            {
                int opposite_index = MancalaController.GetOppositeIndex(MancalaBoardData, current_index);
                StoneContainer opposite_container = GetStoneContainerFromIndex(opposite_index);

                if (opposite_container.StonesCount > 0)
                {
                    source_index = current_index;
                    source_container = GetStoneContainerFromIndex(source_index);
                    // source_start_pos = source_container.RectGlobalPosition + (source_container.RectSize - _moveStone.RectSize) / 2;

                    StoneContainer target_container = player == 0 ? Player0Well : Player1Well;
                    await MoveStone(source_container, target_container);

                    while (opposite_container.StonesCount > 0)
                    {
                        await MoveStone(opposite_container, target_container);
                    }
                }
            }

            if (resultMbd.winner != -1)
            {
                GD.Print("WINNER");
                _move_time = MOVE_TIME_END;

                int player_0_score = GetStoneContainerFromIndex(MancalaController.GetPlayerWellIndex(resultMbd, 0)).StonesCount;
                int player_1_score = GetStoneContainerFromIndex(MancalaController.GetPlayerWellIndex(resultMbd, 1)).StonesCount;

                if (
                    resultMbd.holes[MancalaController.GetPlayerWellIndex(resultMbd, 0)] != player_0_score
                    && resultMbd.holes[MancalaController.GetPlayerWellIndex(resultMbd, 1)] == player_1_score)
                {
                    foreach (StoneContainer st in Player0Holes.GetChildren())
                    {
                        while (st.StonesCount > 0)
                            await MoveStone(st, Player0Well);
                    }
                }
                else if (
                    resultMbd.holes[MancalaController.GetPlayerWellIndex(resultMbd, 0)] == player_0_score
                    && resultMbd.holes[MancalaController.GetPlayerWellIndex(resultMbd, 1)] != player_1_score)
                {
                    foreach (StoneContainer st in Player1Holes.GetChildren())
                    {
                        while (st.StonesCount > 0)
                            await MoveStone(st, Player1Well);
                    }
                }
                else if (resultMbd.holes[MancalaController.GetPlayerWellIndex(resultMbd, 0)] != player_0_score
                    && resultMbd.holes[MancalaController.GetPlayerWellIndex(resultMbd, 1)] != player_1_score)
                {
                    Logger.Fatal("ERROR. Strange situation");
                }
            }

            for (int i = 0; i < MancalaBoardData.holes.Length; i++)
            {
                MancalaBoardData.holes[i] = GetStoneContainerFromIndex(i).StonesCount;
            }

            if (!MancalaBoardData.IsSame(resultMbd))
            {
                Logger.Fatal("Board view result is not equal to true result");
            }

            MancalaBoardData = resultMbd.Copy();

            EmitSignal(nameof(UpdateFinished));
        }

        public StoneContainer GetStoneContainerFromIndex(int index)
        {
            if (index < 6)
                return Player0Holes.GetChild<StoneContainer>(index);
            else if (index == 6)
                return Player0Well;
            else if (index < 13)
                return Player1Holes.GetChild<StoneContainer>(5 - (index - 7));
            else
                return Player1Well;
        }

        public async Task MoveStone(StoneContainer source_cont, StoneContainer target_cont)
        {
            var color = source_cont.RemoveStone();
            _moveStone.Modulate = color;

            _moveStone.RectGlobalPosition = source_cont.RectGlobalPosition + (source_cont.RectSize - _moveStone.RectSize) / 2;
            _moveStone.RectRotation = GD.Randf() * 360;
            _goalPosition = target_cont.RectGlobalPosition + (target_cont.RectSize - _moveStone.RectSize) / 2;

            _moveStone.Visible = true;

            SetProcess(true);
            await ToSignal(this, nameof(MovingFinished));
            SetProcess(false);

            _audioPlayer.Play();

            _moveStone.Visible = false;

            target_cont.AddStone(color);
        }
    }
}
