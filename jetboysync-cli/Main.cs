using System;
using System.IO;

namespace JetBoySync
{
	class MainClass
	{
		private static Repo repo;
		
		public static void Main () {
			string[] args = System.Environment.GetCommandLineArgs();
			
			if(args.Length < 2) {
				Console.WriteLine("Usage: {0} (directory)", args[0]);
				return;
			}
			
			DirectoryInfo DirInfo = new DirectoryInfo(args[1]);
			repo = new Repo();
			repo.Setup(DirInfo.FullName);
			
			if (!Directory.Exists(Path.Combine(DirInfo.FullName, ".hg"))) {
				if (args.Length < 3){
					Console.WriteLine("No repo found.");
					Console.WriteLine("Usage: {0} (directory) (remote repo)", args[0]);
					return;
				}
				repo.pulling = true;
				repo.hg("clone " + args[2] + " \"" + DirInfo.FullName + "\"");
				repo.pulling = false;
            }
			
			repo.Watch();
			
			while(true){
				repo.pulling = true;
				repo.hg("pull");
				repo.hg("update");
				repo.pulling = false;
				System.Threading.Thread.Sleep(10000);
			}
		}
	}
}
