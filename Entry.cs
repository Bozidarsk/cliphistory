using System;
using System.Text;
using System.Reflection;
using System.Linq;
using Gtk;

public sealed class Entry 
{
	public byte[] Content { private set; get; }
	public bool IsImage { private set; get; }
	public bool IsPinned { private set; get; }
	public EventBox Widget { private set; get; }

	// 󰐃󰤱
	public Entry(byte[] content, bool isImage, bool isPinned, string filename) 
	{
		this.Content = content;
		this.IsImage = isImage;
		this.IsPinned = isPinned;
		this.Widget = new EventBox();

		this.Widget.Name = "entry";
		this.Widget.CanFocus = true;
		this.Widget.HeightRequest = 48;
		// this.Widget.Halign = Align.Start;
		// this.Widget.Valign = Align.Center;
		// this.Widget.Hexpand = true;
		// this.Widget.Vexpand = true;
		// this.Widget.HexpandSet = true;
		// this.Widget.VexpandSet = true;

		if (!isImage) 
		{
			Label label = new Label(Encoding.UTF8.GetString(content));
			label.Ellipsize = Pango.EllipsizeMode.End;
			label.Wrap = false;
			label.Name = "text";

			this.Widget.Add(label);
		}
		else 
		{
			// Gdk.Pixbuf buffer = new Gdk.Pixbuf(Path.TmpDir + filename);
			// buffer.ScaleSimple(
			// 	64,
			// 	64,
			// 	Gdk.InterpType.Bilinear
			// );

			// Image image = new Image(buffer);

			Image image = new Image(Path.TmpDir + filename);
			image.Name = "image";

			this.Widget.Add(image);
		}

		MenuItem menuPin = new MenuItem("Pin");
		menuPin.Activated += (object sender, EventArgs e) => { this.IsPinned = !this.IsPinned; menuPin.Label = (this.IsPinned) ? "Unpin" : "Pin"; Program.Pin(this); };
		menuPin.Name = "pin";

		MenuItem menuDelete = new MenuItem("Delete");
		menuDelete.Activated += (object sender, EventArgs e) => { Program.Delete(this); this.Widget.Destroy(); };
		menuDelete.Name = "delete";

		MenuItem menuClear = new MenuItem("Clear");
		menuClear.Activated += (object sender, EventArgs e) => { Program.Clear(); };
		menuClear.Name = "clear";

		Menu menu = new Menu();
		menu.Name = "menu";
		menu.AttachToWidget(this.Widget, null);
		menu.Add(menuPin);
		menu.Add(menuDelete);
		menu.Add(new SeparatorMenuItem());
		menu.Add(menuClear);
		menu.ShowAll();

		this.Widget.KeyPressEvent += (object sender, KeyPressEventArgs e) => 
		{
			if (e.Event.Key == Gdk.Key.Escape) { Application.Quit(); }
			if (e.Event.Key == Gdk.Key.Return || e.Event.Key == Gdk.Key.KP_Enter) { Program.Selected(this); Application.Quit(); }
			if (e.Event.Key == Gdk.Key.Menu) { menu.Popup(null, null, null, 3, e.Event.Time); }
		};

		this.Widget.ButtonPressEvent += (object sender, ButtonPressEventArgs e) => 
		{
			this.Widget.GrabFocus();
			if (e.Event.Button == 1) { Program.Selected(this); Application.Quit(); }
			if (e.Event.Button == 3) { menu.Popup(null, null, null, 3, e.Event.Time); }
		};
	}
}