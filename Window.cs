using System;
using Gtk;

public static class Window 
{
	public delegate void GetPropertiesDelegate(string line, out byte[] content, out bool isImage, out bool isPinned, out string filename);

	public static void Run(string[] entries, GetPropertiesDelegate getProperties) 
	{
		Application.Init();

		Gtk.Window window = new Gtk.Window("");
		window.SetDefaultSize(400, 450);

		CssProvider provider = new CssProvider();
		provider.LoadFromData(System.IO.File.ReadAllText(Path.Css));
		StyleContext.AddProviderForScreen(Gdk.Screen.Default, provider, 800);

		VBox box = new VBox();
		Viewport view = new Viewport();
		ScrolledWindow scroll = new ScrolledWindow();
		view.Add(box);
		scroll.Add(view);
		scroll.Name = "scroll";
		box.Name = "box";

		for (int i = entries.Length - 1; i >= 0; i--) 
		{
			getProperties(entries[i], out byte[] content, out bool isImage, out bool isPinned, out string filename);
			box.PackStart(new Entry(content, isImage, isPinned), false, false, 0);
		}

		if (entries.Length == 0) 
		{
			box.PackStart(new Label("\udb80\udd4c"), false, false, 0);
			box.PackStart(new Label("Clipboard is empty."), false, false, 0);
			box.Children[0].Name = "empty-icon";
			box.Children[1].Name = "empty-text";
		}

		window.FocusOutEvent += (object sender, FocusOutEventArgs e) => { Application.Quit(); };
		window.Resizable = false;
		window.KeepAbove = true;
		window.TypeHint = Gdk.WindowTypeHint.Menu;
		window.Add(scroll);
		window.ShowAll();

		Application.Run();
	}
}