using Godot;
using KayEth.Mancala.Tools;
using System;

namespace KayEth.Mancala.Research
{
    public abstract class ResearchPlayer
    {
        public int PlayerId { get; private set; }
        public abstract string PlayerType { get; }

        public ResearchPlayer(int id)
        {
            PlayerId = id;
        }

        public abstract int GetMove(MancalaBoardData mbd);
    }
}