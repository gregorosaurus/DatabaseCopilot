using System;
namespace DatabaseChatBot.Api.Services
{
	public interface IDataProvider
	{
		Task<List<string>> GetTablesAsync();
		Task<string> GetTableSchemaAsync(string table);
		Task<string> ExecuteQueryAsCSVAsync(string sql);
		Task<List<Sdk.TableDescription>> GetTableDescriptionsAsync();
    }
}

