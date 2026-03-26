namespace runeforge;

public partial class GameForm : Form
{
    public GameForm()
    {
        InitializeComponent();
        ClientSize = new Size(1000, 1000);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var model = new GameModel();
        var gameField = new GameField(model) { Dock = DockStyle.Fill };
        Controls.Add(gameField);

        var timer = new System.Windows.Forms.Timer();
        timer.Interval = 15;
        timer.Tick += (s, e) =>
        {
            model.Update(0.015);
            gameField.Invalidate();
        };
        timer.Start();
    }
}
