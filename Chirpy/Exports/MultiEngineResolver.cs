namespace Chirpy.Exports
{
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.ComponentModel.Composition;
  using System.Linq;
  using ChirpyInterface;
  using Imports;

	[Export(typeof (IEngineResolver))]
	[Export(typeof (IInternalEngineResolver))]
	public class MultiEngineResolver : IInternalEngineResolver, IEngineResolver
	{
    public MultiEngineResolver() {
      EngineCache = new ConcurrentDictionary<string, EngineContainer>();
    }

		[ImportMany]
		public IEnumerable<Lazy<IEngine, IEngineMetadata>> Engines { get; set; }

		[Import]
		public IExtensionResolver ExtensionResolver { get; set; }

		protected ConcurrentDictionary<string, EngineContainer> EngineCache { get; set; }

		Dictionary<string, string> extensions;
		Dictionary<string, string> Extensions
		{
			get
			{
				if (extensions == null)
					extensions = ReloadExtensions();

				return extensions;
			}
		}

		Dictionary<string, string> ReloadExtensions()
		{
			return Engines
				.Select(e => e.Metadata.Category)
				.Distinct()
				.ToDictionary(
					e => e,
					e => ExtensionResolver.GetExtensionFromCategory(e));
    }

    IEngine IEngineResolver.GetEngine(string category) {
      return GetEngine(category);
    }

    IEngine IEngineResolver.GetEngineByName(string name) {
      return GetEngineByName(name);
    }

    IEngine IEngineResolver.GetEngineByFilename(string filename) {
      return GetEngineByFilename(filename);
    }

		public EngineContainer GetEngine(string category)
		{
      var key = GetCategoryKey(category);
      return EngineCache.GetOrAdd(key, _key => {
        //NOTE: `category` can be referenced here, but I'm unsure about the thread safety of it
        var _category = _key.Substring(_key.IndexOf(':') + 1);
        var categories = category.Split('|');
        return new EngineContainer(Engines
          .Where(e => categories.Contains(e.Metadata.Category, StringComparer.InvariantCultureIgnoreCase))
          .ToList());
      });
		} 

		public EngineContainer GetEngineByName(string name) 
    {
      return EngineCache.GetOrAdd(name, _name => {
        var names = _name.Split('|');
        return new EngineContainer(Engines
          .Where(e => names.Contains(e.Metadata.Name, StringComparer.InvariantCultureIgnoreCase))
          .ToList());
      });
		} 

		public EngineContainer GetEngineByFilename(string filename)
		{
			var engines = Engines
				.Where(e => e.Metadata.Category.Contains("."))
				.Where(e => FileMatchesEngine(filename, e.Metadata))
				.ToList();

			if (!engines.Any())
				engines = Engines
					.Where(e => !e.Metadata.Category.Contains("."))
					.Where(e => FileMatchesEngine(filename, e.Metadata))
					.ToList();

			if (!engines.Any())
				return null;

			return GetEngineByName(string.Join("|", engines.Select(x=>x.Metadata.Name)));
		}

		bool FileMatchesEngine(string filename, IEngineMetadata metadata)
		{
			return filename.EndsWith(Extensions[metadata.Category]);
		}

		static string GetCategoryKey(string category)
		{
			var key = string.Format("Category::{0}", category);
			return key;
		}
	}
}