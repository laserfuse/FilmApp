using System;
using System.Drawing;
using System.Windows.Forms;

namespace FilmApp;

public static class Prompt
{
    public static string ShowDialog(string text, string caption)
    {
        var prompt = new Form
        {
            Width          = 300,
            Height         = 150,
            Text           = caption,
            StartPosition  = FormStartPosition.CenterParent
        };

        var lbl = new Label
        {
            Left  = 10,
            Top   = 10,
            Text  = text,
            Width = 260
        };
        var txt = new TextBox
        {
            Left  = 10,
            Top   = 40,
            Width = 260
        };
        var ok = new Button
        {
            Text         = "OK",
            Left         = 200,
            Width        = 70,
            Top          = 70,
            DialogResult = DialogResult.OK
        };

        prompt.Controls.Add(lbl);
        prompt.Controls.Add(txt);
        prompt.Controls.Add(ok);
        prompt.AcceptButton = ok;

        return prompt.ShowDialog() == DialogResult.OK
            ? txt.Text
            : string.Empty;
    }
}