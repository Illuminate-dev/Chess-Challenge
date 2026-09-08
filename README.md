# Chess Coding Challenge (C#)

This is a chess AI programmed for the [chess coding challenge](https://youtu.be/iScy18pVR58) by Sebastian Lague. It placed 41st out of over 600 bots entered.
Note that the only code in this repo that is mine is the bot (`Chess-Challenge/src/My Bot/MyBot.cs`). More details are available at the [original repo](https://github.com/SebLague/Chess-Challenge).

## How it works

The bot is based on a minimax search function.
To choose a move, the `Think` function searches with a depth of 5 over all legal move trees and returns the best move it finds.

### Negamax with alpha-beta pruning

The `Search` function is where most of the work happens. In it, a depth-limited **negamax** search is performed, where negamax is a variant of minimax where each player's score is negative of the opponents score.
**Alpha–beta pruning** is used to reduce the number of subtrees needed to search. Whenever a move is valued to be worse than one already examined, the rest of the branch is cut off.

There are a few special cases:

- **Checkmate/stalemate:** if no legal moves exist, the score is a loss with a value of infinity, or `0` for stalemate.
- **Repetition:** repeated positions are scored as `0` to discourage the bot from randomly shuffling pieces.

### Quiescence Search

When searching to a fixed depth, often times the bot would make a capture on the last move, yet not see that the next move (the depth + 1) would lead to the opponent recapturing.
To fix this, there's a separate function, `CaptureSearch`, which is called when the depth on the main search reaches `0`.
This performs a **quiescence search** that only explores capture moves until the position is "quiet" (no captures, checks, etc.).

### Evaluation Function

`Evaluate` provides a numerical valuation of a given position based on the following factors:

- **Pieces:** piece values (pawn 100, knight 320, bishop 330, rook 500, queen 900, king infinite)
- **Piece-square tables:** each piece type gets a bonus or penalty depending on which square it's on. However, because of the token limit of the challenge, these are crammed into a 8-bit numbers inside bitboards of `ulong`s and are decoded at runtime.
- **Game phase weighting:** there are two scores calculated: a midgame (`mg`) and an endgame (`eg`). These are blended together based on pieces left on the board (the `phase`). As the game progresses, the piece-square tables switch to endgame, meaning that the king is more likely to come out.

### Move Ordering

If we search better moves first, then alpha-beta pruning can cut out a lot of the tree. Thus, `OrderMoves` sorts moves with cheaper heuristics before a complete search:

- Captures
- Promotions
- Penalties for moving to a place attacked by a pawn
