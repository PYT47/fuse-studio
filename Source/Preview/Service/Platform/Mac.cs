using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Outracks;
using Outracks.IPC;
using Uno.Configuration;

namespace Fuse.Preview
{
	public class MacPlatform : IPlatform
	{
		readonly string _monoPath;
		public MacPlatform(string monoPath = null)
		{
			_monoPath = monoPath ?? UnoConfig.Current.GetFullPath("Mono") ?? "/Library/Frameworks/Mono.framework/Commands/mono";
		}

		public IProcess StartProcess(Assembly assembly, params string[] args)
		{
			var exePath = assembly.CodeBase.StripPrefix("file://");

			var startInfo = new ProcessStartInfo
			{
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				FileName = _monoPath,
				Arguments = ProcessArguments.PackList(new [] { exePath }.Concat(args))
			};
			
			Process.Start(startInfo);
			return new MacProcess(args);
		}

		public Stream CreateStream(string name)
		{
			var pipeName = new PipeName(GetProcessIdentifier() + name);
			try
			{
				return new UnixSocketStream(pipeName, SocketUsage.Host);
			}
			catch (Exception)
			{
				UnixSocketStream.Unlink(pipeName);
			}

			return new UnixSocketStream(pipeName, SocketUsage.Host);
		}

		public IEnsureSingleInstance EnsureSingleInstance()
		{
			return new EnsureSingleInstanceMac(GetProcessIdentifier(), ReportFactory.FallbackReport);
		}

		public IProcess StartSingleProcess(Assembly assembly, params string[] args)
		{
			using (var ensureSingleInstance = new EnsureSingleInstanceMac(args.Join("-"), ReportFactory.FallbackReport))
			{
				if (ensureSingleInstance.IsAlreadyRunning())
					return new MacProcess(args);
			}

			return StartProcess(assembly, args);
		}

		string GetProcessIdentifier()
		{
			return Environment.GetCommandLineArgs().Skip(1).Join("-");
		}
	}

	
	class MacProcess : IProcess
	{
		public MacProcess(IList<string> arguments)
		{
			Arguments = arguments;
		}

		public IList<string> Arguments { get; private set; }

		public Stream OpenStream(string name)
		{
			return new UnixSocketStream(new PipeName(GetProcessIdentifier() + name), SocketUsage.Client);
		}

		public string GetProcessIdentifier()
		{
			return Arguments.Join("-");
		}
	}
}