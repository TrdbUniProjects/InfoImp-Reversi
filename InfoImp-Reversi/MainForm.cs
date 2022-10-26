using Eto.Drawing;
using Eto.Forms;
using InfoImp_Reversi.Dialogs;
using InfoImp_Reversi.GraphicsComponents;

namespace InfoImp_Reversi;

public class MainForm : Form {
    
    public MainForm() {
        base.Size = new Size(800, 600);
        this.Content = new WindowContent();
        this.Title = "InfoImp Reversi";
        
        base.Menu = new MenuBar {
            AboutItem = new ButtonMenuItem {
                Text = "Game rules",
                Command = new Command((_, _) => {
                    new HelpDialog().ShowModal(this);
                })
            },
        };
    }
}