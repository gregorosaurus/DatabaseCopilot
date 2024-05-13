using System;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using System.Text.Json;

namespace DatabaseChatBot.Api.Services
{
	public class SQLDataProdiverOptions
	{
		public string? ConnectionString { get; set; }
        public string? TableDescriptionFileUrl { get; set; }
	}

	public class SQLDataProvider : IDataProvider
	{
		private SQLDataProdiverOptions _options;
        private ITokenAcquirer _tokenAcuirer;
        public SQLDataProvider(SQLDataProdiverOptions options, ITokenAcquirer tokenAcquirer)
		{
			_options = options;
            _tokenAcuirer = tokenAcquirer;
		}

        private async Task<string> GenerateAccessTokenAsync()
        {
            
            AcquireTokenResult tokenResult = await _tokenAcuirer.GetTokenForAppAsync("https://database.windows.net/.default");
            string? accessToken = tokenResult?.AccessToken;
            if (accessToken == null)
                throw new Exception("Can't retrieve token");
            return accessToken!;
        }

        public async Task<string> ExecuteQueryAsCSVAsync(string sql)
        {
            StringBuilder csvData = new StringBuilder();

            string? accessToken = await GenerateAccessTokenAsync();
            using (SqlConnection connection = new SqlConnection(_options.ConnectionString))
            {
                connection.AccessToken = accessToken;
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        // Write the header row
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            csvData.Append($"\"{reader.GetName(i)}\"");
                            if (i < reader.FieldCount - 1)
                            {
                                csvData.Append(",");
                            }
                        }
                        csvData.AppendLine();

                        // Write the data rows
                        while (await reader.ReadAsync())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                csvData.Append($"\"{reader.GetValue(i)}\"");
                                if (i < reader.FieldCount - 1)
                                {
                                    csvData.Append(",");
                                }
                            }
                            csvData.AppendLine();
                        }
                    }
                }
            }

            return csvData.ToString();
        }

        public async Task<List<string>> GetTablesAsync()
        {
            List<string> tables = new List<string>();

            using (SqlConnection connection = new SqlConnection(_options.ConnectionString))
            {
                connection.AccessToken = await GenerateAccessTokenAsync();
                connection.Open();

                DataTable schema = connection.GetSchema("Tables");

                foreach (DataRow row in schema.Rows)
                {
                    string tableName = row["TABLE_NAME"].ToString() ?? "";
                    tables.Add(tableName);
                }
            }

            return tables;
        }

        public async Task<string> GetTableSchemaAsync(string table)
        {
            string schema = string.Empty;

            using (SqlConnection connection = new SqlConnection(_options.ConnectionString))
            {
                connection.AccessToken = await GenerateAccessTokenAsync();
                await connection.OpenAsync();

                DataTable schemaTable = connection.GetSchema("Columns", new[] { null, null, table });

                foreach (DataRow row in schemaTable.Rows)
                {
                    string columnName = row["COLUMN_NAME"].ToString() ?? "";
                    string dataType = row["DATA_TYPE"].ToString() ?? "";
                    schema += $"{columnName} ({dataType}), ";
                }

                if (!string.IsNullOrEmpty(schema))
                {
                    // Remove the trailing comma and space
                    schema = schema.Remove(schema.Length - 2);
                }
            }

            return schema;
        }

        public async Task<List<Sdk.TableDescription>> GetTableDescriptionsAsync()
        {
            if (_options.TableDescriptionFileUrl == null)
                throw new ArgumentNullException("No table description url specified.");

            if (_options.TableDescriptionFileUrl.StartsWith("http"))
            {
                throw new NotImplementedException("Http not yet supported.");
            }else
            {
                if (!File.Exists(_options.TableDescriptionFileUrl))
                    throw new FileNotFoundException("No table description file found.");

                string json = await File.ReadAllTextAsync(_options.TableDescriptionFileUrl);
                var descriptions = JsonSerializer.Deserialize<List<Sdk.TableDescription>>(json, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });
                if (descriptions == null)
                    throw new Exception("Unable to deserialize json table descriptions.");

                return descriptions!;
            }
        }
    }
}

