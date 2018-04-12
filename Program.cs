using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace MeshConvert
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: MeshConvert.exe \"input.json|input.vtk\" [\"output-directory\"]");
				return;
			}
			string inputFilename = args[0];
			string outputDirectory;
			if (args.Length > 1)
			{
				outputDirectory = args[1];
			}
			else
			{
				outputDirectory = Path.GetDirectoryName(inputFilename);
			}

			_ = Converter.Convert(inputFilename, outputDirectory);
		}
	}
}
