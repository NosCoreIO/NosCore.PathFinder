//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Globalization;
using System.Resources;
using NosCore.Shared.Enumerations;

namespace NosCore.PathFinder.Gui.I18N
{
    public sealed class LogLanguage
    {
        private static LogLanguage? _instance;

        private static readonly CultureInfo ResourceCulture = new CultureInfo(Language.ToString());

        private readonly ResourceManager _manager;

        private LogLanguage()
        {
            var assem = typeof(LogLanguageKey).Assembly;
            _manager = new ResourceManager(
                assem.GetName().Name + ".Resource.LocalizedResources",
                assem);
        }

        public static RegionType Language { get; set; }

        public static LogLanguage Instance => _instance ??= new LogLanguage();

        public string GetMessageFromKey(LogLanguageKey messageKey)
        {
            return GetMessageFromKey(messageKey, null);
        }

        public string GetMessageFromKey(LogLanguageKey messageKey, string? culture)
        {
            var cult = culture != null ? new CultureInfo(culture) : ResourceCulture;
            var resourceMessage = (_manager != null)
                ? _manager.GetResourceSet(cult, true,
                        cult.TwoLetterISOLanguageName == default(RegionType).ToString().ToLower(cult))
                    ?.GetString(messageKey.ToString()) : string.Empty;

            return !string.IsNullOrEmpty(resourceMessage) ? resourceMessage : $"#<{messageKey}>";
        }
    }
}