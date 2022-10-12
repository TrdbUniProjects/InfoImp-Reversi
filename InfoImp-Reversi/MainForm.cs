using Eto.Drawing;
using Eto.Forms;
using InfoImp_Reversi.GraphicsComponents;

namespace InfoImp_Reversi;

public class MainForm : Form {

    private PlayingGrid _grid;
    
    public MainForm() {
        this._grid = new PlayingGrid(4) {
            Size = new Size(400, 400)
        };
        
        this.Content = this._grid;
    }
}