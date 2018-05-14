using System;
using Newtonsoft.Json;

namespace SketchConverter.SketchModel

{
	public class SketchEntity
	{
		public readonly Guid Id;

		public SketchEntity(Guid id)
		{
			Id = id;
		}
	}
}
