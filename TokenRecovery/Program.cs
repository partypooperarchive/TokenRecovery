//#define DDEBUG
/*
 * Created by SharpDevelop.
 * User: User
 * Date: 29.06.2021
 * Time: 20:13
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace TokenRecovery
{
	class Program
	{
		public static void Main(string[] args)
		{
			#if DDEBUG
			var config_path = @"Z:\OUT\config.ini";
			var dll_path = @"Z:\OUT\DummyDll\Assembly-CSharp.dll";
			var output_path = @"Z:\OUT\types_tokens.csv";
			#else
			if (args.Length < 3)
			{
				Usage();
				return;
			}
			var config_path = args[0];
			var dll_path = args[1];
			var output_path = args[2];
			#endif
			
			var config = new IniReader(config_path);
			var base_class = config.GetValue("ProtobufBaseClass", "Settings");
			var cmd_id = config.GetValue("CmdIdName", "Settings");
			
			var ap = new AssemblyParser(dll_path, base_class, cmd_id);
			
			ap.parse();
			
			ap.writeToCSV(output_path);
		}
		
		public static void Usage() {
			var param_string = "\t{0,-15} {1}";
			
			var usage = string.Join(
				Environment.NewLine,
				"Token dumper tool for Unity projects",
				"",
				"Usage:",
				string.Format("\t{0} config input_dll output_csv", AppDomain.CurrentDomain.FriendlyName),
				"",
				"Parameters:",
				string.Format(param_string, "config", "Path to the config.ini describing specific game version"),
				string.Format(param_string, "input_dll", "Path to the Assembly-CSharp.dll"),
				string.Format(param_string, "output_csv", "Path to the CSV output file"),
				""
			);
			Console.WriteLine(usage);
		}
	}
}