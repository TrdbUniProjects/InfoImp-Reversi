using Eto.Drawing;
using Eto.Forms;
using InfoImp_Reversi.State;

namespace InfoImp_Reversi.GraphicsComponents;

public class WindowContent : Drawable {

    /// <summary>
    /// The label showing who is at set
    /// </summary>
    private readonly Label _playerAtSet;
    /// <summary>
    /// The playing grid
    /// </summary>
    private readonly PlayingGrid _grid;
    /// <summary>
    /// Lable showing informational text to the user
    /// </summary>
    private readonly Label _infoTextLabel = new Label();
    /// <summary>
    /// Textbox where the user can entire the board's size
    /// </summary>
    private readonly TextBox _sizeTextBox = new TextBox {
        Text = PlayingGrid.DefaultCellCount.ToString()
    };
    /// <summary>
    /// Score for player 1
    /// </summary>
    private readonly Label _player1Stones = new Label {
        Font = new Font(SystemFont.Default, 20),
        VerticalAlignment = VerticalAlignment.Center,
        Text = "2 stones"
    };
    /// <summary>
    /// Score for player 2
    /// </summary>
    private readonly Label _player2Stones = new Label {
        Font = new Font(SystemFont.Default, 20),
        VerticalAlignment = VerticalAlignment.Center,
        Text = "2 stones",
    };
    
    public WindowContent() {
        this._playerAtSet = new Label();
        WindowContent self = this;
        this._grid = new PlayingGrid(PlayingGrid.DefaultCellCount, ref self) {
            Size = new Size(400, 400)
        };
        
        this.Paint += this.OnPaintEvent;
        
        Button resetBtn = new Button {
            Text = "Reset"
        };
        resetBtn.Click += this.OnResetBtnClicked;

        Button helpModeBtn = new Button {
            Text = "Help"
        };
        helpModeBtn.Click += this.OnHelpModeBtnClicked;

        this.Content = new StackLayout {
            Items = {
                this._infoTextLabel,
                new TableLayout {
                    Spacing = new Size(10, 5),
                    Rows = {
                        new TableRow {
                            Cells = {
                                new Label {
                                    Text = "Size:",
                                    VerticalAlignment = VerticalAlignment.Center,
                                },
                                this._sizeTextBox,
                                new Label {
                                    Text = "cells",
                                    VerticalAlignment = VerticalAlignment.Center
                                }
                            }
                        },
                        new TableRow {
                            Cells = {
                                resetBtn,
                                helpModeBtn
                            }
                        }
                    }
                },
                this._playerAtSet,
                new TableLayout {
                    Spacing = new Size(30, 0),
                    Rows = {
                        new TableRow {
                            Cells = {
                                this._grid,
                                new TableLayout {
                                    Spacing = new Size(10, 10),
                                    Rows = {
                                        new TableRow {
                                            Cells = {
                                                new ScoreStone(GameManager.Player1Color) {
                                                    Size = new Size(50, 50)
                                                },
                                                this._player1Stones
                                            }
                                        },
                                        new TableRow {
                                            Cells = {
                                                new ScoreStone(GameManager.Player2Color) {
                                                    Size = new Size(50, 50)
                                                },
                                                this._player2Stones
                                            }
                                        },
                                        new TableRow(),
                                    }
                                }
                            } 
                        }
                    }
                }
            }
        };
    }

    /**
     * Set the score fields 
     */
    public void SetScoreText(int player1StoneCount, int player2StoneCount) {
        this._player1Stones.Text = $"{player1StoneCount} stones";
        this._player1Stones.Invalidate();

        this._player2Stones.Text = $"{player2StoneCount} stones";
        this._player2Stones.Invalidate();
    }
    
    private void OnHelpModeBtnClicked(object? sender, EventArgs args) {
        GameManager.IsHelpModeEnabled = !GameManager.IsHelpModeEnabled;
        this._grid.Invalidate();
    }
    
    private void OnResetBtnClicked(object? sender, EventArgs args) {
        this._infoTextLabel.Text = "";
        this._infoTextLabel.Invalidate();
        
        this.UpdateSizeIfNeeded();
        this.ResetGame();
    }

    /**
     * Update the size of the playing board if needed
     */
    private void UpdateSizeIfNeeded() {
        (bool shouldChange, int? newSize) = this.ShouldBoardSizeChange();
        if (!shouldChange) {
            return;
        }

        int newSizeNotNull = newSize ?? default(int);
        if (!PlayingGrid.IsSizeValid(newSizeNotNull)) {
            this._infoTextLabel.Text = $"Size {newSize} is not valid. It must be an even number";
            this._infoTextLabel.Invalidate();
            return;
        }
        
        this._grid.CellCount = newSize ?? default(int);
    }

    /**
     * Reset the entire game
     */
    private void ResetGame() {
        GameManager.PlayerAtSet = PlayingPlayer.Player1;
        GameManager.IsGameComplete = false;
        
        this._playerAtSet.Invalidate();
        this._grid.ResetGrid();
        this._grid.Invalidate();
    }

    /**
     * Check if the board size should change. If the boolean of the pair is true, the int will be non-null
     */
    private (bool, int?) ShouldBoardSizeChange() {
        int currentCount = this._grid.CellCount;
        
        // Parse the new count;
        bool isValid = Util.IsValidInt(this._sizeTextBox.Text);
        if (!isValid) {
            this._infoTextLabel.Text = $"Size has an invalid value: {this._sizeTextBox.Text}";
            this._infoTextLabel.Invalidate();
            return (false, null);
        }

        int newSize = int.Parse(this._sizeTextBox.Text);
        return (newSize != currentCount, newSize);
    }
    
    private void OnPaintEvent(object? sender, PaintEventArgs args) {
        switch (GameManager.PlayerAtSet) {
            case PlayingPlayer.Player1:
                this._playerAtSet.Text = "At set: Player 1";
                this._playerAtSet.TextColor = GameManager.Player1Color;
                break;
            case PlayingPlayer.Player2:
                this._playerAtSet.Text = "At set: Player 2";
                this._playerAtSet.TextColor = GameManager.Player2Color;
                break;
            default:
                throw new NotImplementedException("Case not implemented");
        }
        this._playerAtSet.Invalidate();
    }

    /**
     * Set the text of the info label
     */
    public void SetInfoLabel(string text) {
        this._infoTextLabel.Text = text;
        this._infoTextLabel.Invalidate();
    }
    
}