using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class MyBot : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        public Move Think(Board board, Timer timer)
        {
            Move[] allMoves = board.GetLegalMoves();

            Move moveToPlay = search(board, allMoves);
            Console.WriteLine("Current eval: " + evaluate(board));

            return moveToPlay;
        }

        public Move search(Board currentBoard, Move[] allMoves) {
            int latestBoardEval = evaluate(currentBoard);

            Random rng = new();
            Move random = allMoves[rng.Next(allMoves.Length)];
            Move topMove = random;

            int highestEval = -1;


            List<Move> topMoves = highestValCaps(allMoves, currentBoard);
            Console.WriteLine("Length of top three: " + topMoves.Count);


            foreach (Move move in topMoves) {
                Console.WriteLine("Move in top three: " + move);
                currentBoard.MakeMove(move);

                Move[] allEnemyMoves = currentBoard.GetLegalMoves();

                if (allEnemyMoves.Length == 0) {
                    topMove = move;
                    break;
                }

                Move enemyMove = highestValCap(allEnemyMoves, currentBoard);

                currentBoard.MakeMove(enemyMove);


                int eval = evaluate(currentBoard);

                int netEval = eval - latestBoardEval;

                Console.WriteLine(move + " eval: " + netEval);

                if (eval > highestEval) {
                    topMove  = move;
                    highestEval = eval;
                }
                currentBoard.UndoMove(enemyMove);
                currentBoard.UndoMove(move);
                Console.WriteLine("Top move is: " + topMove);

            }

            if (topMove.IsPromotion) {
                Square moveStart = topMove.StartSquare;
                Square moveEnd = topMove.TargetSquare;
                String moveStartString = moveStart.Name;
                String moveEndString = moveEnd.Name;
                topMove = new Move(moveStartString + moveEndString + "q", currentBoard);
            }
            Console.WriteLine("Actual move: " + topMove);
            return topMove;
        }

        // public Move search(Board currentBoard, Move[] allMoves) {
        //     Random rng = new();
        //     Move topMove = allMoves[rng.Next(allMoves.Length)];
        //     int highestValueCapture = 0;

        //     foreach (Move move in allMoves)
        //     {
        //         // Always play checkmate in one
        //         if (MoveIsCheckmate(currentBoard, move))
        //         {
        //             topMove = move;
        //             break;
        //         }

        //         // Find highest value capture
        //         Piece capturedPiece = currentBoard.GetPiece(move.TargetSquare);
        //         int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

        //         if (capturedPieceValue > highestValueCapture)
        //         {
        //             topMove = move;
        //             highestValueCapture = capturedPieceValue;
        //         }
        //     }
        //     if (topMove.IsPromotion) {
        //         Square moveStart = topMove.StartSquare;
        //         Square moveEnd = topMove.TargetSquare;
        //         String moveStartString = moveStart.Name;
        //         String moveEndString = moveEnd.Name;
        //         topMove = new Move(moveStartString + moveEndString + "q", currentBoard);
        //     }
        //     Console.WriteLine(topMove);
        //     return topMove;
        // }

        public Move highestValCap(Move[] moveList, Board currentBoard) {
            // Pick a random move to play if nothing better is found
            Random rng = new();
            Move moveToPlay = moveList[rng.Next(moveList.Length)];

            int highestValueCapture = 0;

            foreach (Move move in moveList)
            {
                // Always play checkmate in one
                if (MoveIsCheckmate(currentBoard, move))
                {
                    moveToPlay = move;
                    break;
                }

                // Find highest value capture
                Piece capturedPiece = currentBoard.GetPiece(move.TargetSquare);
                int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

                if (capturedPieceValue > highestValueCapture)
                {
                    moveToPlay = move;
                    highestValueCapture = capturedPieceValue;
                }

            }

            return moveToPlay;
        }

        public List<Move> highestValCaps(Move[] moveList, Board currentBoard) {
            // Pick a random move to play if nothing better is found
            Random rng = new();
            Move moveToPlay = moveList[rng.Next(moveList.Length)];

            Dictionary<Move, int> highestValueCaptures = new Dictionary<Move, int>();

            foreach (Move move in moveList)
            {
                // Always play checkmate in one
                if (MoveIsCheckmate(currentBoard, move))
                {   
                    highestValueCaptures.Add(move, 10000);
                    moveToPlay = move;
                    break;
                }

                // Find highest value capture
                Piece capturedPiece = currentBoard.GetPiece(move.TargetSquare);
                int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

                // if (capturedPieceValue > highestValueCapture)
                // {
                //     moveToPlay = move;
                //     highestValueCapture = capturedPieceValue;
                // }
                highestValueCaptures.Add(move, capturedPieceValue);
            }

            List<KeyValuePair<Move, int>> sortedMoves = highestValueCaptures.ToList();

            sortedMoves.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            List<KeyValuePair<Move, int>> topMoves = sortedMoves.Take(20).ToList();

            List<Move> topMovesList = topMoves.Select(pair => pair.Key).ToList();

            return topMovesList;
        }

        // Test if this move gives checkmate
        bool MoveIsCheckmate(Board board, Move move)
        {
            board.MakeMove(move);
            bool isMate = board.IsInCheckmate();
            board.UndoMove(move);
            return isMate;
        }

        int evaluate (Board board) {
            int whiteEvalCurrent = countMat(true, board);
            int blackEvalCurrent = countMat(false, board);

            int eval = whiteEvalCurrent - blackEvalCurrent;

            if (board.IsInCheckmate()) {
                return -1000000 * ((board.IsWhiteToMove) ? 1 : -1);
            }
            else {
                return eval * ((board.IsWhiteToMove) ? 1 : -1);
            }
            
        }

        int countMat(bool colorIndex, Board board) {
            int material = 0;
            material += pieceValues[1] * (board.GetPieceList(PieceType.Pawn, colorIndex).Count);
            material += pieceValues[2] * (board.GetPieceList(PieceType.Knight, colorIndex).Count);
            material += pieceValues[3] * (board.GetPieceList(PieceType.Bishop, colorIndex).Count);
            material += pieceValues[4] * (board.GetPieceList(PieceType.Rook, colorIndex).Count);
            material += pieceValues[5] * (board.GetPieceList(PieceType.Queen, colorIndex).Count);
            return material;
        }
    }
}