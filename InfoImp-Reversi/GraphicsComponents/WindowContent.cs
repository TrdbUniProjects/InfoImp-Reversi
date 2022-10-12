using Eto.Drawing;
using Eto.Forms;

namespace InfoImp_Reversi.GraphicsComponents; 

public class WindowContent : Drawable {

    private readonly Label _playerAtSet;
    private readonly PlayingGrid _grid;

    public WindowContent() {
        this._playerAtSet = new Label();
        WindowContent self = this;
        this._grid = new PlayingGrid(6, ref self) {
            Size = new Size(400, 400)
        };
        
        this.Paint += this.OnPaintEvent;
        
        this.Content = new StackLayout {
            Items = {
                this._playerAtSet,
                this._grid,
            }
        };
    }
    
    
    private void OnPaintEvent(object? sender, PaintEventArgs args) {
        switch (GameManager.PlayerAtSet) {
            case PlayingPlayer.Player1:
                this._playerAtSet.Text = "Player 1";
                this._playerAtSet.TextColor = GameManager.PlayerAColor;
                break;
            case PlayingPlayer.Player2:
                this._playerAtSet.Text = "Player 2";
                this._playerAtSet.TextColor = GameManager.PlayerBColor;
                break;
            default:
                throw new NotImplementedException("Case not implemented");
        }
        this._playerAtSet.Invalidate();
    }
    
}