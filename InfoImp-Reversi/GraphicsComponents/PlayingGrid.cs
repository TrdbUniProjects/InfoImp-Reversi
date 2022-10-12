using Eto.Drawing;
using Eto.Forms;

namespace InfoImp_Reversi.GraphicsComponents;

public class PlayingGrid : Drawable {

    private const int DefaultCellCount = 4;
    private readonly int _cellCount;
    private readonly bool[,] _cellStates;
    
    public PlayingGrid(int? cellCount) {
        this.Paint += this.OnPaintEvent;
        this.MouseDown += this.OnMouseDownEvent;
        
        this._cellCount = cellCount ?? DefaultCellCount;
        this._cellStates = new bool[this._cellCount, this._cellCount];
    }

    private void OnPaintEvent(object? sender, PaintEventArgs args) {
        int cellSize = this.Width / this._cellCount;
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
        for (int x = 0; x < this.Width; x += cellSize) {
            for (int y = 0; y < this.Height; y += cellSize) {
                this.DrawCell(ref graphics, x, y, cellSize);
            }
        }
    }

    private void DrawCell(ref Graphics graphics, int x, int y, int size) {
        float circleSize = size * 0.7f;
        float halfSize = size / 2f;
        
        float cx = x + halfSize;
        float cy = y + halfSize;

        Color g = this._cellStates[x / size, y / size] ? Colors.Red : Colors.Blue;
        graphics.FillEllipse(g, cx - circleSize / 2f, cy - circleSize / 2f, circleSize, circleSize);
    }

    private void OnMouseDownEvent(object? sender, MouseEventArgs args) {
        if (args.Buttons != MouseButtons.Primary) {
            return;
        }
        
        int cellSize = this.Width / this._cellCount;
        int cellX = (int) (args.Location.X / cellSize);
        int cellY = (int) (args.Location.Y / cellSize);

        bool cellState = this._cellStates[cellX, cellY];
        this._cellStates[cellX, cellY] = !cellState;
        
        // TODO cellstate
        
        this.Invalidate(); 
    }
}