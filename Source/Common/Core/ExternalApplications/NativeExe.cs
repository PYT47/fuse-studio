﻿using System;
using System.Diagnostics;
using System.IO;

namespace Outracks.IO
{
	class NativeExe : IExternalApplication
	{
		readonly AbsoluteFilePath _exeFile;

		public NativeExe(AbsoluteFilePath exeFile)
		{
			_exeFile = exeFile;
		}

		public Process Start(Optional<ProcessStartInfo> startInfo)
		{
			//Console.WriteLine("Starting `" + _exeFile.NativePath + "` with arguments `" + startInfo.Select(s => s.Arguments).Or("") + "`");
			var newStartInfo = startInfo.Or(new ProcessStartInfo());
			newStartInfo.FileName = _exeFile.NativePath;
			return Process.Start(newStartInfo);
		}

		public Process Open(IAbsolutePath fileName, Optional<ProcessStartInfo> startInfo)
		{
			var newStartInfo = startInfo.Or(new ProcessStartInfo());
			newStartInfo.Arguments = "\"" + fileName + "\" " + newStartInfo.Arguments;

			return Start(newStartInfo);
		}

		public string Name
		{
			get { return _exeFile.Name.ToString(); }
		}

		public bool Exists
		{
			get { return File.Exists(_exeFile.NativePath); }
		}
	}
}
