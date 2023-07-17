using System;
using Gtk;

public static class Window 
{
	public delegate void GetPropertiesDelegate(string line, out byte[] content, out bool isImage, out bool isPinned, out string filename);

	public static void Run(string[] entries, GetPropertiesDelegate getProperties) 
	{
		Application.Init();

		CssProvider provider = new CssProvider();
		provider.LoadFromData(Config.Css);
		StyleContext.AddProviderForScreen(Gdk.Screen.Default, provider, 800);

		VBox box = new VBox();
		box.Name = "box";

		ScrolledWindow scroll = new ScrolledWindow();
		scroll.Add(box);

		for (int i = entries.Length - 1; i >= 0; i--) 
		{
			getProperties(entries[i], out byte[] content, out bool isImage, out bool isPinned, out string filename);
			Entry entry = new Entry(content, isImage, isPinned, filename);
			box.PackStart(entry.Widget, false, false, 0);
		}

		if (entries.Length == 0) 
		{
			box.PackStart(new Label("\udb80\udd4c"), false, false, 0);
			box.PackStart(new Label("Clipboard is empty."), false, false, 0);
			box.Children[0].Name = "empty-icon";
			box.Children[1].Name = "empty-text";
		}

		Gtk.Window window = new Gtk.Window("");
		window.Resizable = false;
		window.KeepAbove = true;
		window.TypeHint = Gdk.WindowTypeHint.Menu;
		window.FocusOutEvent += (object sender, FocusOutEventArgs e) => { Application.Quit(); };
		window.SetDefaultSize(Config.WindowWidth, Config.WindowHeight);
		window.Add(scroll);
		window.ShowAll();

		Application.Run();
	}
}