namespace BackendBiblioMate.Configuration
{
    /// <summary>
    /// Represents the settings required to connect to MongoDB.
    /// </summary>
    /// <remarks>
    /// These settings are bound from the "Mongo" section of appsettings.json 
    /// and injected via IOptions&lt;MongoSettings&gt; in Startup.ConfigureServices().
    /// </remarks>
    public class MongoSettings
    {
        /// <summary>
        /// Gets or sets the MongoDB connection string, including credentials and host.
        /// </summary>
        /// <value>
        /// A valid MongoDB connection string (e.g. "mongodb://user:pass@host:27017").
        /// </value>
        [System.ComponentModel.DataAnnotations.Required(
            ErrorMessage = "ConnectionString for MongoDB must be provided.")]
        public string ConnectionString { get; set; } = null!;

        /// <summary>
        /// Gets or sets the name of the MongoDB database to use.
        /// </summary>
        /// <value>
        /// The database name as defined in the data access layer.
        /// </value>
        [System.ComponentModel.DataAnnotations.Required(
            ErrorMessage = "DatabaseName for MongoDB must be provided.")]
        public string DatabaseName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the name of the collection where notification logs are stored.
        /// </summary>
        /// <value>
        /// The collection name for <see cref="Models.Mongo.NotificationLogDocument"/>.
        /// </value>
        [System.ComponentModel.DataAnnotations.Required(
            ErrorMessage = "LogCollectionName for MongoDB must be provided.")]
        public string LogCollectionName { get; set; } = "logEntries";
    }
}