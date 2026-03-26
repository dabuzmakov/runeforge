namespace runeforge;

public class GameField : Control
{
    private GameModel _model;

    public GameField(GameModel model)
    {
        _model = model;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;

        var cell = 80;
        var gridSize = 4;
        var tableSize = cell * gridSize;

        var centerX = 500;
        var centerY = 600;

        var offsetX = centerX - tableSize / 2;
        var offsetY = centerY - tableSize / 2;

        // стол
        g.FillRectangle(Brushes.DarkSlateGray, offsetX, offsetY, tableSize, tableSize);

        // сетка + руны
        for (var y = 0; y < 4; y++)
        {
            for (var x = 0; x < 4; x++)
            {
                var px = offsetX + x * cell;
                var py = offsetY + y * cell;

                g.DrawRectangle(Pens.White, px, py, cell, cell);

                var rune = _model.Table.Get(x, y);
                if (rune != null)
                {
                    var brush = rune.Type == RuneType.Uruz ? Brushes.Red : Brushes.Cyan;
                    g.FillEllipse(brush, px + 10, py + 10, cell - 20, cell - 20);
                }
            }
        }

        // путь
        var pathRect = new Rectangle(offsetX - 60, offsetY - 60, tableSize + 120, tableSize + 120);
        g.DrawRectangle(new Pen(Color.Gray, 20), pathRect);

        // враги
        foreach (var enemy in _model.Enemies)
        {
            var t = enemy.PathProgress % 1.0;
            var pos = GetPointOnPath(pathRect, t);

            g.FillRectangle(Brushes.Green, pos.X, pos.Y, 30, 30);
            g.DrawString(enemy.Hp.ToString(), Font, Brushes.White, pos);
        }

        // мешочек
        var bagRect = new Rectangle(centerX - 40, offsetY + tableSize + 20, 80, 80);
        g.FillEllipse(Brushes.Gold, bagRect);
        g.DrawString("SPAWN", Font, Brushes.Black, bagRect);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        var cell = 80;
        var tableSize = cell * 4;
        var centerX = 500;
        var centerY = 600;
        var offsetY = centerY - tableSize / 2;

        var bagRect = new Rectangle(centerX - 40, offsetY + tableSize + 20, 80, 80);

        if (bagRect.Contains(e.Location))
        {
            _model.SpawnRandomRune();
            Invalidate();
        }
    }

    private Point GetPointOnPath(Rectangle r, double t)
    {
        var p = t * 4;

        if (p < 1) return new Point((int)(r.Left + p * r.Width), r.Top);
        if (p < 2) return new Point(r.Right, (int)(r.Top + (p - 1) * r.Height));
        if (p < 3) return new Point((int)(r.Right - (p - 2) * r.Width), r.Bottom);

        return new Point(r.Left, (int)(r.Bottom - (p - 3) * r.Height));
    }
}
