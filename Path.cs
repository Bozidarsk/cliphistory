using System;

public static class Path 
{
	public static readonly string Home = Environment.GetEnvironmentVariable("HOME");
	public static readonly string Css = Path.Home + "/.config/cliphistory/style.css";
	public static readonly string Selected = "/tmp/cliphistoryselectedentry";
	public static readonly string Tmp = "/tmp/cliphistory/entries";
	public static readonly string Pinned = Path.Home + "/.local/cliphistory/entries";
	public static readonly string TmpDir = "/tmp/cliphistory/";
	public static readonly string PinnedDir = Path.Home + "/.local/cliphistory/";
	public static readonly string EntryExtension = ".entry";
}