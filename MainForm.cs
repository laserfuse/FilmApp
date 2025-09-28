using FilmApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace FilmApp;

public class MainForm : Form
{
    private readonly VerticalFlowPanel _panel;
    private readonly Button            _addBtn;
    private const string               DataFile    = "Data/films.json";
    private const int                  ThumbWidth  = 120;
    private const int                  ThumbHeight = 180;

    public MainForm()
    {
        // Titre et icône
        Text = "FilmApp";
        var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.ico");
        if (File.Exists(iconPath))
            Icon = new Icon(iconPath);

        WindowState     = FormWindowState.Maximized;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterScreen;

        _panel = new VerticalFlowPanel
        {
            Dock          = DockStyle.Fill,
            Padding       = new Padding(20),
            AutoScroll    = true,
            WrapContents  = true,
            FlowDirection = FlowDirection.LeftToRight
        };
        _panel.Paint += Panel_PaintGradient;
        Controls.Add(_panel);

        _addBtn = new Button
        {
            Text      = "+",
            Width     = ThumbWidth,
            Height    = ThumbHeight,
            Font      = new Font("Segoe UI", 24F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Cursor    = Cursors.Hand,
            Margin    = new Padding(8)
        };
        _addBtn.FlatAppearance.BorderColor = Color.LightGray;
        _addBtn.FlatAppearance.BorderSize  = 1;
        _addBtn.Click      += AddBtn_Click;
        _addBtn.MouseEnter += (s, e) => HighlightButton(_addBtn, true);
        _addBtn.MouseLeave += (s, e) => HighlightButton(_addBtn, false);
        _panel.Controls.Add(_addBtn);

        LoadFilms();
    }

    private void Panel_PaintGradient(object sender, PaintEventArgs e)
    {
        var rect = _panel.ClientRectangle;
        using var brush = new LinearGradientBrush(
            rect,
            Color.Black,
            Color.Gray,
            LinearGradientMode.Vertical
        );
        e.Graphics.FillRectangle(brush, rect);
    }

    private void LoadFilms()
    {
        if (!File.Exists(DataFile))
            return;

        var json = File.ReadAllText(DataFile);
        var list = JsonSerializer.Deserialize<List<Film>>(json) ?? new List<Film>();
        foreach (var film in list)
        {
            if (!File.Exists(film.ImagePath) || !File.Exists(film.VideoPath))
                continue;
            AddFilmControl(film);
        }
    }

    private void AddBtn_Click(object sender, EventArgs e)
    {
        using var ofdVid = new OpenFileDialog { Filter = "Vidéo (*.mp4;*.avi)|*.mp4;*.avi" };
        if (ofdVid.ShowDialog() != DialogResult.OK) return;

        using var ofdImg = new OpenFileDialog { Filter = "Image (*.jpg;*.png)|*.jpg;*.png" };
        if (ofdImg.ShowDialog() != DialogResult.OK) return;

        var title = Prompt.ShowDialog("Titre du film", "Ajouter un film");
        if (string.IsNullOrWhiteSpace(title)) return;

        Directory.CreateDirectory("Media");
        var destVid = Path.Combine("Media", Path.GetFileName(ofdVid.FileName));
        var destImg = Path.Combine("Media", Path.GetFileName(ofdImg.FileName));
        File.Copy(ofdVid.FileName, destVid, true);
        File.Copy(ofdImg.FileName, destImg, true);

        var film = new Film
        {
            Title     = title,
            VideoPath = destVid,
            ImagePath = destImg
        };

        SaveFilm(film);
        AddFilmControl(film);
    }

    private void SaveFilm(Film film)
    {
        var json = File.Exists(DataFile) ? File.ReadAllText(DataFile) : "[]";
        var list = JsonSerializer.Deserialize<List<Film>>(json) ?? new List<Film>();
        list.Add(film);
        Directory.CreateDirectory("Data");
        File.WriteAllText(
            DataFile,
            JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true })
        );
    }

    private void AddFilmControl(Film film)
    {
        var img = LoadSafeImage(film.ImagePath);
        var btnFilm = new FilmButton
        {
            FilmImage = img,
            Width     = ThumbWidth,
            Height    = ThumbHeight,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Cursor    = Cursors.Hand,
            Margin    = new Padding(8)
        };
        btnFilm.FlatAppearance.BorderColor = Color.LightGray;
        btnFilm.FlatAppearance.BorderSize  = 1;
        btnFilm.MouseEnter += (s, e) => HighlightButton(btnFilm, true);
        btnFilm.MouseLeave += (s, e) => HighlightButton(btnFilm, false);
        btnFilm.Click      += (s, e) => PlayVideo(film.VideoPath);

        var btnDelete = new Button
        {
            Text      = "✕",
            Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Red,
            Width     = 20,
            Height    = 20,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        btnDelete.FlatAppearance.BorderSize = 0;
        var path = new GraphicsPath();
        path.AddEllipse(0, 0, btnDelete.Width, btnDelete.Height);
        btnDelete.Region = new Region(path);
        btnDelete.Top    = 5;
        btnDelete.Left   = btnFilm.Width - btnDelete.Width - 5;
        btnDelete.Click  += (s, e) =>
        {
            if (MessageBox.Show(
                    $"Supprimer « {film.Title} » ?",
                    "Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                ) != DialogResult.Yes)
                return;

            btnFilm.FilmImage.Dispose();
            btnFilm.Dispose();
            DeleteFilm(film, btnFilm);
        };

        btnFilm.Controls.Add(btnDelete);
        _panel.Controls.Add(btnFilm);
    }

    private Image LoadSafeImage(string path)
    {
        try
        {
            var bytes = File.ReadAllBytes(path);
            using var ms = new MemoryStream(bytes);
            return new Bitmap(Image.FromStream(ms));
        }
        catch
        {
            var bmp = new Bitmap(ThumbWidth, ThumbHeight);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.LightGray);
            using var pen = new Pen(Color.DarkGray) { DashStyle = DashStyle.Dash };
            g.DrawRectangle(pen, 0, 0, bmp.Width - 1, bmp.Height - 1);
            g.DrawLine(pen, 0, 0, bmp.Width, bmp.Height);
            g.DrawLine(pen, bmp.Width, 0, 0, bmp.Height);
            return bmp;
        }
    }

    private void HighlightButton(Button btn, bool hover)
    {
        btn.BackColor                  = hover ? Color.LightCyan : Color.White;
        btn.FlatAppearance.BorderColor = hover ? Color.Gray      : Color.LightGray;
        btn.FlatAppearance.BorderSize  = hover ? 2             : 1;
    }

    private void DeleteFilm(Film film, Control control)
    {
        try
        {
            if (File.Exists(film.VideoPath)) File.Delete(film.VideoPath);
            if (File.Exists(film.ImagePath)) File.Delete(film.ImagePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur suppression :\n{ex.Message}");
            return;
        }

        var json = File.Exists(DataFile) ? File.ReadAllText(DataFile) : "[]";
        var list = JsonSerializer.Deserialize<List<Film>>(json) ?? new List<Film>();
        list.RemoveAll(f =>
            f.VideoPath == film.VideoPath &&
            f.ImagePath == film.ImagePath
        );
        File.WriteAllText(
            DataFile,
            JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true })
        );

        _panel.Controls.Remove(control);
        control.Dispose();
    }

    private void PlayVideo(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName        = path,
            UseShellExecute = true
        });
    }
}

internal class VerticalFlowPanel : FlowLayoutPanel
{
    private const int WS_VSCROLL = 0x00200000;
    private const int WS_HSCROLL = 0x00100000;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.Style |= WS_VSCROLL;
            cp.Style &= ~WS_HSCROLL;
            return cp;
        }
    }
}

internal class FilmButton : Button
{
    public Image FilmImage { get; set; }

    public FilmButton()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true
        );
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g    = e.Graphics;
        var dest = ClientRectangle;

        using var bg = new SolidBrush(BackColor);
        g.FillRectangle(bg, dest);

        if (FilmImage != null)
        {
            int iw = FilmImage.Width, ih = FilmImage.Height;
            float ir = (float)iw / ih;
            float dr = (float)dest.Width / dest.Height;
            Rectangle src;

            if (ir > dr)
            {
                int srcW = (int)(ih * dr);
                int srcX = (iw - srcW) / 2;
                src = new Rectangle(srcX, 0, srcW, ih);
            }
            else
            {
                int srcH = (int)(iw / dr);
                int srcY = (ih - srcH) / 2;
                src = new Rectangle(0, srcY, iw, srcH);
            }

            g.DrawImage(FilmImage, dest, src, GraphicsUnit.Pixel);
        }

        int bsize = FlatAppearance.BorderSize;
        if (bsize > 0)
        {
            using var pen = new Pen(FlatAppearance.BorderColor, bsize);
            g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}