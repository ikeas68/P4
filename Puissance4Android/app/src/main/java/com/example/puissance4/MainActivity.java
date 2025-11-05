package com.example.puissance4;

import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.util.TypedValue;
import android.view.View;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.GridLayout;
import android.widget.ImageView;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;

import java.util.ArrayList;
import java.util.List;

public class MainActivity extends AppCompatActivity {
    private final GameBoard board = new GameBoard();
    private MinimaxAi ai = new MinimaxAi(2);
    private ImageView[][] cellViews;
    private TextView statusText;
    private Spinner difficultySpinner;
    private final Handler handler = new Handler(Looper.getMainLooper());
    private final List<GameBoard.Position> highlightedCells = new ArrayList<>();

    private boolean isPlayerTurn = true;
    private boolean isBusy = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        statusText = findViewById(R.id.statusText);
        difficultySpinner = findViewById(R.id.difficultySpinner);
        GridLayout boardGrid = findViewById(R.id.boardGrid);
        Button newGameButton = findViewById(R.id.newGameButton);

        setupBoardGrid(boardGrid);
        setupDifficultySpinner();
        newGameButton.setOnClickListener(v -> startNewGame());

        startNewGame();
    }

    private void setupBoardGrid(GridLayout boardGrid) {
        boardGrid.setRowCount(GameBoard.ROWS);
        boardGrid.setColumnCount(GameBoard.COLUMNS);

        cellViews = new ImageView[GameBoard.ROWS][GameBoard.COLUMNS];

        for (int row = 0; row < GameBoard.ROWS; row++) {
            for (int column = 0; column < GameBoard.COLUMNS; column++) {
                final int selectedColumn = column;
                ImageView cell = new ImageView(this);
                GridLayout.LayoutParams params = new GridLayout.LayoutParams(
                        GridLayout.spec(row, 1f),
                        GridLayout.spec(column, 1f)
                );
                params.width = 0;
                params.height = 0;
                int margin = dpToPx(4);
                params.setMargins(margin, margin, margin, margin);
                cell.setBackgroundResource(R.drawable.cell_background);
                cell.setScaleType(ImageView.ScaleType.CENTER_INSIDE);
                int padding = dpToPx(4);
                cell.setPadding(padding, padding, padding, padding);
                cell.setOnClickListener(v -> onColumnSelected(selectedColumn));

                boardGrid.addView(cell, params);
                cellViews[row][column] = cell;
            }
        }
    }

    private void setupDifficultySpinner() {
        difficultySpinner.setSelection(1);
        difficultySpinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                ai = new MinimaxAi(position + 1);
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {
                // No-op
            }
        });
    }

    private void onColumnSelected(int column) {
        if (!isPlayerTurn || isBusy) {
            return;
        }

        if (!board.canDrop(column)) {
            Toast.makeText(this, R.string.message_column_full, Toast.LENGTH_SHORT).show();
            return;
        }

        int row = board.dropPiece(column, GameBoard.PLAYER_ONE);
        if (row == -1) {
            return;
        }

        updateCell(row, column, GameBoard.PLAYER_ONE);
        clearHighlights();

        if (checkForEnd(row, column, GameBoard.PLAYER_ONE)) {
            return;
        }

        isPlayerTurn = false;
        isBusy = true;
        statusText.setText(R.string.status_ai_turn);

        handler.postDelayed(this::performAiMove, 400);
    }

    private void performAiMove() {
        int column = ai.chooseColumn(board, GameBoard.PLAYER_TWO, GameBoard.PLAYER_ONE);
        if (column == -1) {
            finishWithDraw();
            return;
        }

        int row = board.dropPiece(column, GameBoard.PLAYER_TWO);
        if (row == -1) {
            finishWithDraw();
            return;
        }

        updateCell(row, column, GameBoard.PLAYER_TWO);

        if (checkForEnd(row, column, GameBoard.PLAYER_TWO)) {
            return;
        }

        isPlayerTurn = true;
        isBusy = false;
        statusText.setText(R.string.status_player_turn);
    }

    private boolean checkForEnd(int row, int column, int player) {
        if (board.hasConnectedFour(player)) {
            List<GameBoard.Position> positions = board.getWinningSequence(row, column, player);
            highlightCells(positions);
            isPlayerTurn = false;
            isBusy = true;
            statusText.setText(player == GameBoard.PLAYER_ONE
                    ? R.string.status_player_wins
                    : R.string.status_ai_wins);
            return true;
        }

        if (board.isFull()) {
            finishWithDraw();
            return true;
        }

        return false;
    }

    private void finishWithDraw() {
        isPlayerTurn = false;
        isBusy = true;
        statusText.setText(R.string.status_draw);
    }

    private void startNewGame() {
        handler.removeCallbacksAndMessages(null);
        board.reset();
        isPlayerTurn = true;
        isBusy = false;
        statusText.setText(R.string.status_player_turn);
        clearHighlights();

        for (int row = 0; row < GameBoard.ROWS; row++) {
            for (int column = 0; column < GameBoard.COLUMNS; column++) {
                updateCell(row, column, 0);
            }
        }
    }

    private void updateCell(int row, int column, int player) {
        ImageView cell = cellViews[row][column];
        if (player == GameBoard.PLAYER_ONE) {
            cell.setImageResource(R.drawable.token_player_one);
        } else if (player == GameBoard.PLAYER_TWO) {
            cell.setImageResource(R.drawable.token_player_two);
        } else {
            cell.setImageDrawable(null);
        }
    }

    private void highlightCells(@NonNull List<GameBoard.Position> positions) {
        clearHighlights();
        highlightedCells.addAll(positions);
        for (GameBoard.Position position : positions) {
            cellViews[position.row][position.column].setBackgroundResource(R.drawable.cell_background_win);
        }
    }

    private void clearHighlights() {
        if (!highlightedCells.isEmpty()) {
            for (GameBoard.Position position : highlightedCells) {
                cellViews[position.row][position.column].setBackgroundResource(R.drawable.cell_background);
            }
            highlightedCells.clear();
        }
    }

    private int dpToPx(int dp) {
        return (int) TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_DIP, dp, getResources().getDisplayMetrics());
    }

    @Override
    protected void onDestroy() {
        handler.removeCallbacksAndMessages(null);
        super.onDestroy();
    }
}
