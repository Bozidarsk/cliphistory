using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

public static class Config 
{
	public static int MinHeight { private set; get; }// = 48;
	public static int WindowWidth { private set; get; }// = 400;
	public static int WindowHeight { private set; get; }// = 400;
	// public static string PathTmp { private set; get; }// = "/tmp/cliphistory/entries";
	// public static string PathPinned { private set; get; }// = Home + "/.local/cliphistory/entries";
	public static string PathTmpDir { private set; get; }// = "/tmp/cliphistory/";
	public static string PathPinnedDir { private set; get; }// = Home + "/.local/cliphistory/";
	public static string PathConfigDir { private set; get; }// = Home + "/.config/cliphistory/";
	public static string Css { private set; get; }

	private static readonly string DefaultConfig = "MinHeight = 48\nWindowWidth = 400\nWindowHeight = 450\nPathTmpDir = /tmp/cliphistory/\nPathPinnedDir = $HOME/.local/cliphistory/\nPathConfigDir = $HOME/.config/cliphistory/";
	private static readonly string DefaultCss = "* {\n    font-family: \"Source Code Pro Bold\";\n    font-weight: bold;\n    transition: 0.2s;\n}\n\n/*\nwindow {\n    border: 2px solid @theme_selected_bg_color;\n    background-color: @theme_base_color;\n    border-radius: 8px;\n    font-size: 13px;\n}\n*/\n\n#menu #pin {\n}\n\n#menu #delete {\n}\n\n#menu #clear {\n    \n}\n\n#empty-icon {\n    margin-top: 160px;\n    font-size: 40px;\n}\n\n#empty-text {\n    margin-top: 5px;\n    font-size: 25px;\n}\n\n#box {\n    margin: 4px;\n}\n\n#entry {\n    margin: 0px;\n    padding: 0px;\n    border: 2px solid transparent;\n    border-radius: 8px;\n    background-color: transparent;\n    color: @theme_text_color;\n}\n\n#entry:focus {\n    border: 2px solid transparent;\n    background-color: #38414e;\n}\n\n#entry #text {\n    padding: 10px;\n}\n\n#entry #image {\n    padding: 10px;\n}";

	public static void CreateDefaults() 
	{
		File.WriteAllText(PathConfigDir + "/config", DefaultConfig);
		File.WriteAllText(PathConfigDir + "/style.css", DefaultCss);
	}

	public static void Initialize(string pathConfigDir) 
	{
		PathConfigDir = pathConfigDir ?? (Environment.GetEnvironmentVariable("HOME") + "/.config/cliphistory/");

		string configPath = PathConfigDir + "config";
		string cssPath = PathConfigDir + "style.css";

		Config.Css = (File.Exists(cssPath)) ? File.ReadAllText(cssPath) : Config.DefaultCss;

		string[] config = ((File.Exists(configPath)) ? File.ReadAllText(configPath) : Config.DefaultConfig).Split('\n').Where(x => !x.StartsWith("#")).ToArray();
		for (int i = 0; i < config.Length; i++) 
		{
			if (string.IsNullOrEmpty(config[i])) { continue; }
			int index = config[i].IndexOf("=");
			string name = config[i].Substring(0, index).TrimEnd();
			string content = config[i].Remove(0, index + 1).TrimStart();

			for (Match match = Regex.Match(content, "\\$[a-zA-Z0-9_\\-]+"); match.Success; match = Regex.Match(content, "\\$[a-zA-Z0-9_\\-]+")) 
			{ match.Captures.ToList().ForEach(x => content = content.Replace(x.Value, Environment.GetEnvironmentVariable(x.Value.Remove(0, 1)))); }

			PropertyInfo property = typeof(Config).GetProperty(name);
			if (property == null) { Console.WriteLine("Property '" + name + "' was not found."); continue; }

			try { property.SetValue(null, Convert.ChangeType(content, property.PropertyType)); }
			catch 
			{
				Console.WriteLine("Error parsing '" + content + "' for '" + name + "'.");
				Environment.Exit(1);
			}
		}
	}
}