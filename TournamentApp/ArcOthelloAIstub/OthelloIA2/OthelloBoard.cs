﻿using OthelloAIstub;
using System;
using System.Collections.Generic;

namespace IACapocasaleMoulin
{

    // Tile states
    public enum TileState
    {
        EMPTY = -1,
        WHITE = 0,
        BLACK = 1
    }

    public class OthelloBoard : IPlayable.IPlayable
    {
        const int BOARDSIZE_X = 9;
        const int BOARDSIZE_Y = 7;

        int[,] theBoard = new int[BOARDSIZE_X, BOARDSIZE_Y];
        int whiteScore = 0;
        int blackScore = 0;
        public bool GameFinish { get; set; }

        public static readonly int[,] SCORE_MATRIX = new int[9, 7] {
            { 500,  -25,   25,    3,   25,  -25,  500},
            { -25, -150,    0,    0,    0, -150,  -25},
            {  25,    0,    3,    3,    3,    0,   25},
            {   3,    0,    3,    3,    3,    0,    3},
            {  25,    0,    3,    3,    3,    0,   25},
            {   3,    0,    3,    3,    3,    0,    3},
            {  25,    0,    3,    3,    3,    0,   25},
            { -25, -150,    0,    0,    0, -150,  -25},
            { 500,  -25,   25,    3,   25,  -25,  500},
         };

        public OthelloBoard()
        {
            InitBoard();
        }

        public OthelloBoard(int[,] board)
        {
            theBoard = (int[,])board.Clone();
        }

        /// <summary>
        /// Return in a tuple the number of pawn opponent and current user in the board.
        /// The first tuple item is the number of current user pawn
        /// The second tuple item is the number of opponent player pawn
        /// </summary>
        /// <returns>The tuple with the number of pwan by player</returns>
        public static Tuple<int, int> CountPawn(int[,] theBoard, int playerVal)
        {
            int nbUserToken = 0;
            int nbOpponentToken = 0;
            
            for (int line = 0; line < BOARDSIZE_Y; line++)
            {
                for (int col = 0; col < BOARDSIZE_X; col++)
                {
                    if(theBoard[col, line] != -1)
                    {
                        if (theBoard[col, line] == playerVal)
                        {
                            nbUserToken++;
                        }
                        else
                        {
                            nbOpponentToken++;
                        }
                    }
                }
            }
            return new Tuple<int, int>(nbUserToken, nbOpponentToken);
        }

        /// <summary>
        /// Return in a tuple the number of captured corner for the opponent and for the current user in the board.
        /// The first tuple item is the number of capturer corner for the user pawn
        /// The second tuple item is the number of captured corner for the opponent player pawn
        /// </summary>
        /// <returns>The tuple with the number of captured corner by player</returns>
        public static Tuple<int,int> CountCorner(int[,] theBoard, int playerVal)
        {
            int nbUserCorner = 0;
            int nbOpponentCorner = 0;

            //List of corner coordinate
            List<Tuple<int, int>> cornerCoords = new List<Tuple<int, int>> { new Tuple<int, int>(0,0),
                                                                           new Tuple<int, int>(BOARDSIZE_X - 1, BOARDSIZE_Y - 1),
                                                                           new Tuple<int, int>(0, BOARDSIZE_Y - 1),
                                                                           new Tuple<int, int>(BOARDSIZE_X - 1,0)};
            foreach(Tuple<int,int> cornerCoord in cornerCoords)
            {
                if (theBoard[cornerCoord.Item1, cornerCoord.Item2] != -1)
                {
                    if (theBoard[cornerCoord.Item1, cornerCoord.Item2] == playerVal)
                    {
                        nbUserCorner++;
                    }
                    else
                    {
                        nbOpponentCorner++;
                    }
                }
            }
            return new Tuple<int, int>(nbUserCorner, nbOpponentCorner);
        }
        

        public void DrawBoard()
        {
            Console.WriteLine("REFERENCE" + "\tBLACK [X]:" + blackScore + "\tWHITE [O]:" + whiteScore);
            Console.WriteLine("  A B C D E F G H I");
            for (int line = 0; line < BOARDSIZE_Y; line++)
            {
                Console.Write($"{(line + 1)}");
                for (int col = 0; col < BOARDSIZE_X; col++)
                {
                    Console.Write((theBoard[col, line] == (int)TileState.EMPTY) ? " -" : (theBoard[col, line] == (int)TileState.WHITE) ? " O" : " X");
                }
                Console.Write("\n");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        /// <summary>
        /// Returns the board game as a 2D array of int
        /// with following values
        /// -1: empty
        ///  0: white
        ///  1: black
        /// </summary>
        /// <returns></returns>
        public int[,] GetBoard()
        {
            return (int[,])theBoard;
        }

        #region IPlayable
        public int GetWhiteScore() { return whiteScore; }
        public int GetBlackScore() { return blackScore; }
        public string GetName() { return "CapocasaleMoulin"; }

        /// <summary>
        /// Play a move with the alphabeta algo
        /// </summary>
        /// <param name="game">the current board</param>
        /// <param name="level">max depth for the algo</param>
        /// <param name="whiteTurn">indicated if is white turn or not</param>
        /// <returns>The move it will play, will return {P,0} if it has to PASS its turn (no move is possible)</returns>
        public Tuple<int, int> GetNextMove(int[,] game, int level, bool whiteTurn)
        {
            List<Tuple<int, int>> possibleMoves = GetPossibleMove(whiteTurn);

             if (possibleMoves.Count == 0)
             {
                 return new Tuple<int, int>(-1, -1);
             }
             else
             {
                // Create the root node and pass them to the alphabeta algo
                 TreeNode root = new TreeNode(this, whiteTurn);
                 Tuple<int, Tuple<int, int>> move = AlphaBetaAlgo.Alphabeta(root, level, 1, int.MaxValue);
               

                if(possibleMoves.Contains(move.Item2))
                {
                    return move.Item2;
                }
                else
                {
                    return possibleMoves[0];
                }

             }

        }
        private Random rnd = new Random();

        public bool PlayMove(int column, int line, bool isWhite)
        {
            //0. Verify if indices are valid
            if ((column < 0) || (column >= BOARDSIZE_X) || (line < 0) || (line >= BOARDSIZE_Y))
                return false;
            //1. Verify if it is playable
            if (IsPlayable(column, line, isWhite) == false)
                return false;

            //2. Create a list of directions {dx,dy,length} where tiles are flipped
            int c = column, l = line;
            bool playable = false;
            TileState opponent = isWhite ? TileState.BLACK : TileState.WHITE;
            TileState ownColor = (!isWhite) ? TileState.BLACK : TileState.WHITE;
            List<Tuple<int, int, int>> catchDirections = new List<Tuple<int, int, int>>();

            for (int dLine = -1; dLine <= 1; dLine++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    c = column + dCol;
                    l = line + dLine;
                    if ((c < BOARDSIZE_X) && (c >= 0) && (l < BOARDSIZE_Y) && (l >= 0)
                        && (theBoard[c, l] == (int)opponent))
                    // Verify if there is a friendly tile to "pinch" and return ennemy tiles in this direction
                    {
                        int counter = 0;
                        while (((c + dCol) < BOARDSIZE_X) && (c + dCol >= 0) &&
                                  ((l + dLine) < BOARDSIZE_Y) && ((l + dLine >= 0))
                                   && (theBoard[c, l] == (int)opponent)) // pour éviter les trous
                        {
                            c += dCol;
                            l += dLine;
                            counter++;
                            if (theBoard[c, l] == (int)ownColor)
                            {
                                playable = true;
                                theBoard[column, line] = (int)ownColor;
                                catchDirections.Add(new Tuple<int, int, int>(dCol, dLine, counter));
                            }
                        }
                    }
                }
            }
            // 3. Flip ennemy tiles
            foreach (var v in catchDirections)
            {
                int counter = 0;
                l = line;
                c = column;
                while (counter++ < v.Item3)
                {
                    c += v.Item1;
                    l += v.Item2;
                    theBoard[c, l] = (int)ownColor;
                }
            }
            //Console.WriteLine("CATCH DIRECTIONS:" + catchDirections.Count);
            ComputeScore();
            return playable;
        }

        /// <summary>
        /// More convenient overload to verify if a move is possible
        /// </summary>
        /// <param name=""></param>
        /// <param name="isWhite"></param>
        /// <returns></returns>
        public bool IsPlayable(Tuple<int, int> move, bool isWhite)
        {
            return IsPlayable(move.Item1, move.Item2, isWhite);
        }

        public bool IsPlayable(int column, int line, bool isWhite)
        {
            //1. Verify if the tile is empty !
            if (theBoard[column, line] != (int)TileState.EMPTY)
                return false;
            //2. Verify if at least one adjacent tile has an opponent tile
            TileState opponent = isWhite ? TileState.BLACK : TileState.WHITE;
            TileState ownColor = (!isWhite) ? TileState.BLACK : TileState.WHITE;
            int c = column, l = line;
            bool playable = false;
            List<Tuple<int, int, int>> catchDirections = new List<Tuple<int, int, int>>();
            for (int dLine = -1; dLine <= 1; dLine++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    c = column + dCol;
                    l = line + dLine;
                    if ((c < BOARDSIZE_X) && (c >= 0) && (l < BOARDSIZE_Y) && (l >= 0)
                        && (theBoard[c, l] == (int)opponent))
                    // Verify if there is a friendly tile to "pinch" and return ennemy tiles in this direction
                    {
                        int counter = 0;
                        while (((c + dCol) < BOARDSIZE_X) && (c + dCol >= 0) &&
                                  ((l + dLine) < BOARDSIZE_Y) && ((l + dLine >= 0)))
                        {
                            c += dCol;
                            l += dLine;
                            counter++;
                            if (theBoard[c, l] == (int)ownColor)
                            {
                                playable = true;
                                break;
                            }
                            else if (theBoard[c, l] == (int)opponent)
                                continue;
                            else if (theBoard[c, l] == (int)TileState.EMPTY)
                                break;  //empty slot ends the search
                        }
                    }
                }
            }
            return playable;
        }
        #endregion

        /// <summary>
        /// Returns all the playable moves in a human readable way (e.g. "G3")
        /// </summary>
        /// <param name="v"></param>
        /// <param name="whiteTurn"></param>
        /// <returns></returns>
        public List<Tuple<char, int>> GetPossibleMoves(bool whiteTurn, bool show = false)
        {
            char[] colonnes = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            List<Tuple<char, int>> possibleMoves = new List<Tuple<char, int>>();
            for (int i = 0; i < BOARDSIZE_X; i++)
                for (int j = 0; j < BOARDSIZE_Y; j++)
                {
                    if (IsPlayable(i, j, whiteTurn))
                    {
                        possibleMoves.Add(new Tuple<char, int>(colonnes[i], j + 1));
                        if (show == true)
                            Console.Write((colonnes[i]).ToString() + (j + 1).ToString() + ", ");
                    }
                }
            return possibleMoves;
        }
        

        /// <summary>
        /// Returns all the playable moves in a computer readable way (e.g. "<3, 0>")
        /// </summary>
        /// <param name="v"></param>
        /// <param name="whiteTurn"></param>
        /// <returns></returns>
        public List<Tuple<int, int>> GetPossibleMove(bool whiteTurn, bool show = false)
        {
            char[] colonnes = "ABCDEFGHIJKL".ToCharArray();
            List<Tuple<int, int>> possibleMoves = new List<Tuple<int, int>>();
            for (int i = 0; i < BOARDSIZE_X; i++)
                for (int j = 0; j < BOARDSIZE_Y; j++)
                {
                    if (IsPlayable(i, j, whiteTurn))
                    {
                        possibleMoves.Add(new Tuple<int, int>(i, j));
                        if (show == true)
                            Console.Write((colonnes[i]).ToString() + (j + 1).ToString() + ", ");
                    }
                }
            return possibleMoves;
        }

       

        private void InitBoard()
        {
            for (int i = 0; i < BOARDSIZE_X; i++)
                for (int j = 0; j < BOARDSIZE_Y; j++)
                    theBoard[i, j] = (int)TileState.EMPTY;

            theBoard[3, 3] = (int)TileState.WHITE;
            theBoard[4, 4] = (int)TileState.WHITE;
            theBoard[3, 4] = (int)TileState.BLACK;
            theBoard[4, 3] = (int)TileState.BLACK;

            ComputeScore();
        }

        private void ComputeScore()
        {
            whiteScore = 0;
            blackScore = 0;
            foreach (var v in theBoard)
            {
                if (v == (int)TileState.WHITE)
                    whiteScore++;
                else if (v == (int)TileState.BLACK)
                    blackScore++;
            }
            GameFinish = ((whiteScore == 0) || (blackScore == 0) || (whiteScore + blackScore == 63));
        }
    }

}