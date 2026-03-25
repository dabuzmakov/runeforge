namespace runeforge;

public class GameField : Control
{

    private GameModel _model;

    public GameField(GameModel model)
    {
        _model = model;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;

        var cellSize = 80;
        var offsetX = 200;
        var offsetY = 100;

        // сетка
        for (var y = 0; y < _model.Table.Size; y++)
        {
            for (var x = 0; x < _model.Table.Size; x++)
            {
                var px = offsetX + x * cellSize;
                var py = offsetY + y * cellSize;

                g.DrawRectangle(Pens.White, px, py, cellSize, cellSize);

                var rune = _model.Table.Get(x, y);
                if (rune != null)
                {
                    Brush brush = rune.Type == RuneType.Uruz ? Brushes.Red : Brushes.Cyan;
                    g.FillRectangle(brush, px + 5, py + 5, cellSize - 10, cellSize - 10);
                }
            }
        }

        // мешок
        g.FillEllipse(Brushes.Gold, 350, 450, 80, 80);
        g.DrawString("Мешок", Font, Brushes.Black, 350, 480);

        // враги
        var enemyX = 100;
        var enemyY = 100;

        foreach (var enemy in _model.Enemies)
        {
            g.FillRectangle(Brushes.Green, enemyX, enemyY, 40, 40);
            g.DrawString(enemy.Hp.ToString(), Font, Brushes.White, enemyX, enemyY);

            enemyX += 60;
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        var rect = new Rectangle(350, 450, 80, 80);

        if (rect.Contains(e.Location))
        {
            _model.SpawnRandomRune();
            Invalidate();
        }
    }
}