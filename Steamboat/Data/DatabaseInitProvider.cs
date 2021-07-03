using Steamboat.Util;

namespace Steamboat.Data
{
    internal static class DatabaseInitProvider
    {
        /// <summary>
        /// Get the SQL needed to create the initial database structure.
        /// </summary>
        public static string GetInitScript()
        {
            return EmbeddedResourcesUtil.ReadString(
                $"{nameof(Steamboat)}.{nameof(Data)}.database.sql", typeof(DatabaseInitProvider).Assembly);
        }

        /// <summary>
        /// In case a breaking change is made in the init script, this number should get incremented. 
        /// </summary>
        public static int InitScriptVersion => 1;
    }
}