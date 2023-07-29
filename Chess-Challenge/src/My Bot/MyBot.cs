using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{

    // none, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 320, 330, 500, 900, 1000000 };
    int[] piecePhases = { 0, 0, 1, 1, 2, 4, 0 };
    // 0-7: pawn, 8-15: knight, 16-23: bishop, 24-31: rook, 32-39: queen, 40-47: king mg, 48-55: king eg
    ulong[] psts = { 0b0011001000110010001100100011001000110010001100100011001000110010, 0b0011011100111100001111000001111000011110001111000011110000110111, 0b0011011100101101001010000011001000110010001010000010110100110111, 0b0011001000110010001100100100011001000110001100100011001000110010, 0b0011011100110111001111000100101101001011001111000011011100110111, 0b0011110000111100010001100101000001010000010001100011110000111100, 0b0110010001100100011001000110010001100100011001000110010001100100, 0b0011001000110010001100100011001000110010001100100011001000110010, 0b0000000000001010000101000001010000010100000101000000101000000000, 0b0000101000011110001100100011011100110111001100100001111000001010, 0b0001010000110111001111000100000101000001001111000011011100010100, 0b0001010000110010010000010100011001000110010000010011001000010100, 0b0001010000110111010000010100011001000110010000010011011100010100, 0b0001010000110010001111000100000101000001001111000011001000010100, 0b0000101000011110001100100011001000110010001100100001111000001010, 0b0000000000001010000101000001010000010100000101000000101000000000, 0b0001111000101000001010000010100000101000001010000010100000011110, 0b0010100000110111001100100011001000110010001100100011011100101000, 0b0010100000111100001111000011110000111100001111000011110000101000, 0b0010100000110010001111000011110000111100001111000011001000101000, 0b0010100000110111001101110011110000111100001101110011011100101000, 0b0010100000110010001101110011110000111100001101110011001000101000, 0b0010100000110010001100100011001000110010001100100011001000101000, 0b0001111000101000001010000010100000101000001010000010100000011110, 0b0011001000110010001100100011011100110111001100100011001000110010, 0b0010110100110010001100100011001000110010001100100011001000101101, 0b0010110100110010001100100011001000110010001100100011001000101101, 0b0010110100110010001100100011001000110010001100100011001000101101, 0b0010110100110010001100100011001000110010001100100011001000101101, 0b0010110100110010001100100011001000110010001100100011001000101101, 0b0011011100111100001111000011110000111100001111000011110000110111, 0b0011001000110010001100100011001000110010001100100011001000110010, 0b0001111000101000001010000010110100101101001010000010100000011110, 0b0010100000110010001101110011001000110010001100100011001000101000, 0b0010100000110111001101110011011100110111001101110011001000101000, 0b0011001000110010001101110011011100110111001101110011001000101101, 0b0010110100110010001101110011011100110111001101110011001000101101, 0b0010100000110010001101110011011100110111001101110011001000101000, 0b0010100000110010001100100011001000110010001100100011001000101000, 0b0001111000101000001010000010110100101101001010000010100000011110, 0b0100011001010000001111000011001000110010001111000101000001000110, 0b0100011001000110001100100011001000110010001100100100011001000110, 0b0010100000011110000111100001111000011110000111100001111000101000, 0b0001111000010100000101000000101000001010000101000001010000011110, 0b0001010000001010000010100000000000000000000010100000101000010100, 0b0001010000001010000010100000000000000000000010100000101000010100, 0b0001010000001010000010100000000000000000000010100000101000010100, 0b0001010000001010000010100000000000000000000010100000101000010100, 0b0000000000010100000101000001010000010100000101000001010000000000, 0b0001010000010100001100100011001000110010001100100001010000010100, 0b0001010000101000010001100101000001010000010001100010100000010100, 0b0001010000101000010100000101101001011010010100000010100000010100, 0b0001010000101000010100000101101001011010010100000010100000010100, 0b0001010000101000010001100101000001010000010001100010100000010100, 0b0001010000011110001010000011001000110010001010000001111000010100, 0b0000000000001010000101000001111000011110000101000000101000000000 };


    private static int maxDepth = 5;

    static Move bestMove = Move.NullMove;

    // multiplying -1 to int.MinValue will cause an overflow, so use infinity for max number
    static int infinity = 9999999;

    int posCount = 0;

    public Move Think(Board board, Timer timer)
    {
        posCount = 0;
        System.Console.WriteLine("x: " + getPstValue(7, 0));
        Search(board, maxDepth, -infinity, infinity);

        System.Console.WriteLine("Best move: " + bestMove.ToString());

        System.Console.WriteLine(timer.MillisecondsElapsedThisTurn);
        System.Console.WriteLine("posCount: " + posCount);


        return bestMove.IsNull ? board.GetLegalMoves()[0] : bestMove;
    }

    int getPstValue(int pieceType, int idx)
    {
        return (int)(psts[((pieceType - 1) * 8 + (idx / 8))] >> (64 - (((idx % 8) + 1) * 8)) & 127) - 50;
    }


    int Evaluate(Board board)
    {

        int mg = 0, eg = 0, phase = 0;
        foreach (bool isWhite in new[] { true, false })
        {
            for (var p = PieceType.Pawn; p <= PieceType.King; p++)
            {
                int piece = (int)p;
                ulong bb = board.GetPieceBitboard(p, isWhite);
                while (bb != 0)
                {
                    phase += piecePhases[piece];
                    int idx = isWhite ? BitboardHelper.ClearAndGetIndexOfLSB(ref bb) : 64 - BitboardHelper.ClearAndGetIndexOfLSB(ref bb);
                    mg += pieceValues[piece] + getPstValue(piece, idx);
                    eg += pieceValues[piece] + getPstValue(p == PieceType.King ? 7 : piece, idx);
                }

            }
            mg = -mg;
            eg = -eg;

        }

        return (mg * (Math.Max(phase, 24)) + eg * (24 - phase)) * (board.IsWhiteToMove ? 1 : -1);
    }

    int CaptureSearch(Board board, int alpha, int beta)
    {
        int evaluation = Evaluate(board);
        if (evaluation >= beta)
            return beta;
        alpha = Math.Max(alpha, evaluation);

        Move[] moves = board.GetLegalMoves(true);

        OrderMoves(board, moves);

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int score = -CaptureSearch(board, -beta, -alpha);

            board.UndoMove(move);
            if (score >= beta)
                return beta;
            alpha = Math.Max(alpha, score);
        }

        return alpha;
    }

    int Search(Board board, int depth, int alpha, int beta, int movesFromRoot = 0)
    {
        posCount += 1;
        bool root = movesFromRoot == 0;

        if (!root && board.IsRepeatedPosition())
            return 0;

        if (depth == 0)
        {
            return CaptureSearch(board, alpha, beta);
        }

        Move[] moves = board.GetLegalMoves();

        if (moves.Length == 0)
        {
            return board.IsInCheck() ? -infinity + movesFromRoot : 0;
        }


        OrderMoves(board, moves);

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int score = -Search(board, depth - 1, -beta, -alpha, movesFromRoot + 1);

            board.UndoMove(move);
            if (score >= beta)
            {
                return beta;
            }
            if (score > alpha)
            {
                alpha = score;
                if (movesFromRoot == 0)
                {
                    bestMove = move;
                }
            }
        }
        return alpha;
    }

    void OrderMoves(Board board, Move[] moves)
    {
        int[] moveScores = new int[moves.Length];

        for (int i = 0; i < moves.Length; i++)
        {
            int moveScoreGuess = 0;
            Move move = moves[i];
            if (move.IsCapture)
            {
                moveScoreGuess += 10 * pieceValues[(int)move.CapturePieceType] - pieceValues[(int)move.MovePieceType];
            }
            if (move.IsPromotion)
            {
                moveScoreGuess += pieceValues[(int)move.PromotionPieceType];
            }

            foreach (Piece p in board.GetPieceList(PieceType.Pawn, !board.IsWhiteToMove))
            {
                if (BitboardHelper.SquareIsSet(BitboardHelper.GetPawnAttacks(p.Square, !board.IsWhiteToMove), move.TargetSquare))
                {
                    moveScoreGuess -= pieceValues[(int)move.MovePieceType];
                    break;
                }
            }

            moveScores[i] = moveScoreGuess;
        }

        Sort(moves, moveScores);
    }

    void Sort(Move[] moves, int[] moveScores)
    {
        for (int i = 0; i < moves.Length - 1; i++)
        {

            for (int j = i + 1; j > 0; j--)
            {
                int swapIndex = j - 1;
                if (moveScores[swapIndex] < moveScores[j])
                {
                    (moves[j], moves[swapIndex]) = (moves[swapIndex], moves[j]);
                    (moveScores[j], moveScores[swapIndex]) = (moveScores[swapIndex], moveScores[j]);
                }
            }
        }
    }

}
