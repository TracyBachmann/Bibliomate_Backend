using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.Configuration
{
    /// <summary>
    /// Represents the configuration settings required to connect to MongoDB.
    /// </summary>
    /// <remarks>
    /// These settings are typically bound from the "Mongo" section of <c>appsettings.json</c>.
    /// They can be injected into services via <c>IOptions&lt;MongoSettings&gt;</c>.
    /// </remarks>
    public class MongoSettings
    {
        /// <summary>
        /// Gets or sets the MongoDB connection string.
        /// </summary>
        /// <value>
        /// A valid MongoDB connection string including credentials and host
        /// (e.g. <c>mongodb://user:pass@host:27017</c>).
        /// </value>
        [Required(ErrorMessage = "ConnectionString for MongoDB must be provided.")]
        public string ConnectionString { get; set; } = null!;

        /// <summary>
        /// Gets or sets the name of the MongoDB database to use.
        /// </summary>
        /// <value>
        /// The database name where collections are stored.
        /// </value>
        [Required(ErrorMessage = "DatabaseName for MongoDB must be provided.")]
        public string DatabaseName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the name of the MongoDB collection where notification logs are stored.
        /// </summary>
        /// <value>
        /// The collection name that stores <see cref="Models.Mongo.NotificationLogDocument"/> records.
        /// Defaults to <c>"logEntries"</c> if not explicitly configured.
        /// </value>
        [Required(ErrorMessage = "LogCollectionName for MongoDB must be provided.")]
        public string LogCollectionName { get; set; } = "logEntries";
    }
}