using Godot;
using System;
using System.Collections.Generic;

namespace KayEth.Mancala.Tools
{
    public class MancalaBoardData
    {
        public const int PLAYER_NONE = -1;
        public const int PLAYER_ONE = 0;
        public const int PLAYER_TWO = 1;
        public const int TIE = 2;

        public int winner;
        public readonly int holesCount;
        public int[] holes;

        public List<Tuple<int, int>> moves;

        public MancalaBoardData(int numberOfHolesForPlayer, int numberOfStartStones)
        {
            winner = PLAYER_NONE;
            holesCount = numberOfHolesForPlayer;
            holes = new int[numberOfHolesForPlayer * 2 + 2];
            moves = new List<Tuple<int, int>>();

            for (int i = 0; i < holes.Length; i++)
            {
                if (i % (holesCount + 1) == holesCount)
                    holes[i] = 0;
                else
                    holes[i] = numberOfStartStones;
            }
        }

        public MancalaBoardData Copy()
        {
            var result = new MancalaBoardData(holesCount, 0);
            result.winner = this.winner;
            for (int i = 0; i < holes.Length; i++)
            {
                result.holes[i] = this.holes[i];
            }

            foreach (var tuple in moves)
            {
                result.moves.Add(new Tuple<int, int>(tuple.Item1, tuple.Item2));
            }

            return result;
        }

        public bool IsSame(MancalaBoardData other)
        {
            for (int i = 0; i < holes.Length; i++)
            {
                if (holes[i] != other.holes[i])
                    return false;
            }

            return true;
        }
    }
}