//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Diagnostics.CodeAnalysis;

namespace NosCore.PathFinder.Api;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum LogLanguageKey
{
    DATABASE_CONFIGURED,
    NO_DATABASE_CONFIGURED,
    BASE_DIRECTORY,
    CURRENT_DIRECTORY,
    SEARCHING_FOR_CONFIG,
    CHECKING_PATH,
    LOADED_DATABASE_CONFIG,
    FAILED_TO_PARSE_CONFIG,
    USING_CONNECTION_STRING_FROM_APPSETTINGS,
    NO_DATABASE_CONFIGURATION_FOUND,
    LOADED_MAPS_FROM_DATABASE,
    FAILED_TO_LOAD_MAP,
    FAILED_TO_CONNECT_TO_DATABASE,
    API_STARTED,
    LANGUAGE_LOADED
}
