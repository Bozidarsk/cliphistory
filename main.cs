using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;

public static class Program 
{
	public static void Selected(Entry entry) 
	{
		if (entry.IsImage) 
		{
			File.WriteAllBytes(Path.Selected, entry.Content);
			Process.Start(
				Environment.GetEnvironmentVariable("SHELL"),
				"-c \"cat " + Path.Selected + " | wl-copy\""
			);
		}
		else { Process.Start("wl-copy", Encoding.UTF8.GetString(entry.Content)); }

	}

	public static void Store(byte[] content, bool isImage) 
	{
		string line = ConstructLine(content, isImage, false, out string filename);

		if (!File.Exists(Path.Tmp)) 
		{
			File.WriteAllText(Path.Tmp, line);
			File.WriteAllBytes(Path.TmpDir + filename, content);
			return;
		}

		if (File.Exists(Path.TmpDir + filename)) { return; }
		File.WriteAllText(Path.Tmp, File.ReadAllText(Path.Tmp) + "\n" + line);
		File.WriteAllBytes(Path.TmpDir + filename, content);
	}

	public static void Pin(Entry entry) { Pin(entry.Content, entry.IsImage, entry.IsPinned); }
	public static void Pin(byte[] content, bool isImage, bool newIsPinned) 
	{
		List<string> lines = File.ReadAllText(Path.Tmp).Split('\n').ToList();
		List<string> pinnedLines = new List<string>();
		string dump, file = "";

		Directory.GetFiles(Path.PinnedDir, "*" + Path.EntryExtension).ToList().ForEach(x => File.Delete(x));

		for (int i = 0; i < lines.Count; i++) 
		{
			if (lines[i].GetHashCode() == ConstructLine(content, isImage, !newIsPinned, out dump).GetHashCode()) 
			{ lines[i] = ConstructLine(content, isImage, newIsPinned, out dump); } 

			DeconstructLine(lines[i], out byte[] _content, out bool _isImage, out bool _isPinned, out string _filename);
			if (_isPinned) 
			{
				pinnedLines.Add(lines[i]);
				File.WriteAllBytes(Path.PinnedDir + _filename, _content);
			}

			file += "\n" + lines[i];
		}

		File.WriteAllText(Path.Tmp, file.Remove(0, 1));

		file = "";
		for (int i = 0; i < pinnedLines.Count; i++) { file += "\n" + pinnedLines[i]; }
		if (file != "") { File.WriteAllText(Path.Pinned, file.Remove(0, 1)); }
		if (file == "" && File.Exists(Path.Pinned)) { File.Delete(Path.Pinned); }
	}

	public static void Delete(Entry entry) { Delete(entry.Content, entry.IsImage, entry.IsPinned); }
	public static void Delete(byte[] content, bool isImage, bool isPinned) 
	{
		DeleteLine(Path.Tmp, Path.TmpDir, content, isImage, isPinned);
		if (isPinned) { DeleteLine(Path.Pinned, Path.PinnedDir, content, isImage, isPinned); }
	}

	private static void DeleteLine(string path, string pathDir, byte[] content, bool isImage, bool isPinned) 
	{
		if (!File.Exists(path)) { return; }

		List<string> lines = File.ReadAllText(path).Split('\n').ToList();
		string file = "";

		for (int i = 0; i < lines.Count; i++) 
		{
			if (lines[i].GetHashCode() == ConstructLine(content, isImage, isPinned, out string filename).GetHashCode()) 
			{
				File.Delete(pathDir + filename);
				continue;
			}

			file += "\n" + lines[i];
		}

		if (file != "") { File.WriteAllText(path, file.Remove(0, 1)); }
		else { File.Delete(path); }
	}

	public static void Clear() 
	{
		Directory.GetFiles(Path.TmpDir, "*" + Path.EntryExtension).ToList().ForEach(x => File.Delete(x));
		if (File.Exists(Path.Pinned)) 
		{
			Directory.GetFiles(Path.PinnedDir, "*" + Path.EntryExtension).ToList().ForEach(x => 
				{
					File.WriteAllBytes(x.Remove(0, x.LastIndexOf("/") + 1), File.ReadAllBytes(x));
				}
			);
			File.WriteAllText(Path.Tmp, File.ReadAllText(Path.Pinned));
		} else { File.Delete(Path.Tmp); }
	}

	private static string ConstructLine(byte[] content, bool isImage, bool isPinned, out string filename) 
	{
		string file = Encoding.UTF8.GetString(content).GetHashCode().ToString() + Path.EntryExtension;
		string line = ((isImage) ? 1 : 0).ToString() + ((isPinned) ? 1 : 0).ToString() + " " + file;
		filename = file;
		return line;
	}

	private static void DeconstructLine(string line, out byte[] content, out bool isImage, out bool isPinned, out string filename) 
	{
		string file = line.Remove(0, 3);
		isImage = line[0] == '1';
		isPinned = line[1] == '1';
		content = File.ReadAllBytes(Path.TmpDir + file);
		filename = file;
	}

	private static int Main(string[] args) 
	{
		if (!Directory.Exists(Path.TmpDir)) { Directory.CreateDirectory(Path.TmpDir); }
		if (!Directory.Exists(Path.PinnedDir)) { Directory.CreateDirectory(Path.PinnedDir); }
		if (!File.Exists(Path.Tmp) && File.Exists(Path.Pinned)) { File.WriteAllText(Path.Tmp, File.ReadAllText(Path.Pinned)); }

		Stream stream;
		List<byte> bytes;

		if (args.Length == 0) { args = new string[] { "help" }; }
		switch (args[0]) 
		{
			case "-h":
			case "--help":
			case "help":
				Console.WriteLine("Usage:\n\tcliphistory <command> [arguments...]");
				Console.WriteLine("\nCommands:");
				Console.WriteLine("\twindow                       Opens a gtk3 window to select an entry from history.");
				Console.WriteLine("\tstore <type>                 Adds a new entry to history. Value is (text|image).");
				Console.WriteLine("\tpin <isImage> <newIsPinned>  Pins (or unpins a clipboard entry). Values are (true|false).");
				Console.WriteLine("\tdelete <isImage> <IsPinned>  Deletes a clipboard entry. Values are (true|false).");
				Console.WriteLine("\tclear                        Deletes all unpined clipboard entries.");
				Console.WriteLine("\tclearall                     Deletes all (pined and unpined) clipboard entries.");
				break;
			case "window":
				Window.Run((File.Exists(Path.Tmp)) ? File.ReadAllText(Path.Tmp).Split('\n') : new string[] {}, DeconstructLine);
				break;
			case "store":
				if (args.Length != 2) { Console.WriteLine("Invalid arguments."); return 1; }

				stream = Console.OpenStandardInput();
				bytes = new List<byte>();
				for (int b = stream.ReadByte(); b != -1; b = stream.ReadByte()) { bytes.Add((byte)b); }
				Store(bytes.ToArray(), args[1] == "image");
				break;
			case "pin":
				if (args.Length != 3) { Console.WriteLine("Invalid arguments."); return 1; }

				stream = Console.OpenStandardInput();
				bytes = new List<byte>();
				for (int b = stream.ReadByte(); b != -1; b = stream.ReadByte()) { bytes.Add((byte)b); }
				Pin(bytes.ToArray(), args[1] == "true", args[2] == "true");
				break;
			case "delete":
				if (args.Length != 3) { Console.WriteLine("Invalid arguments."); return 1; }

				stream = Console.OpenStandardInput();
				bytes = new List<byte>();
				for (int b = stream.ReadByte(); b != -1; b = stream.ReadByte()) { bytes.Add((byte)b); }
				Delete(bytes.ToArray(), args[1] == "true", args[2] == "true");
				break;
			case "clear":
				Clear();
				break;
			case "clearall":
				Directory.GetFiles(Path.TmpDir, "*").ToList().ForEach(x => File.Delete(x));
				Directory.GetFiles(Path.PinnedDir, "*").ToList().ForEach(x => File.Delete(x));
				break;
			default:
				Console.WriteLine("Unrecognized command.");
				return 1;
		}

		return 0;
	}
}