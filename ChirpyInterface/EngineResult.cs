﻿namespace ChirpyInterface
{
	using System.Collections.Generic;

	public class EngineResult
	{
		public string Extension { get; set; }
		public string FileName { get; set; }
		public string Contents { get; set; }
		public List<ChirpyException> Exceptions { get; private set; }

		public EngineResult()
		{
			Exceptions = new List<ChirpyException>();
		}

		public void AddException(ChirpyException exception)
		{
			Exceptions.Add(exception);
		}

		public void AddException(string message, string filename, int lineNumber, int position, string line, ErrorCategory category)
		{
			var exception = new ChirpyException(message, filename, lineNumber, position, line, category);

			AddException(exception);
		}

		public void AddException(string message, string filename, ErrorCategory category)
		{
			var exception = new ChirpyException(message, filename, category);

			AddException(exception);
		}
	}
}