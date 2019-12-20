// csc /nologo /t:winexe /win32icon:50px.ico /o 50px.cs
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
class FiftyPixel : Form
{
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SetClipboardViewer(IntPtr hWnd);
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);
    IntPtr ClipboardViewerNext;

    private void RegisterClipboardViewer() { ClipboardViewerNext = SetClipboardViewer(Handle); }
    private void UnregisterClipboardViewer() { ChangeClipboardChain(Handle, ClipboardViewerNext); }

    [STAThread]
    static void Main() { Application.Run(new FiftyPixel()); }
    ContextMenuStrip menu = new ContextMenuStrip();
    NotifyIcon icon;
    public FiftyPixel()
    {
        WindowState = FormWindowState.Minimized;
        ShowInTaskbar = false;
        RegisterClipboardViewer();
        menu.Items.Add("Listen Clipboard", null, ListenClipboard_Click);
        ((ToolStripMenuItem)menu.Items[0]).Checked = true;
        RegisterClipboardViewer();
        menu.Items.Add("Show Balloon Tip", null, (s, e) => ((ToolStripMenuItem)menu.Items[1]).Checked = showBalloonTip ^= true);
        ((ToolStripMenuItem)menu.Items[1]).Checked = true;
        menu.Items.Add("Exit", null, (s, e) => Close());
        icon = new NotifyIcon
        {
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
            Text = "50px: click = convert now, right click for more options",
            Visible = true,
            ContextMenuStrip = menu
        };
        icon.Click += (s, e) => doConvert();
        FormClosed += (s, e) =>
        {
            icon.Visible = false;
            icon.Dispose();
            if (IsListening)
                UnregisterClipboardViewer();
        };
    }

    bool IsListening = true;
    public void ListenClipboard_Click(object sender, EventArgs e)
    {
        bool isListening = ((ToolStripMenuItem)menu.Items[0]).Checked ^= true;
        if (isListening != IsListening)
        {
            if (IsListening = isListening)
                RegisterClipboardViewer();
            else
                UnregisterClipboardViewer();
        }
    }

    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case /* WM_DRAWCLIPBOARD */ 0x308:
                {
                    doConvert();
                    break;
                }
            default:
                {
                    base.WndProc(ref m);
                    break;
                }
        }
    }

    private void doConvert()
    {
        string s = Clipboard.GetText(TextDataFormat.Html);
        string p = "<img src=\"file:///(.+)\"";
        string x = null;
        foreach (Match match in Regex.Matches(s, p))
            if (match.Success && (String.IsNullOrEmpty(x) || !x.EndsWith(".gif")))
                x = match.Groups[1].Value;
        if (!String.IsNullOrEmpty(x))
            handle(x);
    }

    bool showBalloonTip = true;
    private void handle(string file)
    {
        if (file.EndsWith(".gif"))
        {
            string fileName = Path.GetTempPath() + Path.GetRandomFileName() + ".gif";
            run("gifsicle", "--resize-fit-width 50 -i \"" + file + "\" -o " + fileName);
            byte[] data = Encoding.UTF8.GetBytes("<QQRichEditFormat><Info version=\"1001\"></Info><EditElement type=\"1\" filepath=\"" + fileName + "\" shortcut=\"\"></EditElement></QQRichEditFormat>");
            var obj = new DataObject();
            obj.SetData("QQ_Unicode_RichEdit_Format", new System.IO.MemoryStream(data));
            obj.SetData("QQ_RichEdit_Format", new System.IO.MemoryStream(data));
            obj.SetData("FileDrop", new string[] { fileName });
            obj.SetData("FileNameW", new string[] { fileName });
            obj.SetData("FileName", new string[] { fileName });
            Clipboard.SetDataObject(obj, true);
        }
        else
            run("magick", "\"" + file + "\" -resize 50x-1 clipboard:");
        if (showBalloonTip && icon != null)
        {
            icon.BalloonTipText = file + " ok";
            icon.ShowBalloonTip(500);
            Thread t = new Thread(clearNotifications) { IsBackground = true };
            t.Start();
        }
    }

    private void run(string exe, string arg)
    {
        ProcessStartInfo info = new ProcessStartInfo(exe, arg) { WindowStyle = ProcessWindowStyle.Hidden };
        Process process = new Process { StartInfo = info };
        process.Start();
        process.WaitForExit();
    }

    private void clearNotifications() {
        Thread.Sleep(1000);
        icon.Visible = false;
        icon.Visible = true;
    }
}