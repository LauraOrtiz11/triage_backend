using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace triage_backend.Utilities
{
    /// <summary>
    /// Class responsible for managing the database connection.
    /// Ensures safe opening and closing of connections.
    /// </summary>
    public class ContextDB : IDisposable
    {
        private readonly string _connectionString;
        private IDbConnection? _connection;

        /// <summary>
        /// Constructor that gets the connection string from configuration.
        /// </summary>
        /// <param name="configuration">Application configuration (appsettings.json).</param>
        public ContextDB(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("The connection string 'DefaultConnection' is not configured.");
        }

        /// <summary>
        /// Opens and returns a database connection.
        /// </summary>
        /// <returns>IDbConnection already opened.</returns>
        public IDbConnection OpenConnection()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
            }

            return _connection;
        }

        /// <summary>
        /// Closes the database connection if it is open.
        /// </summary>
        public void CloseConnection()
        {
            if (_connection != null && _connection.State != ConnectionState.Closed)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        /// <summary>
        /// Releases the resources associated with the connection (Dispose pattern).
        /// </summary>
        public void Dispose()
        {
            CloseConnection();
        }
    }
}
