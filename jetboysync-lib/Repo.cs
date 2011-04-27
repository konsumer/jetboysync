using System;
using System.IO;
using System.Security.Permissions;

namespace JetBoySync
{
	public class Repo
	{
		private static string local_dir;
		private static FileSystemWatcher watcher;
		public bool pulling;
		
		public void Setup(string m_local){
			local_dir = m_local;
			pulling = false;
		}
		
		[PermissionSet(SecurityAction.Demand, Name="FullTrust")]
		public void Watch(string m_filter) {			
			watcher = new FileSystemWatcher();
			watcher.Path = local_dir;
			watcher.IncludeSubdirectories = true;
			watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
			watcher.Filter = m_filter;
			watcher.Changed += new FileSystemEventHandler(OnChanged);
			watcher.Created += new FileSystemEventHandler(OnChanged);
			watcher.Deleted += new FileSystemEventHandler(OnChanged);
			watcher.Renamed += new RenamedEventHandler(OnChanged);
			watcher.EnableRaisingEvents = true;
		}
		
		public void Watch() {
			Watch("*.*");
		}		

		public void hg(string command) {
			try {
				if (Directory.Exists(local_dir)){
				    Directory.SetCurrentDirectory(local_dir);
				}
				
				// TODO: I should do some string-parsing of output in here, so I can return useful bits from program output
				
				System.Diagnostics.ProcessStartInfo procStartInfo =
				new System.Diagnostics.ProcessStartInfo("hg", command);
				procStartInfo.RedirectStandardOutput = true;
				procStartInfo.UseShellExecute = false;
				procStartInfo.CreateNoWindow = true;
				System.Diagnostics.Process proc = new System.Diagnostics.Process();
				proc.StartInfo = procStartInfo;
				proc.Start();
				string result = proc.StandardOutput.ReadToEnd();
				Console.WriteLine("hg {0}", command);
				Console.WriteLine(result);
			} catch {}
		}
		
		
		
		private void OnChanged(object source, FileSystemEventArgs e) {
			string hgpath = Path.Combine(local_dir, ".hg");
			bool isHG = Path.GetDirectoryName(e.FullPath) == hgpath || Path.GetDirectoryName(e.FullPath) == Path.Combine(hgpath, "store");	
			
			// these happen in gnome. they are just noisy temp files...
			bool isGobj = false;
			try{
				isGobj = Path.GetFileName(e.FullPath).Substring(0, 15) == ".goutputstream-";
			}catch{}
			
			if (!isHG && !pulling && !isGobj){				
				hg("addremove");
				hg("commit -m \"" + e.ChangeType + ": " + e.FullPath + "\"");				
				hg("push");
			}
    	}
		
    	private void OnChanged(object source, RenamedEventArgs e) {
			string hgpath = Path.Combine(local_dir, ".hg");
			bool isHG = Path.GetDirectoryName(e.FullPath) == hgpath || Path.GetDirectoryName(e.FullPath) == Path.Combine(hgpath, "store");
			
			// these happen in gnome. they are just noisy temp files...
			bool isGobj = false;
			try{
				isGobj = Path.GetFileName(e.FullPath).Substring(0, 15) == ".goutputstream-";
			}catch{}
			
			if (!isHG && !pulling && !isGobj){		
				hg("addremove");
				hg("commit -m \"Renamed: " + e.OldFullPath + " to " + e.FullPath + "\"");
				hg("push");
			}
    	}
	}
}
