using Eto.Drawing;
using Eto.Forms;

namespace InfoImp_Reversi.Dialogs; 

public class HelpDialog : Dialog {

    public HelpDialog(Control parent) {
        base.Size = new Size(500, 400);
        this.Content = new Label {
            Text = "Foo"
        };
    }
}