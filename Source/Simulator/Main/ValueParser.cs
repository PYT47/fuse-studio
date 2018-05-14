﻿using System;
using System.IO;
using System.Reflection;
using Uno.Collections;
using Uno.Compiler;
using Uno.Logging;
using Uno.UX.Markup;
using Uno.UX.Markup.CompilerReflection;
using Uno.UX.Markup.Reflection;

namespace Outracks.Simulator
{
	using Bytecode;

	public class ValueParser
	{
		readonly IDataTypeProvider _compiler;
		readonly Uno.UX.Markup.UXIL.Compiler _valueParser;

		public ValueParser(IDataTypeProvider compiler, string project)
		{
			_compiler = compiler;
			var valueParserCtor = GetUxilCompilerCtor();
			_valueParser = (global::Uno.UX.Markup.UXIL.Compiler)valueParserCtor.Invoke(
				new object[]
				{
					compiler,
					Path.GetDirectoryName(project),
					null,
					new MarkupErrorLog(new Log(Console.Error, Console.Error), SourcePackage.Unknown)
				});

		}

		internal static ConstructorInfo GetUxilCompilerCtor()
		{
			var valueParserType = typeof(global::Uno.UX.Markup.UXIL.Compiler);
			var ctors = valueParserType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
			return ctors.First();
		}

		public AtomicValue Parse(string value, TypeName typeName, FileSourceInfo sourceInfo)
		{
			var dataType = _compiler.TryGetTypeByName(typeName.FullName);
			if (dataType == null)
				throw new Exception("Data type not found: " + typeName.FullName);

			return Parse(value, dataType, sourceInfo);
		}

		public AtomicValue Parse(string value, IDataType dataType, FileSourceInfo sourceInfo)
		{
			var parsedValue = _valueParser.Parse(value, dataType, sourceInfo);
			if (parsedValue == null)
				throw new Exception("Failed to parse property value: " + value);

			return parsedValue;
		}
	}
}