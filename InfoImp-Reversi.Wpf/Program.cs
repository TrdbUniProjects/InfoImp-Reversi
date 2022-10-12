using Eto.Forms;

namespace InfoImp_Reversi.Wpf; 

public class Program {
    
    [STAThread]
    public static void Main(String[] args) {
        new Application().Run(new MainForm());
    }
}