using Eto.Drawing;
using Eto.Forms;

namespace InfoImp_Reversi.GraphicsComponents;

public class PlayingGrid : Drawable {

    private const int DefaultCellCount = 4;
    private readonly int _cellCount;
    private readonly CellState[,] _cellStates;

    private readonly WindowContent _windowContent;
    
    public PlayingGrid(int? cellCount, ref WindowContent windowContent) {
        this._windowContent = windowContent;
        
        if (cellCount % 2 != 0) {
            throw new InvalidDataException("Cellcount must be an even number");
        }
        
        this.Paint += this.OnPaintEvent;
        this.MouseDown += this.OnMouseDownEvent;
        
        this._cellCount = cellCount ?? DefaultCellCount;
        this._cellStates = new CellState[this._cellCount, this._cellCount];

        int topLeftCenter = this._cellCount / 2 - 1;
        
        this._cellStates[topLeftCenter, topLeftCenter] = CellState.Player1;
        this._cellStates[topLeftCenter + 1, topLeftCenter] = CellState.Player2;
        this._cellStates[topLeftCenter, topLeftCenter + 1] = CellState.Player2;
        this._cellStates[topLeftCenter + 1, topLeftCenter + 1] = CellState.Player1;
    }

    private void OnPaintEvent(object? sender, PaintEventArgs args) {
        int cellSize = (int) Math.Floor(this.Width / (float) this._cellCount);
        
        Graphics g = args.Graphics;
        
        this.DrawGrid(ref g, cellSize);
        this.DrawCells(ref g, cellSize);
        
        g.Flush();
    }

    private void DrawGrid(ref Graphics graphics, int cellSize) {
        Pen pen = new Pen(Colors.Black);
        
        for (int row = 1; row < this._cellCount; row++) {
            int rowY = row * cellSize;
            graphics.DrawLine(pen, 0, rowY, this.Width, rowY);
        }

        for (int col = 1; col < this._cellCount; col++) {
            int colX = col * cellSize;
            graphics.DrawLine(pen, colX, 0, colX, this.Height);
        }
    }

    private void DrawCells(ref Graphics graphics, int cellSize) {
        for (int x = 0; x < this._cellCount; x++) {
            for (int y = 0; y < this._cellCount; y++) {
                this.DrawCell(ref graphics, x, y, cellSize);
            }
        }
    }

    private void DrawCell(ref Graphics graphics, int cellX, int cellY, int cellSize) {
        float circleSize = cellSize * 0.7f;
        float halfSize = cellSize / 2f;
        
        float cx = (cellX * cellSize) + halfSize;
        float cy = (cellY * cellSize) + halfSize;

        CellState cellState = this._cellStates[cellX, cellY];
        Color color = this.GetColor(cellState);
        graphics.FillEllipse(color, cx - circleSize / 2f, cy - circleSize / 2f, circleSize, circleSize);
    }

    private Color GetColor(CellState cellState) {
        switch (cellState) {
            case CellState.None:
                return Color.FromArgb(0, 0, 0, 0);
            case CellState.Player1:
                return GameManager.PlayerAColor;
            case CellState.Player2:
                return GameManager.PlayerBColor;
            default:
                throw new NotImplementedException("Case not implemented");
        }
    }
 
    private void OnMouseDownEvent(object? sender, MouseEventArgs args) {
        if (args.Buttons != MouseButtons.Primary) {
            return;
        }
        
        int cellSize = this.Width / this._cellCount;
        int cellX = (int) (args.Location.X / cellSize);
        int cellY = (int) (args.Location.Y / cellSize);
        
        CellState currentCellState = this._cellStates[cellX, cellY];
        if (currentCellState != CellState.None) {
            return;
        }
        
        NeighborStates neighborStates = this.GetNeighborStates(cellX, cellY, this._cellCount);

        if (!neighborStates.IsPlacementValid(GameManager.PlayerAtSet)) {
            return;
        }

        CellState newState;
        switch (GameManager.PlayerAtSet) {
            case PlayingPlayer.Player1:
                newState = CellState.Player1;
                break;
            case PlayingPlayer.Player2:
                newState = CellState.Player2;
                break;
            default:
                throw new NotImplementedException("Case not implemented");
        }

        this._cellStates[cellX, cellY] = newState;
        this.FlipStones(cellX, cellY, this._cellCount, GameManager.PlayerAtSet);
        
        this.Invalidate();
        this._windowContent.Invalidate();

        switch (GameManager.PlayerAtSet) {
            case PlayingPlayer.Player1:
                GameManager.PlayerAtSet = PlayingPlayer.Player2;
                break;
            case PlayingPlayer.Player2:
                GameManager.PlayerAtSet = PlayingPlayer.Player1;
                break;
            default:
                throw new NotImplementedException("Case not implemented");
        }
    }

    struct NeighborStates {
        public CellState? TopLeft, Top, TopRight, Left, Right, BottomLeft, Bottom, BottomRight;

        public bool IsPlacementValid(PlayingPlayer playingPlayer) {
            switch (playingPlayer) {
                case PlayingPlayer.Player1:
                    return this.IsAnyCellOfState(CellState.Player2);
                case PlayingPlayer.Player2:
                    return this.IsAnyCellOfState(CellState.Player1);
                default:
                    throw new NotImplementedException("Case not implemented");
            }
        }

        private bool IsAnyCellOfState(CellState cellState) {
            if (this.TopLeft == cellState) return true;
            if (this.Top == cellState) return true;
            if (this.TopRight == cellState) return true;
            if (this.Left == cellState) return true;
            if (this.Right == cellState) return true;
            if (this.BottomLeft == cellState) return true;
            if (this.Bottom == cellState) return true;
            if (this.BottomRight == cellState) return true;

            return false;
        }
    }
    
    private NeighborStates GetNeighborStates(int x, int y, int cellCount) {
        // Corner
        if (this.IsCellCorner(x, y, cellCount)) {
            switch (this.GetCellCornerPosition(x, y, cellCount)) {
                case CellCornerPosition.TopLeft:
                    // Can only check cell to the right and cell below
                    return new NeighborStates {
                        Right = this._cellStates[1, 0],
                        Bottom = this._cellStates[0, 1],
                        BottomRight = this._cellStates[1, 1],
                    };
                case CellCornerPosition.TopRight:
                    // Can only check cell to the left and cell below
                    return new NeighborStates {
                        Left = this._cellStates[x - 1, 0], 
                        Bottom = this._cellStates[x, 1],
                        BottomLeft = this._cellStates[x - 1, y + 1],
                    };
                case CellCornerPosition.BottomLeft:
                    // Can only check cell to the right and cell to the top
                    return new NeighborStates {
                        Right = this._cellStates[1, y], 
                        Top = this._cellStates[0, y - 1],
                        TopRight = this._cellStates[x + 1, y - 1],
                    };
                case CellCornerPosition.BottomRight:
                    // Can only check cell to the left and cell to the top
                    return new NeighborStates {
                        Left = this._cellStates[x - 1, y], 
                        Top = this._cellStates[x, y - 1],
                        TopLeft = this._cellStates[x - 1, y - 1],
                    };    
                default:
                    throw new NotImplementedException("Case not implemented");
            }
        }
        
        CellSide? cellSide = this.GetCellSide(x, y, cellCount);
        // Side, but not a corner
        if (cellSide != null) {
            switch (cellSide) {
                case CellSide.Top:
                    return new NeighborStates {
                        Left = this._cellStates[x - 1, y],
                        Right = this._cellStates[x + 1, y],
                        Bottom = this._cellStates[x, y + 1],
                        BottomLeft = this._cellStates[x - 1, y + 1],
                        BottomRight = this._cellStates[x + 1, y + 1],
                    };
                case CellSide.Left:
                    NeighborStates states =  new NeighborStates {
                        Top = this._cellStates[x, y + 1],
                        Bottom = this._cellStates[x, y - 1],
                        Right = this._cellStates[x + 1, y],
                        TopRight = this._cellStates[x + 1, y - 1],
                        BottomRight = this._cellStates[x + 1, y + 1],
                    };
                    return states;
                case CellSide.Right:
                    return new NeighborStates {
                        Top = this._cellStates[x, y + 1],
                        Bottom = this._cellStates[x, y - 1],
                        Left = this._cellStates[x - 1, y],
                        TopLeft = this._cellStates[x - 1, y + 1],
                        BottomLeft = this._cellStates[x - 1, y - 1],
                    };
                case CellSide.Bottom:
                    return new NeighborStates {
                        Top = this._cellStates[x, y - 1],
                        Left = this._cellStates[x - 1, y],
                        Right = this._cellStates[x + 1, y],
                        TopLeft = this._cellStates[x - 1, y - 1],
                        TopRight = this._cellStates[x + 1, y - 1],
                    };
                default:
                    throw new NotImplementedException("Case not implemented");
            }
        }
        
        // Not a side nor a corner
        return new NeighborStates {
            TopLeft = this._cellStates[x - 1, y - 1],
            Top = this._cellStates[x, y + 1],
            TopRight = this._cellStates[x + 1, y - 1],
            Left = this._cellStates[x - 1, y],
            Right = this._cellStates[x + 1, y],
            BottomLeft = this._cellStates[x - 1, y + 1],
            Bottom = this._cellStates[x, y - 1],
            BottomRight = this._cellStates[x + 1, y + 1],
        };
    }

    private enum CellSide {
        Top,
        Left,
        Right,
        Bottom,
    }

    private CellState GetPlayingPlayerCellState(PlayingPlayer playingPlayer) {
        switch (playingPlayer) {
            case PlayingPlayer.Player1:
                return CellState.Player1;
            case PlayingPlayer.Player2:
                return CellState.Player2;
            default:
                throw new NotImplementedException("Case not implemented");
        }
    }

    private CellState GetOtherPlayerCellState(PlayingPlayer playingPlayer) {
        switch (playingPlayer) {
            case PlayingPlayer.Player1:
                return CellState.Player2;
            case PlayingPlayer.Player2:
                return CellState.Player1;
            default:
                throw new NotImplementedException("Case not implemented");
        }
    }

    private void FlipStones(int x, int y, int boardSize, PlayingPlayer playingPlayer) {
        // Diagonal top left
        this.ProcessCellDirection(
            playingPlayer, 
            (x, y),
            (tx, ty) => tx > 0 && ty > 0,
            (tx, ty) => (--tx, --ty)
        );
        
        // Top
        this.ProcessCellDirection(
            playingPlayer,
            (x, y),
            (_, ty) => ty > 0,
            (tx, ty) => (tx, --ty)
        );
        
        // Diagonal top right
        this.ProcessCellDirection(
            playingPlayer,
            (x, y),
            (tx, ty) => tx < boardSize - 1 && ty > 0,
            (tx, ty) => (++tx, --ty)
        );
        
        // Right
        this.ProcessCellDirection(
            playingPlayer,
            (x, y),
            (tx, _) => tx < boardSize - 1,
            (tx, ty) => (++tx, ty) 
        );
        
        // Diagonal bottom right
        this.ProcessCellDirection(
            playingPlayer,
            (x, y),
            (tx, ty) => tx < boardSize - 1 && ty < boardSize - 1,
            (tx, ty) => (++tx, ++ty)
        );
        
        // Bottom
        this.ProcessCellDirection(
            playingPlayer,
            (x, y),
            (_, ty) => ty < boardSize - 1,
            (tx, ty) => (tx, ++ty)
        );
        
        // Diagonal bottom left
        this.ProcessCellDirection(
            playingPlayer,
            (x, y),
            (tx, ty) => tx > 0 && ty < boardSize - 1,
            (tx, ty) => (--tx, ++ty) // F
        );
        
        // Left
        this.ProcessCellDirection(
            playingPlayer,
            (x, y),
            (tx, _) => tx > 0,
            (tx, ty) => (--tx, ty)
        );
    }

    private void ProcessCellDirection(PlayingPlayer playingPlayer, (int x, int y) cellPos, Func<int, int, bool> condition, Func<int, int, (int x, int y)> apply) {
        CellState otherPlayerState = this.GetOtherPlayerCellState(playingPlayer);
        
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

    private CellState GetCellState(int x, int y) {
        return this._cellStates[x, y];
    }

    private void FlipStone(int x, int y) {
        CellState currentCellState = this.GetCellState(x, y);
        CellState newCellState;
        switch (currentCellState) {
            case CellState.None:
                throw new InvalidDataException("Cell state may nont be None now");
            case CellState.Player1:
                newCellState = CellState.Player2;
                break;
            case CellState.Player2:
                newCellState = CellState.Player1;
                break;
            default:
                throw new NotImplementedException("Case not implemented");
        }
        
        this.SetCellState(x, y, newCellState);
    }

    private void SetCellState(int x, int y, CellState state) {
        this._cellStates[x, y] = state;
    }

    private CellSide? GetCellSide(int x, int y, int cellCount) {
        if (x > 0 && x < cellCount - 1 && y == 0) {
            return CellSide.Top;
        }

        if (x == 0 && y > 0 && y < cellCount - 1) {
            return CellSide.Left;
        }

        if (x == cellCount - 1 && y > 0 && y < cellCount - 1) {
            return CellSide.Right;
        }

        if (x > 0 && x < cellCount - 1 && y == cellCount - 1) {
            return CellSide.Bottom;
        }

        return null;
    }
    
    private bool IsCellCorner(int x, int y, int cellCount) {
        return (x == 0 || x == cellCount - 1) && (y == 0 || y == cellCount - 1);
    }

    private enum CellCornerPosition {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }

    private CellCornerPosition GetCellCornerPosition(int x, int y, int cellCount) {
        if (x == 0 && y == 0) {
            return CellCornerPosition.TopLeft;
        } 
        
        if (x == cellCount - 1 && y == 0) {
            return CellCornerPosition.TopRight;
        }

        if (x == 0 && y == cellCount - 1) {
            return CellCornerPosition.BottomLeft;
        }

        if (x == cellCount - 1 && y == cellCount - 1) {
            return CellCornerPosition.BottomRight;
        }

        throw new InvalidDataException($"Cell {x}:{y} is not a corner cell");
    }
}