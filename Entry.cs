using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net.Http;
using System.Text;
using System.IO;
using Gtk;

public sealed class Entry : ImageMenuItem
{
	public byte[] Content { private set; get; }
	public bool IsImage { private set; get; }
	public bool IsPinned { private set; get; }

	public Entry(byte[] content, bool isImage, bool isPinned) : base()
	{
		this.Content = content;
		this.IsImage = isImage;
		this.IsPinned = isPinned;

		// 󰐃󰤱
		if (!isImage) { base.Label = Encoding.UTF8.GetString(this.Content); }
		else 
		{
			base.Label = "BINARY PNG IMAGE";
			// new Thread(new ThreadStart(() => 
			// {
			// 	int start = content.IndexOf("src=\"");
			// 	string uri = content.Substring(
			// 		start + 5,
			// 		Math.Min(content.IndexOf("\"", start + 6), content.IndexOf("?", start + 6)) - start - 5
			// 	);

			// 	base.Label = this.Content;
			// 	Stream stream = new HttpClient(new HttpClientHandler()).GetStreamAsync(uri).Result;

			// 	List<byte> bytes = new List<byte>();
			// 	for (int b = stream.ReadByte(); b != -1; b = stream.ReadByte()) { bytes.Add((byte)b); }
			// 	this.Bytes = bytes.ToArray();

			// 	base.AlwaysShowImage = true;
			// 	// base.Image = new Image(stream);
			// 	Console.WriteLine(base.AllocatedWidth);
			// 	Console.WriteLine(base.AllocatedHeight);
			// })).Start();
		}

		base.Name = "entry";
		base.CanFocus = true;

		MenuItem menuPin = new MenuItem("Pin");
		menuPin.Activated += (object sender, EventArgs e) => { this.IsPinned = !this.IsPinned; menuPin.Label = (this.IsPinned) ? "Unpin" : "Pin"; Program.Pin(this.Content, this.IsImage, this.IsPinned); };
		menuPin.FocusHadjustment = new Adjustment(0, 0, 0, 0, 0, 0);
		menuPin.Name = "pin";

		MenuItem menuDelete = new MenuItem("Delete");
		menuDelete.Activated += (object sender, EventArgs e) => { Program.Delete(this.Content, this.IsImage, this.IsPinned); base.Destroy(); };
		menuDelete.Name = "delete";

		MenuItem menuClear = new MenuItem("Clear");
		menuClear.Activated += (object sender, EventArgs e) => { Program.Clear(); };
		menuClear.Name = "clear";

		Menu menu = new Menu();
		menu.Name = "menu";
		menu.AttachToWidget(this, null);
		menu.Add(menuPin);
		menu.Add(menuDelete);
		menu.Add(new SeparatorMenuItem());
		menu.Add(menuClear);
		menu.ShowAll();

		base.KeyPressEvent += (object sender, KeyPressEventArgs e) => 
		{
			if (e.Event.Key == Gdk.Key.Escape) { Application.Quit(); }
			if (e.Event.Key == Gdk.Key.Return || e.Event.Key == Gdk.Key.KP_Enter) { Program.Selected(this); Application.Quit(); }
			if (e.Event.Key == Gdk.Key.Menu) { menu.Popup(null, null, null, 3, e.Event.Time); }
		};

		base.ButtonPressEvent += (object sender, ButtonPressEventArgs e) => 
		{
			base.GrabFocus();
			if (e.Event.Button == 1) { Program.Selected(this); Application.Quit(); }
			if (e.Event.Button == 3) { menu.Popup(null, null, null, 3, e.Event.Time); }
		};
	}
}