using Eto.Drawing;
using Eto.Forms;

namespace InfoImp_Reversi.GraphicsComponents; 

/// <summary>
/// A ciruclar stone with a color
/// </summary>
public class ScoreStone : Drawable {

    /// <summary>
    /// The color of the stone
    /// </summary>
    private readonly Color _color;
    
    public ScoreStone(Color color) {
        this._color = color;

        this.Paint += this.OnPaintEvent;
    }

    private void OnPaintEvent(object? sender, PaintEventArgs args) {
        args.Graphics.FillEllipse(this._color, 0, 0, this.Width, this.Height);
        args.Graphics.Flush();
    }
}