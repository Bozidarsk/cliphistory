using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.IO;
using Gtk;

public sealed class Entry 
{
	public byte[] Content { private set; get; }
	public bool IsImage { private set; get; }
	public bool IsPinned { private set; get; }
	public Widget Widget { private set; get; }

	// 󰐃󰤱
	public Entry(byte[] content, bool isImage, bool isPinned, string filename) 
	{
		this.Content = content;
		this.IsImage = isImage;
		this.IsPinned = isPinned;

		if (!isImage) 
		{
			this.Widget = new EventBox();
			((EventBox)this.Widget).Add(new Label(Encoding.UTF8.GetString(content)));
			// this.Widget = new ImageMenuItem();
			// ((ImageMenuItem)this.Widget).Label = Encoding.UTF8.GetString(content);
		}
		else 
		{
			this.Widget = new EventBox();
			((EventBox)this.Widget).Add(new Image(Path.TmpDir + filename));
			// this.Widget = new Image(Path.TmpDir + filename);
		}

		this.Widget.Name = "entry";
		this.Widget.CanFocus = true;
		this.Widget.HeightRequest = 64;
		// this.Widget.Halign = Align.Start;
		// this.Widget.Valign = Align.Center;
		// this.Widget.Hexpand = true;
		// this.Widget.HexpandSet = true;

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