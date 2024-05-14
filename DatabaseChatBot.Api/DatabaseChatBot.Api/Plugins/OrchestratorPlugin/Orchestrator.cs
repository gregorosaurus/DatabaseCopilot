using System;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;

namespace DatabaseChatBot.Api.Plugins.OrchestratorPlugin
{
    public class Orchestrator
    {
        private readonly Kernel _kernel;
        private readonly Services.IDataProvider _dataProvider;

        private string? _groundingDataDescription;

        public Orchestrator(Kernel kernel, Services.IDataProvider dataProvider, IConfiguration configuration)
        {
            _kernel = kernel;
            _groundingDataDescription = configuration.GetValue<string>("GorundingDataDescription");
            _dataProvider = dataProvider;
        }

        public async Task<string> RouteRequestAsync(string input)
        {
            bool isValidRequest = true;// await DetermineIfValidPrompt(input);
            if(!isValidRequest)
            {
                return "I'm sorry, I'm not able to answer that question.";
            }

            string? tableToUse = await DetermineWhichTableToQueryAsync(input);
            if(tableToUse == null)
            {
                return "I'm unable to determine how to query this question, sorry.";
            }

            string? sql = await GenerateSQLQueryAsync(input, tableToUse!);
            if(sql == null)
            {
                return "I'm unable to generate a query for this request.";
            }
            Console.WriteLine($"SQL Generated: {sql}");

            try
            {
                string csvData = await _dataProvider.ExecuteQueryAsCSVAsync(sql);

                string? summarizedData = await SummarizeCSVData(input, csvData);

                return (summarizedData ?? "") + $"\n```{csvData}```";
            }
            catch (Exception e)
            {
                return $"An error occurred while executing a query. {e.Message}";
            }
        }

        public async Task<string?> SummarizeCSVData(string input, string csvData)
        {
            var result = await _kernel.InvokeAsync("OrchestratorPlugin", "SummarizeCSVData", new KernelArguments()
            {
                {"input" , input },
                {"csv_data",csvData  },
            });

            return result?.GetValue<string>();
        }

        public async Task<string?> GenerateChartJavascript(string inputCsvData)
        {
            var result = await _kernel.InvokeAsync("OrchestratorPlugin", "GenerateChartJS", new KernelArguments()
            {
                { "csv_data", inputCsvData }
            });

            var value = result?.GetValue<string>() ?? "";

            Match codeMatch = Regex.Match(value, @"```([\s\S.]*?)```");
            if (codeMatch.Success)
            {
                var code = codeMatch.Groups[1].Value;
                return code.Replace("let chartConfig = ", "")
                    .Replace("let chartConfig=", "")
                    .Replace(";", "")
                    .Replace("json","")
                    .Trim();
            }
            else
            {
                throw new Exception($"Unable to generate code: {value}");
            }
        }

        public async Task<string?> DetermineWhichTableToQueryAsync(string input)
        {
            var result  = await _kernel.InvokeAsync("OrchestratorPlugin", "DetermineTable", new KernelArguments()
            {
                {"input" , input },
                {"table_description",  await GetTableDescriptionsStringValueAsync()},
            });

            IEnumerable<string> availableTables = await _dataProvider.GetTablesAsync();

            string? tableResult = result?.GetValue<string>();

            return availableTables.Where(x => tableResult?.ToLower().Contains(x.ToLower()) ?? false).FirstOrDefault();
        }

        private async Task<bool> DetermineIfValidPrompt(string input)
        {
            var result = await _kernel.InvokeAsync("OrchestratorPlugin", "CheckPromptSafety", new KernelArguments()
            {
                { "grounding_comment", _groundingDataDescription ?? "" },
                { "input" , input }
            });

            return result?.GetValue<string>()?.ToLower().Contains("true") ?? false;
        }

        private async Task<string?> GenerateSQLQueryAsync(string input, string table) {
            var result = await _kernel.InvokeAsync("OrchestratorPlugin", "GenerateSQL", new KernelArguments()
            {
                {"schema", await _dataProvider.GetTableSchemaAsync(table) },
                {"table_description" , await GetTableDescriptionsStringValueAsync() },
                {"input" , input },
                {"table_name", table }
            });

            var sql = result?.GetValue<string>()?.ToLower();
            return sql;
        }


        private async Task<string> GetTableDescriptionsStringValueAsync()
        {
            var tableDescriptions = await _dataProvider.GetTableDescriptionsAsync();
            var tableDescriptionsStringValue = string.Join("\r\n\r\n", tableDescriptions.Select(x => $"Table '{x.TableName}':\r\n{x.Description}").ToArray());

            return tableDescriptionsStringValue;
        }
    }
}

