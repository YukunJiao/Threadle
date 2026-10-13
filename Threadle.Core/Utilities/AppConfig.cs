using System.Text.Json;

namespace Threadle.Core.Utilities
{
    /// <summary>
    /// Static class for reading the optional configuration file 'appconfig.json', which is looked for
    /// in the same folder as the Threadle executable. The file lets e.g. system administrators set
    /// startup defaults and limits, irrespective of where Threadle is installed. Example:
    /// <code>
    /// {
    ///   "maxthreadslimit": 8,
    ///   "defaultmaxthreads": 4
    /// }
    /// </code>
    /// 'maxthreadslimit' is the limit for the 'maxthreads' setting (-1 = no limit other than the number of
    /// available cores), and 'defaultmaxthreads' is the value of the 'maxthreads' setting at startup
    /// (-1 = unconstrained, or the limit if 'maxthreadslimit' is set). Both default to -1.
    /// </summary>
    public static class AppConfig
    {
        #region Fields
        /// <summary>
        /// The name of the configuration file.
        /// </summary>
        public const string FileName = "appconfig.json";
        #endregion


        #region Properties
        /// <summary>
        /// Gets the path where the configuration file is looked for: the folder of the executable.
        /// AppContext.BaseDirectory is used as it also works for single-file published executables.
        /// </summary>
        public static string DefaultPath => Path.Combine(AppContext.BaseDirectory, FileName);

        /// <summary>
        /// Gets the path of the configuration file that was loaded, or null if none was loaded.
        /// </summary>
        public static string? LoadedPath { get; private set; }
        #endregion


        #region Methods
        /// <summary>
        /// Loads the configuration file next to the executable, if it exists.
        /// </summary>
        /// <returns>A list of warnings about invalid content (empty if all is fine or if there is no file).</returns>
        public static List<string> Load() => Load(DefaultPath);

        /// <summary>
        /// Loads the configuration file at the provided path, if it exists, and applies its settings.
        /// A missing file is not an error: the settings then keep their defaults. Invalid content results
        /// in warnings, where the invalid entries (or the whole file, if it can't be parsed) are ignored.
        /// </summary>
        /// <param name="path">The path to the configuration file.</param>
        /// <returns>A list of warnings about invalid content (empty if all is fine or if there is no file).</returns>
        public static List<string> Load(string path)
        {
            var warnings = new List<string>();
            LoadedPath = null;
            if (!File.Exists(path))
                return warnings;

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(File.ReadAllText(path), new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
            }
            catch (Exception ex) when (ex is JsonException || ex is IOException || ex is UnauthorizedAccessException)
            {
                warnings.Add($"Could not read configuration file '{path}', so it is ignored: {ex.Message}");
                return warnings;
            }

            int maxThreadsLimit = -1, defaultMaxThreads = -1;
            using (document)
            {
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    warnings.Add($"Configuration file '{path}' must contain a JSON object, so it is ignored.");
                    return warnings;
                }
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    switch (property.Name.ToLowerInvariant())
                    {
                        case "maxthreadslimit":
                            if (TryGetThreadsValue(property, path, warnings, out int maxThreadsLimitValue))
                                maxThreadsLimit = maxThreadsLimitValue;
                            break;
                        case "defaultmaxthreads":
                            if (TryGetThreadsValue(property, path, warnings, out int defaultMaxThreadsValue))
                                defaultMaxThreads = defaultMaxThreadsValue;
                            break;
                        default:
                            warnings.Add($"Unknown setting '{property.Name}' in configuration file '{path}' is ignored.");
                            break;
                    }
                }
            }

            UserSettings.MaxThreadsLimit = maxThreadsLimit;
            if (defaultMaxThreads == -1)
                UserSettings.MaxDegreeOfParallelism = maxThreadsLimit == -1 ? -1 : UserSettings.EffectiveMaxThreadsLimit;
            else if (defaultMaxThreads > UserSettings.EffectiveMaxThreadsLimit)
            {
                warnings.Add($"Setting 'defaultmaxthreads' ({defaultMaxThreads}) in configuration file '{path}' exceeds the maximum number of threads allowed ({UserSettings.EffectiveMaxThreadsLimit}), so {UserSettings.EffectiveMaxThreadsLimit} is used.");
                UserSettings.MaxDegreeOfParallelism = UserSettings.EffectiveMaxThreadsLimit;
            }
            else
                UserSettings.MaxDegreeOfParallelism = defaultMaxThreads;

            LoadedPath = path;
            return warnings;
        }

        /// <summary>
        /// Reads a thread count from a configuration property, which must be -1 or a positive integer.
        /// </summary>
        private static bool TryGetThreadsValue(JsonProperty property, string path, List<string> warnings, out int value)
        {
            if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out value) && (value == -1 || value >= 1))
                return true;
            warnings.Add($"Setting '{property.Name}' in configuration file '{path}' must be -1 or a positive integer, got '{property.Value.GetRawText()}', so it is ignored.");
            value = -1;
            return false;
        }
        #endregion
    }
}