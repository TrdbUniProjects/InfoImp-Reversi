using Eto.Drawing;
using Eto.Forms;

namespace InfoImp_Reversi.GraphicsComponents; 

public class ScoreStone : Drawable {

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