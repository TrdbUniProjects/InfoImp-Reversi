using Eto.Drawing;
using Eto.Forms;

namespace InfoImp_Reversi.Dialogs;

public class HelpDialog : Dialog {

    public HelpDialog() {
        base.Size = new Size(500, 400);
        this.Content =
            new Label {
                Text =
                    "De spelregels van Reversi zijn simpel: Je mag een steen neerzetten als 1 van de buren van de\n" +
                    "positie waar je je steen wilt neerzetten een steen van de andere speler is. Ook moet de positie\n" +
                    "waar je de steen neer wilt zetten leeg zijn.\n" +
                    "\n" +
                    "Als een steen neergezet is, worden alle ingesloten stenen van de tegenpartij ingesloten. Als er\n" +
                    "geen mogelijkheden zijn, of het speelbord is vol, dan wint de speler met de meeste stenen."
            };
    }
}