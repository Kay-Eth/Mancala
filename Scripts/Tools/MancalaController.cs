using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KayEth.Mancala.Tools
{
    public static class MancalaController
    {
        public static int GetPlayerHoleIndex(MancalaBoardData boardData, int player, int hole)
        {
            return hole + player + player * boardData.holesCount;
        }

        public static int GetPlayerWellIndex(MancalaBoardData boardData, int player)
        {
            return player + player * boardData.holesCount + boardData.holesCount;
        }

        public static int GetHoleStonesCount(MancalaBoardData boardData, int index)
        {
            return boardData.holes[index];
        }

        public static void IncreaseStonesCount(MancalaBoardData boardData, int index, int amount)
        {
            Logger.Info($"Increased hole's count. index: {index}, amount: {amount}");
            boardData.holes[index] += amount;
        }

        public static bool IsMoveLegal(MancalaBoardData boardData, int player, int hole)
        {
            return GetHoleStonesCount(boardData, GetPlayerHoleIndex(boardData, player, hole)) > 0;
        }

        public static int GetOpponent(int player)
        {
            return player == 0 ? 1 : 0;
        }

        public static int GetNextIndex(MancalaBoardData boardData, int player, int index)
        {
            var result = index + 1;
            if (result == GetPlayerWellIndex(boardData, GetOpponent(player)))
                result++;
            if (result == boardData.holes.Length)
                result = 0;
            return result;
        }

        public static bool IsPlayersHole(MancalaBoardData boardData, int player, int index)
        {
            if (player == 0 && index < (boardData.holes.Length / 2))
                return true;
            if (player == 1 && index > (boardData.holes.Length / 2 - 1))
                return true;
            return false;
        }

        public static int GetOppositeIndex(MancalaBoardData boardData, int index)
        {
            return boardData.holes.Length - index - 2;
        }
        
        public static void CleanHole(MancalaBoardData boardData, int index)
        {
            Logger.Info($"Clean hole. Index: {index}");
            boardData.holes[index] = 0;
        }

        public static int[] GetLegalMoves(MancalaBoardData boardData, int player)
        {
            var result = new List<int>();
            var start_index = player * boardData.holes.Length / 2;
            for (int i = 0; i < boardData.holesCount; i++)
            {
                if (GetHoleStonesCount(boardData, start_index + i) > 0)
                    result.Add(i);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Makes given move on the board.
        /// </summary>
        /// <param name="boardData">Board data object</param>
        /// <param name="player">Player id</param>
        /// <param name="hole">Hole id</param>
        /// <returns>Next player id</returns>
        public static int MakeMove(MancalaBoardData boardData, int player, int hole)
        {
            if (!IsMoveLegal(boardData, player, hole))
            {
                Logger.Error("Illegal move.");
                return -1;
            }
                
            Logger.Info($"Player's move: {player}, hole {hole}");

            boardData.moves.Add(new Tuple<int, int>(player, hole));

            int next_player = GetOpponent(player);

            int index = GetPlayerHoleIndex(boardData, player, hole);
            Logger.Debug($"Player's hole index: {index}");
            int stones_count = GetHoleStonesCount(boardData, index);
            Logger.Debug($"Player's hole stones count: {stones_count}");
            CleanHole(boardData, index);

            while (stones_count > 0)
            {
                index = GetNextIndex(boardData, player, index);
                Logger.Debug($"Next index: {index}");
                IncreaseStonesCount(boardData, index, 1);
                stones_count--;
            }

            Logger.Info($"Last index: {index}");
            Logger.Debug($"Is player's well: {index == GetPlayerWellIndex(boardData, player)}");
            Logger.Debug($"Is player's hole: {IsPlayersHole(boardData, player, index)}");
            Logger.Debug($"Player's hole count: {GetHoleStonesCount(boardData, index)}");
            if (index == GetPlayerWellIndex(boardData, player))
                next_player = player;
            else if (
                IsPlayersHole(boardData, player, index)
                && GetHoleStonesCount(boardData, index) == 1)
            {
                int opposite_index = GetOppositeIndex(boardData, index);
                Logger.Debug($"Opposite index: {opposite_index}");
                int opposite_count = GetHoleStonesCount(boardData, opposite_index);
                Logger.Debug($"Opposite count: {opposite_count}");
                if (opposite_count > 0)
                {
                    CleanHole(boardData, index);
                    CleanHole(boardData, opposite_index);

                    IncreaseStonesCount(boardData, GetPlayerWellIndex(boardData, player), opposite_count + 1);
                }
            }

            if (GetLegalMoves(boardData, next_player).Length == 0)
            {
                Logger.Info("End of the game.");

                int opponent = GetOpponent(next_player);
                int opponent_well_index = GetPlayerWellIndex(boardData, opponent);
                for (int i = 0; i < boardData.holesCount; i++)
                {
                    int player_0_hole_index = GetPlayerHoleIndex(boardData, 0, i);
                    int player_1_hole_index = GetPlayerHoleIndex(boardData, 1, i);
                    IncreaseStonesCount(
                        boardData,
                        opponent_well_index,
                        GetHoleStonesCount(boardData, player_0_hole_index) + GetHoleStonesCount(boardData, player_1_hole_index)
                    );
                    CleanHole(boardData, player_0_hole_index);
                    CleanHole(boardData, player_1_hole_index);
                }

                var player_0_score = GetHoleStonesCount(boardData, GetPlayerWellIndex(boardData, 0));
                var player_1_score = GetHoleStonesCount(boardData, GetPlayerWellIndex(boardData, 1));

                if (player_0_score == player_1_score)
                    boardData.winner = MancalaBoardData.TIE;
                else if (player_0_score > player_1_score)
                    boardData.winner = MancalaBoardData.PLAYER_ONE;
                else
                    boardData.winner = MancalaBoardData.PLAYER_TWO;

                return -1;
            }

            Logger.Info($"Next player: {next_player}");
            return next_player;
        }

        public static void PrintPretty(MancalaBoardData boardData)
        {
            GD.PrintRaw("\n\t");
            for (int i = 0; i < boardData.holesCount; i++)
            {
                GD.PrintRaw(GD.Str(boardData.holes[boardData.holes.Length - i - 2], "\t"));
            }
            GD.PrintRaw("\n");
            GD.PrintRaw(GD.Str(boardData.holes[boardData.holes.Length - 1]));
            for (int i = 0; i < boardData.holesCount; i++)
            {
                GD.PrintRaw("\t");
            }
            GD.PrintRaw(GD.Str("\t", boardData.holes[boardData.holes.Length / 2 - 1], "\n"));
            GD.PrintRaw("\t");
            for (int i = 0; i < boardData.holesCount; i++)
            {
                GD.PrintRaw(GD.Str(boardData.holes[i], "\t"));
            }
            GD.PrintRaw("\n\n");
        }

        public static int MovesCountOfPlayer(MancalaBoardData boardData, int player)
        {
            return boardData.moves.FindAll(x => x.Item1 == player).Count();
        }
    }
}