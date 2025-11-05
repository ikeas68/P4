package com.example.puissance4;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;

public class MinimaxAi {
    private final int maxDepth;

    public MinimaxAi(int difficulty) {
        if (difficulty <= 1) {
            maxDepth = 2;
        } else if (difficulty == 2) {
            maxDepth = 4;
        } else if (difficulty == 3) {
            maxDepth = 5;
        } else {
            maxDepth = 6;
        }
    }

    public int chooseColumn(GameBoard board, int aiPlayer, int humanPlayer) {
        List<Integer> validMoves = board.getValidMoves();
        if (validMoves.isEmpty()) {
            return -1;
        }

        List<Integer> orderedMoves = new ArrayList<>(validMoves);
        double center = GameBoard.COLUMNS / 2.0;
        Collections.sort(orderedMoves, Comparator.comparingDouble(column -> Math.abs(column - center)));

        int bestScore = Integer.MIN_VALUE;
        int bestColumn = orderedMoves.get(0);

        for (int column : orderedMoves) {
            int row = board.dropPiece(column, aiPlayer);
            int score = minimax(board, maxDepth - 1, false, aiPlayer, humanPlayer, Integer.MIN_VALUE, Integer.MAX_VALUE);
            board.removePiece(column, row);

            if (score > bestScore) {
                bestScore = score;
                bestColumn = column;
            }
        }

        return bestColumn;
    }

    private int minimax(GameBoard board, int depth, boolean maximizingPlayer, int aiPlayer, int humanPlayer, int alpha, int beta) {
        if (depth == 0 || board.isFull() || board.hasConnectedFour(aiPlayer) || board.hasConnectedFour(humanPlayer)) {
            return evaluate(board, aiPlayer, humanPlayer);
        }

        if (maximizingPlayer) {
            int value = Integer.MIN_VALUE;
            for (int column : board.getValidMoves()) {
                int row = board.dropPiece(column, aiPlayer);
                value = Math.max(value, minimax(board, depth - 1, false, aiPlayer, humanPlayer, alpha, beta));
                board.removePiece(column, row);
                alpha = Math.max(alpha, value);
                if (alpha >= beta) {
                    break;
                }
            }
            return value;
        }

        int minimizingValue = Integer.MAX_VALUE;
        for (int column : board.getValidMoves()) {
            int row = board.dropPiece(column, humanPlayer);
            minimizingValue = Math.min(minimizingValue, minimax(board, depth - 1, true, aiPlayer, humanPlayer, alpha, beta));
            board.removePiece(column, row);
            beta = Math.min(beta, minimizingValue);
            if (alpha >= beta) {
                break;
            }
        }
        return minimizingValue;
    }

    private int evaluate(GameBoard board, int aiPlayer, int humanPlayer) {
        if (board.hasConnectedFour(aiPlayer)) {
            return 1_000_000;
        }
        if (board.hasConnectedFour(humanPlayer)) {
            return -1_000_000;
        }

        int score = 0;
        int centerColumn = GameBoard.COLUMNS / 2;
        int centerCount = 0;
        for (int row = 0; row < GameBoard.ROWS; row++) {
            if (board.getCell(row, centerColumn) == aiPlayer) {
                centerCount++;
            }
        }
        score += centerCount * 6;
        score += evaluateLines(board, aiPlayer, humanPlayer);
        return score;
    }

    private int evaluateLines(GameBoard board, int aiPlayer, int humanPlayer) {
        int score = 0;

        // Horizontal
        for (int row = 0; row < GameBoard.ROWS; row++) {
            for (int column = 0; column <= GameBoard.COLUMNS - 4; column++) {
                int[] window = new int[4];
                for (int i = 0; i < 4; i++) {
                    window[i] = board.getCell(row, column + i);
                }
                score += evaluateWindow(window, aiPlayer, humanPlayer);
            }
        }

        // Vertical
        for (int column = 0; column < GameBoard.COLUMNS; column++) {
            for (int row = 0; row <= GameBoard.ROWS - 4; row++) {
                int[] window = new int[4];
                for (int i = 0; i < 4; i++) {
                    window[i] = board.getCell(row + i, column);
                }
                score += evaluateWindow(window, aiPlayer, humanPlayer);
            }
        }

        // Positive slope diagonals
        for (int row = 0; row <= GameBoard.ROWS - 4; row++) {
            for (int column = 0; column <= GameBoard.COLUMNS - 4; column++) {
                int[] window = new int[4];
                for (int i = 0; i < 4; i++) {
                    window[i] = board.getCell(row + i, column + i);
                }
                score += evaluateWindow(window, aiPlayer, humanPlayer);
            }
        }

        // Negative slope diagonals
        for (int row = 3; row < GameBoard.ROWS; row++) {
            for (int column = 0; column <= GameBoard.COLUMNS - 4; column++) {
                int[] window = new int[4];
                for (int i = 0; i < 4; i++) {
                    window[i] = board.getCell(row - i, column + i);
                }
                score += evaluateWindow(window, aiPlayer, humanPlayer);
            }
        }

        return score;
    }

    private int evaluateWindow(int[] window, int aiPlayer, int humanPlayer) {
        int aiCount = 0;
        int humanCount = 0;
        int emptyCount = 0;

        for (int value : window) {
            if (value == aiPlayer) {
                aiCount++;
            } else if (value == humanPlayer) {
                humanCount++;
            } else {
                emptyCount++;
            }
        }

        int score = 0;

        if (aiCount == 4) {
            score += 5_000;
        } else if (aiCount == 3 && emptyCount == 1) {
            score += 150;
        } else if (aiCount == 2 && emptyCount == 2) {
            score += 20;
        }

        if (humanCount == 4) {
            score -= 5_000;
        } else if (humanCount == 3 && emptyCount == 1) {
            score -= 180;
        } else if (humanCount == 2 && emptyCount == 2) {
            score -= 18;
        }

        return score;
    }
}
