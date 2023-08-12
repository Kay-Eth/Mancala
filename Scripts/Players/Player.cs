using Godot;
using KayEth.Mancala.Managers;
using KayEth.Mancala.Tools;
using System;

namespace KayEth.Mancala.Players
{
    public abstract class Player : Node
    {
        public int PlayerId { get; private set; }
        public MancalaBoardData BoardData { get { return GameManager.Instance.BoardData; } }
        public abstract string PlayerType { get; }

        public Player()
        {
            PlayerId = -1;
        }

        public Player(int playerId)
        {
            PlayerId = playerId;
        }

        public abstract void MakeMove(MancalaBoardData mbd);

        public void ExecuteMove(int hole)
        {
            Logger.Info($"Player: {PlayerId}, Hole: {hole}");
            GameManager.Instance.ExecuteMove(PlayerId, hole);
        }
    }
}