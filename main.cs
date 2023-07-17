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
			File.WriteAllBytes("/tmp/cliphistoryselectedimage", entry.Content);
			Process.Start(
				Environment.GetEnvironmentVariable("SHELL"),
				"-c \"cat /tmp/cliphistoryselectedimage | wl-copy\""
			);
		}
		else { Process.Start("wl-copy", Encoding.UTF8.GetString(entry.Content)); }

	}

	public static void Store(byte[] content, bool isImage) 
	{
		string line = ConstructLine(content, isImage, false, out string filename);

		if (!File.Exists(Config.PathTmpDir + "/entries")) 
		{
			File.WriteAllText(Config.PathTmpDir + "/entries", line);
			File.WriteAllBytes(Config.PathTmpDir + filename, content);
			return;
		}

		if (File.Exists(Config.PathTmpDir + filename)) { return; }
		File.WriteAllText(Config.PathTmpDir + "/entries", File.ReadAllText(Config.PathTmpDir + "/entries") + "\n" + line);
		File.WriteAllBytes(Config.PathTmpDir + filename, content);
	}

	public static void Pin(Entry entry) { Pin(entry.Content, entry.IsImage, entry.IsPinned); }
	public static void Pin(byte[] content, bool isImage, bool newIsPinned) 
	{
		List<string> lines = File.ReadAllText(Config.PathTmpDir + "/entries").Split('\n').ToList();
		List<string> pinnedLines = new List<string>();
		string dump, file = "";

		Directory.GetFiles(Config.PathPinnedDir, "*.entry").ToList().ForEach(x => File.Delete(x));

		for (int i = 0; i < lines.Count; i++) 
		{
			if (lines[i].GetHashCode() == ConstructLine(content, isImage, !newIsPinned, out dump).GetHashCode()) 
			{ lines[i] = ConstructLine(content, isImage, newIsPinned, out dump); } 

			DeconstructLine(lines[i], out byte[] _content, out bool _isImage, out bool _isPinned, out string _filename);
			if (_isPinned) 
			{
				pinnedLines.Add(lines[i]);
				File.WriteAllBytes(Config.PathPinnedDir + _filename, _content);
			}

			file += "\n" + lines[i];
		}

		File.WriteAllText(Config.PathTmpDir + "/entries", file.Remove(0, 1));

		file = "";
		for (int i = 0; i < pinnedLines.Count; i++) { file += "\n" + pinnedLines[i]; }
		if (file != "") { File.WriteAllText(Config.PathPinnedDir + "/entries", file.Remove(0, 1)); }
		if (file == "" && File.Exists(Config.PathPinnedDir + "/entries")) { File.Delete(Config.PathPinnedDir + "/entries"); }
	}

	public static void Delete(Entry entry) { Delete(entry.Content, entry.IsImage, entry.IsPinned); }
	public static void Delete(byte[] content, bool isImage, bool isPinned) 
	{
		DeleteLine(Config.PathTmpDir + "/entries", Config.PathTmpDir, content, isImage, isPinned);
		if (isPinned) { DeleteLine(Config.PathPinnedDir + "/entries", Config.PathPinnedDir, content, isImage, isPinned); }
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
		Directory.GetFiles(Config.PathTmpDir, "*").ToList().ForEach(x => File.Delete(x));
		Directory.GetFiles(Config.PathPinnedDir, "*").ToList().ForEach(x => File.Copy(x, x.Replace(Config.PathPinnedDir, Config.PathTmpDir)));
	}

	private static string ConstructLine(byte[] content, bool isImage, bool isPinned, out string filename) 
	{
		string file = Encoding.UTF8.GetString(content).GetHashCode().ToString() + ".entry";
		string line = ((isImage) ? 1 : 0).ToString() + ((isPinned) ? 1 : 0).ToString() + " " + file;
		filename = file;
		return line;
	}

	private static void DeconstructLine(string line, out byte[] content, out bool isImage, out bool isPinned, out string filename) 
	{
		string file = line.Remove(0, 3);
		isImage = line[0] == '1';
		isPinned = line[1] == '1';
		content = File.ReadAllBytes(Config.PathTmpDir + file);
		filename = file;
	}

	private static int Main(string[] args) 
	{
		int index = Array.IndexOf(args, "--config-dir");
		if (index == args.Length - 1) { Console.WriteLine("Invalid options."); return 1; }
		if (index >= 0) { Config.Initialize(args[index + 1]); }
		else { Config.Initialize(null); }

		if (!Directory.Exists(Config.PathConfigDir)) { Directory.CreateDirectory(Config.PathConfigDir); }
		if (!Directory.Exists(Config.PathTmpDir)) { Directory.CreateDirectory(Config.PathTmpDir); }
		if (!Directory.Exists(Config.PathPinnedDir)) { Directory.CreateDirectory(Config.PathPinnedDir); }
		if (!File.Exists(Config.PathTmpDir + "/entries") && File.Exists(Config.PathPinnedDir + "/entries")) { File.WriteAllText(Config.PathTmpDir + "/entries", File.ReadAllText(Config.PathPinnedDir + "/entries")); }

		Stream stream;
		List<byte> bytes;

		if (args.Length == 0) { args = new string[] { "help" }; }
		switch (args[0]) 
		{
			case "-h":
			case "--help":
			case "help":
				Console.WriteLine("Usage:\n\tcliphistory <command> [arguments...] [options]");
				Console.WriteLine("\nCommands:");
				Console.WriteLine("\twindow                       Opens a gtk3 window to select an entry from history.");
				Console.WriteLine("\tstore <type>                 Adds a new entry to history. Value is (text|image).");
				Console.WriteLine("\tpin <isImage> <newIsPinned>  Pins (or unpins a clipboard entry). Values are (true|false).");
				Console.WriteLine("\tdelete <isImage> <IsPinned>  Deletes a clipboard entry. Values are (true|false).");
				Console.WriteLine("\tclear                        Deletes all unpined clipboard entries.");
				Console.WriteLine("\tclearall                     Deletes all (pined and unpined) clipboard entries.");
				Console.WriteLine("\tdefaults                     Creates default configuration and style files.");
				Console.WriteLine("\nOptions:");
				Console.WriteLine("\t--config-dir <dir>           Directory which contains configuration and style files.");
				break;
			case "window":
				Window.Run((File.Exists(Config.PathTmpDir + "/entries")) ? File.ReadAllText(Config.PathTmpDir + "/entries").Split('\n') : new string[] {}, DeconstructLine);
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
				Directory.GetFiles(Config.PathTmpDir, "*").ToList().ForEach(x => File.Delete(x));
				Directory.GetFiles(Config.PathPinnedDir, "*").ToList().ForEach(x => File.Delete(x));
				break;
			case "defaults":
				Config.CreateDefaults();
				break;
			default:
				Console.WriteLine("Unrecognized command.");
				return 1;
		}

		return 0;
	}
}