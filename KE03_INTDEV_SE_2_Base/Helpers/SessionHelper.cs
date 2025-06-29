// Importeert JSON serialization functionaliteit
using System.Text.Json;

namespace KE03_INTDEV_SE_2_Base.Helpers
{
    /// <summary>
    /// Helper klasse met extensie methoden voor ASP.NET Core Session management.
    /// Biedt eenvoudige JSON serialization/deserialization voor complexe objecten in sessies.
    /// </summary>
    public static class SessionHelper
    {
        /// <summary>
        /// Extensie methode om een complex object als JSON string op te slaan in de sessie.
        /// Serialiseert het object naar JSON en slaat het op onder de gegeven key.
        /// </summary>
        /// <param name="session">De HTTP sessie instantie</param>
        /// <param name="key">De unieke key om het object onder op te slaan</param>
        /// <param name="value">Het object om te serialiseren en op te slaan</param>
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        /// <summary>
        /// Extensie methode om een object uit de sessie te halen en te deserializen.
        /// Haalt de JSON string op en deserialiseert het naar het gewenste type.
        /// </summary>
        /// <typeparam name="T">Het type waarnaar gedeserialiseerd moet worden</typeparam>
        /// <param name="session">De HTTP sessie instantie</param>
        /// <param name="key">De key waaronder het object is opgeslagen</param>
        /// <returns>Het gedeserialiseerde object of default(T) als niet gevonden</returns>
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonSerializer.Deserialize<T>(value);
        }
    }
}
