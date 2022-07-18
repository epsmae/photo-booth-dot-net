namespace PhotoBooth.Abstraction.Configuration
{
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Deletes all the settings
        /// </summary>
        void DeleteAll();

        /// <summary>
        /// Save a generic configuration entry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurationId"></param>
        /// <param name="value"></param>
        void SaveConfiguration<T>(string configurationId, ConfigurationEntry<T> value);

        /// <summary>
        /// Load a generic configuration entry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurationId"></param>
        /// <returns></returns>
        T LoadConfiguration<T>(string configurationId) where T : ConfigurationEntry;

        /// <summary>
        /// Load a configuration entry
        /// </summary>
        /// <param name="configurationId"></param>
        /// <returns></returns>
        ConfigurationEntry LoadConfiguration(string configurationId);

        /// <summary>
        /// Delete the specified configuration
        /// </summary>
        /// <param name="configurationId"></param>
        void Delete(string configurationId);
    }
}
