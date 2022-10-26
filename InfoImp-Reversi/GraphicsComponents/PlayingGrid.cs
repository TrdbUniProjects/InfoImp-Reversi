using Eto.Drawing;
using Eto.Forms;
using InfoImp_Reversi.Exceptions;
using InfoImp_Reversi.State;

namespace InfoImp_Reversi.GraphicsComponents;

public class PlayingGrid : Drawable {

    /**
     * The default amount of cells
     * <seealso cref="CellCount"/>
     */
    public const int DefaultCellCount = 6;
    
    /**
     * The amount of cells in a direction on the board
     * E.g. if this is 4, the board is 4x4
     */
    public int CellCount { get; set; }
    
    /**
     * Array that keeps track of the state of each cell on the board.
     * Indexed by the cell coordinates
     */
    private CellState[,] _cellStates;
    
    private readonly WindowContent _windowContent;
    
    public PlayingGrid(int? cellCount, ref WindowContent windowContent) {
        this._windowContent = windowContent;
        
        this.Paint += this.OnPaintEvent;
        this.MouseDown += this.OnMouseDownEvent;
        
        this.CellCount = cellCount ?? DefaultCellCount;
        
        // Cant have uneven sizes due to the starting positions
        if (!IsSizeValid(this.CellCount)) {
            throw new InvalidDataException("Cellcount must be an even number");
        }
        
        this._cellStates = new CellState[this.CellCount, this.CellCount];
        this.SetStartingStones();
    }

    public static bool IsSizeValid(int size) {
        return size % 2 == 0;
    }
    
    public void ResetGrid() {
        this._cellStates = new CellState[this.CellCount, this.CellCount];
        this.SetStartingStones();
    }
    
    /// <summary>
    /// Set the starting stone configuration for the game.
    /// This will place two stone per player in the center of the
    /// game in the following pattern:
    /// <code>
    /// XO
    /// OX
    /// </code>
    /// </summary>
    private void SetStartingStones() {
        int topLeftCenter = this.CellCount / 2 - 1;
        this._cellStates[topLeftCenter, topLeftCenter] = CellState.Player1;
        this._cellStates[topLeftCenter + 1, topLeftCenter] = CellState.Player2;
        this._cellStates[topLeftCenter, topLeftCenter + 1] = CellState.Player2;
        this._cellStates[topLeftCenter + 1, topLeftCenter + 1] = CellState.Player1;
    }

    private void OnPaintEvent(object? sender, PaintEventArgs args) {
        int cellSize = (int) Math.Floor(this.Width / (float) this.CellCount);
        
        Graphics g = args.Graphics;
        
        this.DrawGrid(ref g, cellSize);
        this.DrawCells(ref g, cellSize);

        bool playerCanPlay = this.DrawHelpCellsIfEnabled(ref g, cellSize);

        if (playerCanPlay) {
            GameManager.PlayerCouldNotPlay = false;
        } else {
            if (GameManager.PlayerCouldNotPlay) {
                if (this.CheckForWinnerAndUpdateLabels()) {
                    return;
                }
            } else {
                GameManager.PlayerCouldNotPlay = true;
                GameManager.SwapPlayingPlayer();
            }
        }
        
        g.Flush();
    }

    /// <summary>
    /// Draw the gridlines of the game
    /// </summary>
    /// <param name="graphics">A reference to the Graphics object</param>
    /// <param name="cellSize">The size of a one cell</param>
    private void DrawGrid(ref Graphics graphics, int cellSize) {
        Pen pen = new Pen(Colors.Black);
        
        for (int i = 1; i < this.CellCount; i++) {
            int offset = i * cellSize;
            
            // Rows
            graphics.DrawLine(pen, 0, offset, this.Width, offset);
            // Columns
            graphics.DrawLine(pen, offset, 0, offset, this.Height);
        }
    }

    /// <summary>
    /// Draw all stones in cells in the game. 
    /// </summary>
    /// <param name="graphics">A reference to the Graphics object</param>
    /// <param name="cellSize">The size of one cells</param>
    private void DrawCells(ref Graphics graphics, int cellSize) {
        for (int x = 0; x < this.CellCount; x++) {
            for (int y = 0; y < this.CellCount; y++) {
                this.DrawCell(ref graphics, x, y, cellSize);
            }
        }
    }

    /// <summary>
    /// Draw the contents of a single cell
    /// </summary>
    /// <param name="graphics">A reference to the Graphics object</param>
    /// <param name="cellX">
    ///     The X coordinate of the cell.
    ///     If the grid is 4x4, the bottom right cell has a coordinate of (3, 3)
    /// </param>
    /// <param name="cellY">
    ///     The Y coordinate of the cell.
    ///     If the grid is 4x4, the bottom right cell has a coordinate of (3, 3)
    /// </param>
    /// <param name="cellSize">The size of a single cell</param>
    /// <param name="helpCell">Whether this cell is a help cell</param>
    private void DrawCell(ref Graphics graphics, int cellX, int cellY, int cellSize, bool helpCell = false) {
        // The diameter of the circle to draw
        float circleSize = cellSize * 0.7f;
        // Compute beforehand to save a multiplication
        float halfSize = cellSize / 2f;
        
        // The center of the cell
        // In pixel coordinates
        float cx = cellX * cellSize + halfSize;
        float cy = cellY * cellSize + halfSize;
        
        // Get the state of the cell and the color associated with the state
        CellState cellState = this._cellStates[cellX, cellY];
        Color color = GetColor(cellState);

        float halfCircle = circleSize / 2f;
        float posX = cx - halfCircle;
        float posY = cy - halfCircle;
        
        if (helpCell) {
            // Draw a stone, but only the outline
            graphics.DrawEllipse(Colors.Gray, posX, posY, circleSize, circleSize);
            graphics.Flush();
        } else {
            // Draw the stone
            graphics.FillEllipse(color, posX, posY, circleSize, circleSize);
        }
    }

    /// <summary>
    /// Get the color associated with a cell state
    /// </summary>
    /// <param name="cellState">The state of the cell</param>
    /// <returns>The color of the cell</returns>
    /// <exception cref="NotImplementedException"></exception>
    private static Color GetColor(CellState cellState) {
        return cellState switch {
            CellState.None => Color.FromArgb(0, 0, 0, 0),
            CellState.Player1 => GameManager.Player1Color,
            CellState.Player2 => GameManager.Player2Color,
            _ => throw new CaseNotImplementedException()
        };
    }
 
    /// <summary>
    /// Called when a mouse button is pressed.
    /// This event handler will only handle the primary mouse button
    /// </summary>
    /// <param name="sender">The object responsible for invoking this function</param>
    /// <param name="args">The associated events</param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnMouseDownEvent(object? sender, MouseEventArgs args) {
        // We only care for the primary button
        if (args.Buttons != MouseButtons.Primary) {
            return;
        }
        
        // Dont handle clicks once the game is over
        if (GameManager.IsGameComplete) {
            return;
        }
        
        int cellSize = this.Width / this.CellCount;
        // Get the Cell coordinate where the mouse clicked
        int cellX = (int) (args.Location.X / cellSize);
        int cellY = (int) (args.Location.Y / cellSize);
        
        CellState currentCellState = this._cellStates[cellX, cellY];
        
        // If the cell is occupied, no actions can be performed
        if (currentCellState != CellState.None) {
            return;
        }
        
        // Get the states of all neighboring cells
        // Note that some fields will be null if the cell is on a corner or a side
        NeighborStates neighborStates = this.GetNeighborStates(cellX, cellY);

        // Check if the placement of the stone is valid
        if (!neighborStates.IsPlacementValid(GameManager.PlayerAtSet)) {
            return;
        }

        // Retrieve the state the cell should be, and update it accordingly
        CellState newState = this.GetPlayingPlayerCellState(GameManager.PlayerAtSet);
        this._cellStates[cellX, cellY] = newState;
        
        // Propagate the change out to other cells
        this.FlipStones(cellX, cellY, GameManager.PlayerAtSet);
        
        // Invalidate this to redraw the cells
        this.Invalidate();
        // Redraw the window content to update fields
        this._windowContent.Invalidate();

        if (this.CheckForWinnerAndUpdateLabels()) {
            return;
        }

        GameManager.SwapPlayingPlayer();
    }

    /**
     * Check whether there was a winner and update the score and winner labels accordingly
     * <returns><code>true</code> if there was a winner</returns>
     */
    private bool CheckForWinnerAndUpdateLabels() {
        (int, int)[] player1Cells = this.GetAllCellsOfState(CellState.Player1);
        (int, int)[] player2Cells = this.GetAllCellsOfState(CellState.Player2);
        
        this._windowContent.SetScoreText(player1Cells.Length, player2Cells.Length);

        if (player1Cells.Length + player2Cells.Length != this.CellCount * this.CellCount) {
            return false;
        }
        
        string playerWon = player1Cells.Length > player2Cells.Length ? "1" : "2";
        this._windowContent.SetInfoLabel($"Player {playerWon} has won!");
        GameManager.IsGameComplete = true;
        return true;

    }

    /**
     * Draw help cells, if it is enabled. These are cells indicating to the player where they may place their next stone.
     * If the help cells are disabled, it'll only return whether there would be at least one help cell drawn. 
     * <returns>Returns <code>true</code> if there was at least one possible move</returns>
     */
    private bool DrawHelpCellsIfEnabled(ref Graphics graphics, int cellSize) {
        CellState wantedState = GameManager.GetOtherPlayerCellState(GameManager.PlayerAtSet);
        (int x, int y)[] cellsOfState = this.GetAllCellsOfState(wantedState);

        bool anySetPossible = false;
        
        foreach ((int x, int y) in cellsOfState) {
            NeighborStates neighborStates = this.GetNeighborStates(x, y);
            IEnumerable<(int x, int y)> eligibleNeighbors = neighborStates.GetNeighborCellsOfState(CellState.None, x, y);
            foreach ((int nx, int ny) in eligibleNeighbors) {
                if (this.GetCellState(nx, ny) != CellState.None) {
                    continue;
                }
                
                if (GameManager.IsHelpModeEnabled) {
                    this.DrawCell(ref graphics, nx, ny, cellSize, true);
                }
                anySetPossible = true;
            }
        }

        return anySetPossible;
    }

    /**
     * Get all cells on the board with a specific state
     */
    private (int x, int y)[] GetAllCellsOfState(CellState requiredState) {
        List<(int x, int y)> result = new List<(int x, int y)>();
        for (int x = 0; x < this.CellCount; x++) {
            for (int y = 0; y < this.CellCount; y++) {
                CellState state = this._cellStates[x, y];
                if (state == requiredState) {
                    result.Add((x, y));
                }
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// The neighboring states of a cell
    /// </summary>
    private struct NeighborStates {
        /// <summary>
        /// The neighboring cell states
        /// </summary>
        public CellState? TopLeft, Top, TopRight, Left, Right, BottomLeft, Bottom, BottomRight;

        /// <summary>
        /// Check if the placement of a stone is valid.
        /// A placement is only valid if any of the cells around it are from the other player
        /// </summary>
        /// <param name="playingPlayer">The player at set</param>
        /// <returns>Whether the placement is valid</returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsPlacementValid(PlayingPlayer playingPlayer) {
            return playingPlayer switch {
                PlayingPlayer.Player1 => this.IsAnyCellOfState(CellState.Player2),
                PlayingPlayer.Player2 => this.IsAnyCellOfState(CellState.Player1),
                _ => throw new CaseNotImplementedException()
            };
        }

        /// <summary>
        /// Check if any of the surrounding cells have the state provided
        /// </summary>
        /// <param name="cellState">The state to check</param>
        /// <returns>Whether any of the surrounding cells have the state provided</returns>
        private bool IsAnyCellOfState(CellState cellState) {
            if (this.TopLeft == cellState) return true;
            if (this.Top == cellState) return true;
            if (this.TopRight == cellState) return true;
            if (this.Left == cellState) return true;
            if (this.Right == cellState) return true;
            if (this.BottomLeft == cellState) return true;
            if (this.Bottom == cellState) return true;
            // Could elide the if statement
            // but it's more readable this way
            if (this.BottomRight == cellState) return true;

            return false;
        }

        /**
         * Get the neighboring cells of a cell that have a set state
         */
        public IEnumerable<(int x, int y)> GetNeighborCellsOfState(CellState cellState, int x, int y) {
            List<(int x, int y)> result = new List<(int x, int y)>();
            if (this.TopLeft != null && this.TopLeft == cellState) result.Add((x - 1, y - 1));
            if (this.Top != null && this.Top == cellState) result.Add((x, y - 1));
            if (this.TopRight != null && this.TopRight == cellState) result.Add((x + 1, y - 1));
            if (this.Left != null && this.Left == cellState) result.Add((x - 1, y));
            if (this.Right != null && this.Right == cellState) result.Add((x + 1, y));
            if (this.BottomLeft != null && this.BottomLeft == cellState) result.Add((x - 1, y + 1));
            if (this.Bottom != null && this.Bottom == cellState) result.Add((x, y + 1));
            if (this.BottomRight != null && this.BottomRight == cellState) result.Add((x + 1, y + 1));

            return result.ToArray();
        }
    }
    
    /// <summary>
    /// Get the cell states of neighboring cells
    /// </summary>
    /// <param name="cellX">The cell's X coordinate</param>
    /// <param name="cellY">The cell's Y coordinate</param>
    /// <returns>Returns the state of the neighbors around the cell</returns>
    /// <exception cref="NotImplementedException"></exception>
    private NeighborStates GetNeighborStates(int cellX, int cellY) {
        // Check if the cell is a corner
        // A corner cell only has 3 valid neighbors
        if (this.IsCellCorner(cellX, cellY)) {
            // Which neighbor depends on which corner the cell is located ats
            return this.GetCellCornerPosition(cellX, cellY) switch {
                CellCornerPosition.TopLeft =>
                    // Can only check cell to the right and cell below
                    new NeighborStates {
                        Right = this._cellStates[1, 0],
                        Bottom = this._cellStates[0, 1],
                        BottomRight = this._cellStates[1, 1],
                    },
                CellCornerPosition.TopRight =>
                    // Can only check cell to the left and cell below
                    new NeighborStates {
                        Left = this._cellStates[cellX - 1, 0],
                        Bottom = this._cellStates[cellX, 1],
                        BottomLeft = this._cellStates[cellX - 1, cellY + 1],
                    },
                CellCornerPosition.BottomLeft =>
                    // Can only check cell to the right and cell to the top
                    new NeighborStates {
                        Right = this._cellStates[1, cellY],
                        Top = this._cellStates[0, cellY - 1],
                        TopRight = this._cellStates[cellX + 1, cellY - 1],
                    },
                CellCornerPosition.BottomRight =>
                    // Can only check cell to the left and cell to the top
                    new NeighborStates {
                        Left = this._cellStates[cellX - 1, cellY],
                        Top = this._cellStates[cellX, cellY - 1],
                        TopLeft = this._cellStates[cellX - 1, cellY - 1],
                    },
                _ => throw new CaseNotImplementedException()
            };
        }
        
        CellSide? cellSide = this.GetCellSide(cellX, cellY);
        // Side, but not a corner
        // A cell on a side has 5 valid neighbors
        if (cellSide != null) {
            // Which valid neightbor depends on which side the cell is located at
            return cellSide switch {
                CellSide.Top =>
                    new NeighborStates {
                        Left = this._cellStates[cellX - 1, cellY],
                        Right = this._cellStates[cellX + 1, cellY],
                        Bottom = this._cellStates[cellX, cellY + 1],
                        BottomLeft = this._cellStates[cellX - 1, cellY + 1],
                        BottomRight = this._cellStates[cellX + 1, cellY + 1],
                    },
                CellSide.Left =>
                    new NeighborStates {
                        Top = this._cellStates[cellX, cellY + 1],
                        Bottom = this._cellStates[cellX, cellY - 1],
                        Right = this._cellStates[cellX + 1, cellY],
                        TopRight = this._cellStates[cellX + 1, cellY - 1],
                        BottomRight = this._cellStates[cellX + 1, cellY + 1],
                    },
                CellSide.Right =>
                    new NeighborStates {
                        Top = this._cellStates[cellX, cellY + 1],
                        Bottom = this._cellStates[cellX, cellY - 1],
                        Left = this._cellStates[cellX - 1, cellY],
                        TopLeft = this._cellStates[cellX - 1, cellY + 1],
                        BottomLeft = this._cellStates[cellX - 1, cellY - 1],
                    },
                CellSide.Bottom =>
                    new NeighborStates {
                        Top = this._cellStates[cellX, cellY - 1],
                        Left = this._cellStates[cellX - 1, cellY],
                        Right = this._cellStates[cellX + 1, cellY],
                        TopLeft = this._cellStates[cellX - 1, cellY - 1],
                        TopRight = this._cellStates[cellX + 1, cellY - 1],
                    },
                _ => throw new CaseNotImplementedException()
            };
        }
        
        // Not a side nor a corner
        // This is a cell not in the outer ring of the playing field
        // thus it has 8 valid neighbors
        return new NeighborStates {
            TopLeft = this._cellStates[cellX - 1, cellY - 1],
            Top = this._cellStates[cellX, cellY - 1],
            TopRight = this._cellStates[cellX + 1, cellY - 1],
            Left = this._cellStates[cellX - 1, cellY],
            Right = this._cellStates[cellX + 1, cellY],
            BottomLeft = this._cellStates[cellX - 1, cellY + 1],
            Bottom = this._cellStates[cellX, cellY + 1],
            BottomRight = this._cellStates[cellX + 1, cellY + 1],
        };
    }

    /// <summary>
    /// The side at which a cell sits
    /// </summary>
    private enum CellSide {
        Top,
        Left,
        Right,
        Bottom,
    }

    /// <summary>
    /// Get the CellState associated with the player currently playing
    /// </summary>
    /// <param name="playingPlayer">The player currently playing</param>
    /// <returns>The CellState</returns>
    /// <exception cref="NotImplementedException"></exception>
    private CellState GetPlayingPlayerCellState(PlayingPlayer playingPlayer) {
        return playingPlayer switch {
            PlayingPlayer.Player1 => CellState.Player1,
            PlayingPlayer.Player2 => CellState.Player2,
            _ => throw new CaseNotImplementedException()
        };
    }

    /// <summary>
    /// Propagate the Cell states that occured after a player made a move.
    /// This checks every direction from the cell at which the move was made.
    /// </summary>
    /// <param name="cellX">The X coordinate of the cell</param>
    /// <param name="cellY">The Y coordinate of the cell</param>
    /// <param name="playingPlayer">The player currently at move</param>
    private void FlipStones(int cellX, int cellY, PlayingPlayer playingPlayer) {
        // Diagonal top left
        this.ProcessCellDirection(
            playingPlayer, 
            (cellX, cellY),
            (tx, ty) => tx > 0 && ty > 0,
            (tx, ty) => (--tx, --ty)
        );
        
        // Top
        this.ProcessCellDirection(
            playingPlayer,
            (cellX, cellY),
            (_, ty) => ty > 0,
            (tx, ty) => (tx, --ty)
        );
        
        // Diagonal top right
        this.ProcessCellDirection(
            playingPlayer,
            (cellX, cellY),
            (tx, ty) => tx < this.CellCount - 1 && ty > 0,
            (tx, ty) => (++tx, --ty)
        );
        
        // Right
        this.ProcessCellDirection(
            playingPlayer,
            (cellX, cellY),
            (tx, _) => tx < this.CellCount - 1,
            (tx, ty) => (++tx, ty) 
        );
        
        // Diagonal bottom right
        this.ProcessCellDirection(
            playingPlayer,
            (cellX, cellY),
            (tx, ty) => tx < this.CellCount - 1 && ty < this.CellCount - 1,
            (tx, ty) => (++tx, ++ty)
        );
        
        // Bottom
        this.ProcessCellDirection(
            playingPlayer,
            (cellX, cellY),
            (_, ty) => ty < this.CellCount - 1,
            (tx, ty) => (tx, ++ty)
        );
        
        // Diagonal bottom left
        this.ProcessCellDirection(
            playingPlayer,
            (cellX, cellY),
            (tx, ty) => tx > 0 && ty < this.CellCount - 1,
            (tx, ty) => (--tx, ++ty)
        );
        
        // Left
        this.ProcessCellDirection(
            playingPlayer,
            (cellX, cellY),
            (tx, _) => tx > 0,
            (tx, ty) => (--tx, ty)
        );
    }

    /// <summary>
    /// Process the cell state changes resulting from a player move in one direction
    /// </summary>
    /// <param name="playingPlayer">The player currently at set</param>
    /// <param name="cellPos">The X and Y coordinates of the cell</param>
    /// <param name="condition">A function returning whether the input coordinates are still valid (i.e. within array bounds)</param>
    /// <param name="apply">A function returning the transformed cell coordinates</param>
    private void ProcessCellDirection(PlayingPlayer playingPlayer, (int x, int y) cellPos, Func<int, int, bool> condition, Func<int, int, (int x, int y)> apply) {
        CellState otherPlayerState = GameManager.GetOtherPlayerCellState(playingPlayer);
        
        int x = cellPos.x;
        int y = cellPos.y;
        
        List<(int, int)> cellsToFlip = new List<(int, int)>();

        while (condition(x, y)) {
            (x, y) = apply(x, y);
            
            CellState cellState = this.GetCellState(x, y);
            if (cellState == CellState.None) {
                cellsToFlip.Clear();
                break;
            }

            if (cellState == otherPlayerState) {
                cellsToFlip.Add((x, y));
                continue;
            }

            foreach ((int xFlip, int yFlip) in cellsToFlip) {
                this.FlipStone(xFlip, yFlip);
            }
            
            cellsToFlip.Clear();
            break;
        }
    }

    /// <summary>
    /// Get the state of a cell
    /// </summary>
    /// <param name="cellX">The Cell's X coordinate</param>
    /// <param name="cellY">The Cell's Y coordinate</param>
    /// <returns>The state of the cell</returns>
    private CellState GetCellState(int cellX, int cellY) {
        return this._cellStates[cellX, cellY];
    }

    /// <summary>
    /// Flip a stone to the other state
    /// </summary>
    /// <param name="cellX">The X coordinate of the cell</param>
    /// <param name="cellY">The Y coordinate of the cell</param>
    /// <exception cref="InvalidDataException">If the Cell has a CellState of None</exception>
    /// <exception cref="NotImplementedException"></exception>
    private void FlipStone(int cellX, int cellY) {
        CellState currentCellState = this.GetCellState(cellX, cellY);
        CellState newCellState = currentCellState switch {
            CellState.None => throw new InvalidDataException("Cell state may nont be None now"),
            CellState.Player1 => CellState.Player2,
            CellState.Player2 => CellState.Player1,
            _ => throw new CaseNotImplementedException()
        };

        this.SetCellState(cellX, cellY, newCellState);
    }

    /// <summary>
    /// Set the state of a cell
    /// </summary>
    /// <param name="cellX">The X coordinate of the cell</param>
    /// <param name="cellY">The Y coordinate of the cell</param>
    /// <param name="state">The state to set</param>
    private void SetCellState(int cellX, int cellY, CellState state) {
        this._cellStates[cellX, cellY] = state;
    }

    /// <summary>
    /// Get the side on which the cell is located
    /// </summary>
    /// <param name="cellX">The X coordinate of the cell</param>
    /// <param name="cellY">The Y coordinate of the cell</param>
    /// <returns>The side the cell is located on, or null if it is not located on any side
    /// </returns>
    private CellSide? GetCellSide(int cellX, int cellY) {
        int cellCount = this.CellCount;
        
        if (cellX > 0 && cellX < cellCount - 1 && cellY == 0) {
            return CellSide.Top;
        }

        if (cellX == 0 && cellY > 0 && cellY < cellCount - 1) {
            return CellSide.Left;
        }

        if (cellX == cellCount - 1 && cellY > 0 && cellY < cellCount - 1) {
            return CellSide.Right;
        }

        if (cellX > 0 && cellX < cellCount - 1 && cellY == cellCount - 1) {
            return CellSide.Bottom;
        }

        return null;
    }

    /// <summary>
    /// Check whether the cell is on the corner of the playing field
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <returns>Whether a cell lays on the corner of the playing field</returns>
    private bool IsCellCorner(int cellX, int cellY) {
        return (cellX == 0 || cellX == this.CellCount - 1) && // Top left, right 
               (cellY == 0 || cellY == this.CellCount - 1); // Bottom left, right
    }

    /// <summary>
    /// The corner position of a cell
    /// </summary>
    private enum CellCornerPosition {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }

    /// <summary>
    /// Check which corner position a cell is located at
    /// </summary>
    /// <param name="cellX">The X coordinate of the cell</param>
    /// <param name="cellY">The Y coordinate of the cell</param>
    /// <returns></returns>
    private CellCornerPosition? GetCellCornerPosition(int cellX, int cellY) {
        if (cellX == 0 && cellY == 0) {
            return CellCornerPosition.TopLeft;
        } 
        
        if (cellX == this.CellCount - 1 && cellY == 0) {
            return CellCornerPosition.TopRight;
        }

        if (cellX == 0 && cellY == this.CellCount - 1) {
            return CellCornerPosition.BottomLeft;
        }

        if (cellX == this.CellCount - 1 && cellY == this.CellCount - 1) {
            return CellCornerPosition.BottomRight;
        }
        
        return null;
    }
}