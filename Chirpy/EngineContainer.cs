namespace Chirpy
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ChirpyInterface;

	public class EngineContainer : IEngine
	{
		public IEnumerable<Lazy<IEngine, IEngineMetadata>> Engines { get; set; }

		public string Name { get; private set; }
		public string Category { get; private set; }
		public bool[] Internal { get; private set; }
		public bool Minifier { get; private set; }

		public EngineContainer(IEnumerable<Lazy<IEngine, IEngineMetadata>> engines)
		{
			Engines = engines;

			if(!engines.Any())
			{
				Internal = new bool[0];
				return;
			}

      Name = string.Join("|", engines.Select(x => x.Metadata.Name).Distinct().OrderBy(x => x));
      Category = string.Join("|", engines.Select(x => x.Metadata.Category).Distinct().OrderBy(x => x));
      Minifier = engines.Any(x => x.Metadata.Minifier);

			Internal = engines.Select(e => e.Metadata.Internal)
				.Distinct()
				.ToArray();
		}

		public List<string> GetDependencies(string contents, string filename)
		{
			return Execute(e => e.GetDependencies(contents, filename))
        .SelectMany(x=>x)
        .Where(x=>x!=null)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();
		}

		public List<EngineResult> Process(string contents, string filename)
		{
      return Execute(e => e.Process(contents, filename)).SelectMany(x => x).ToList();
		}

    IEnumerable<T> Execute<T>(Func<IEngine, T> action) {
      foreach (var engine in Engines) {
        var result = default(T);
        if (Try(action, engine, out result))
          yield return result;
        else yield break;
      }
    }

		bool HasInternalEngine()
		{
			if(Internal.Length == 0) return false;

			return Internal.Length == 2 || Internal[0];
		}

		bool HasExternalEngine()
		{
			if(Internal.Length == 0) return false;

			return Internal.Length == 2 || !Internal[0];
		}

		bool Try<T>(Func<IEngine, T> action, Lazy<IEngine, IEngineMetadata> engine, out T result)
		{
			if(engine == null)
			{
				result = default(T);
				return false;
			}

			try
			{
				result = action(engine.Value);
				return true;
			}
			catch (ChirpyException e)
			{
//				if(Internal.Length == 1 || engine.Metadata.Internal)
//				{
//					// Log exception
//				}
			}
			catch (Exception e)
			{
//				if(Internal.Length == 1 || engine.Metadata.Internal)
//				{
//					// Log exception
//				}
			}

			result = default(T);
			return false;
		}
	}
}