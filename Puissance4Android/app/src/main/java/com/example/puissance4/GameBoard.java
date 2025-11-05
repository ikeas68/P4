package com.example.puissance4;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public class GameBoard {
    public static final int ROWS = 6;
    public static final int COLUMNS = 7;
    public static final int PLAYER_ONE = 1;
    public static final int PLAYER_TWO = 2;

    private final int[][] cells = new int[ROWS][COLUMNS];

    public void reset() {
        for (int row = 0; row < ROWS; row++) {
            for (int column = 0; column < COLUMNS; column++) {
                cells[row][column] = 0;
            }
        }
    }

    public int getCell(int row, int column) {
        return cells[row][column];
    }

    public boolean canDrop(int column) {
        return column >= 0 && column < COLUMNS && cells[0][column] == 0;
    }

    public List<Integer> getValidMoves() {
        List<Integer> moves = new ArrayList<>();
        for (int column = 0; column < COLUMNS; column++) {
            if (canDrop(column)) {
                moves.add(column);
            }
        }
        return moves;
    }

    public int dropPiece(int column, int player) {
        if (!canDrop(column)) {
            return -1;
        }

        for (int row = ROWS - 1; row >= 0; row--) {
            if (cells[row][column] == 0) {
                cells[row][column] = player;
                return row;
            }
        }

        return -1;
    }

    public void removePiece(int column, int row) {
        if (column < 0 || column >= COLUMNS || row < 0 || row >= ROWS) {
            return;
        }

        if (cells[row][column] != 0) {
            cells[row][column] = 0;
        }
    }

    public boolean isFull() {
        for (int column = 0; column < COLUMNS; column++) {
            if (cells[0][column] == 0) {
                return false;
            }
        }
        return true;
    }

    public boolean hasConnectedFour(int player) {
        for (int row = 0; row < ROWS; row++) {
            for (int column = 0; column < COLUMNS; column++) {
                if (cells[row][column] == player && hasConnectedFourFrom(row, column, player)) {
                    return true;
                }
            }
        }
        return false;
    }

    public List<Position> getWinningSequence(int row, int column, int player) {
        if (player == 0) {
            return Collections.emptyList();
        }

        for (int[] direction : DIRECTIONS) {
            List<Position> positions = collectAlignedPositions(row, column, direction[0], direction[1], player);
            if (positions.size() >= 4) {
                return positions;
            }
        }

        return Collections.emptyList();
    }

    private boolean hasConnectedFourFrom(int row, int column, int player) {
        for (int[] direction : DIRECTIONS) {
            List<Position> positions = collectAlignedPositions(row, column, direction[0], direction[1], player);
            if (positions.size() >= 4) {
                return true;
            }
        }
        return false;
    }

    private List<Position> collectAlignedPositions(int row, int column, int dr, int dc, int player) {
        int startRow = row;
        int startColumn = column;

        while (isInside(startRow - dr, startColumn - dc) && cells[startRow - dr][startColumn - dc] == player) {
            startRow -= dr;
            startColumn -= dc;
        }

        List<Position> positions = new ArrayList<>();
        int currentRow = startRow;
        int currentColumn = startColumn;

        while (isInside(currentRow, currentColumn) && cells[currentRow][currentColumn] == player) {
            positions.add(new Position(currentRow, currentColumn));
            currentRow += dr;
            currentColumn += dc;
        }

        return positions;
    }

    private boolean isInside(int row, int column) {
        return row >= 0 && row < ROWS && column >= 0 && column < COLUMNS;
    }

    public static class Position {
        public final int row;
        public final int column;

        public Position(int row, int column) {
            this.row = row;
            this.column = column;
        }
    }

    private static final int[][] DIRECTIONS = new int[][] {
        {0, 1},
        {1, 0},
        {1, 1},
        {1, -1}
    };
}
