using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ReadLargeData.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ReadLargeData
{
    public class App
    {
        DataModelContext _dataModelContext;
        IConfiguration _configuration;

        public App(DataModelContext dataModelContext, IConfiguration configuration)
        {
            _dataModelContext = dataModelContext;
            _configuration = configuration;
        }

        public void Run()
        {
            if (!_dataModelContext.Data.AnyAsync().GetAwaiter().GetResult())
            {
                 SeedDataAsync().GetAwaiter().GetResult();
            }

            MeasureMemoryUsageAsync(nameof(GetData_Using_AsyncEnumerable), GetData_Using_AsyncEnumerable).GetAwaiter().GetResult();
            MeasureMemoryUsageAsync(nameof(GetData_Using_List), GetData_Using_List).GetAwaiter().GetResult();
            MeasureMemoryUsageAsync(nameof(GetData_Using_ListAsync), GetData_Using_ListAsync).GetAwaiter().GetResult();
            MeasureMemoryUsageAsync(nameof(GetData_Using_Dapper), GetData_Using_Dapper).GetAwaiter().GetResult();
            MeasureMemoryUsageAsync(nameof(GetData_Using_DapperAsync), GetData_Using_DapperAsync).GetAwaiter().GetResult();
            MeasureMemoryUsageAsync(nameof(GetData_Using_DapperList), GetData_Using_DapperList).GetAwaiter().GetResult();

            Console.WriteLine("===========");
            Console.WriteLine("===========");
            Console.WriteLine("===========");

            MeasureMemoryUsageAsync(nameof(GetData_Using_AsyncEnumerable_With_Out_ConfigureAwait), GetData_Using_AsyncEnumerable_With_Out_ConfigureAwait).GetAwaiter().GetResult();
            MeasureMemoryUsageAsync(nameof(GetData_Using_ListAsync_With_Out_ConfigureAwait), GetData_Using_ListAsync).GetAwaiter().GetResult();
            MeasureMemoryUsageAsync(nameof(GetData_Using_DapperAsync_With_Out_ConfigureAwait), GetData_Using_DapperAsync).GetAwaiter().GetResult();
        }

        public async Task SeedDataAsync()
        {
            var prepareData = new List<MyData>();
            for (int i = 0; i < 2100000; i++)
            {
                prepareData.Add(new MyData()
                {
                    EmployeeID = Guid.NewGuid(),
                    Department = "Department" + DateTime.Now.ToString(),
                    Name = "Name" + DateTime.Now.ToString()
                });

                if (i % 1000 == 0)
                {
                    await _dataModelContext.AddRangeAsync(prepareData);
                    await _dataModelContext.SaveChangesAsync();
                    prepareData = new List<MyData>();
                }
            }
        }

        public async Task GetData_Using_AsyncEnumerable()
        {
            var fileInfo = new FileInfo("DataAsyncEnumerable.csv");

            if (fileInfo.Exists)
            {
                fileInfo.Delete();  // Ensure file is removed before writing
            }

            using (var writer = new StreamWriter(fileInfo.FullName))
            {
                // Write the header
                await writer.WriteLineAsync("EmployeeID,Name,Department");


                await foreach (var data in _dataModelContext.Data.FromSqlRaw("Select * From data").AsNoTracking().AsAsyncEnumerable().ConfigureAwait(false))
                {
                    var line = $"{data.EmployeeID},{data.Name},{data.Department}";
                    await writer.WriteLineAsync(line);
                }
            }
        }

        public async Task GetData_Using_List()
        {
            var fileInfo = new FileInfo("DataAsList.csv");

            // Create the Excel file if it doesn't exist
            if (fileInfo.Exists)
            {
                fileInfo.Delete();  // Ensure file is removed before writing
            }

            using (var writer = new StreamWriter(fileInfo.FullName))
            {
                // Write the header
                await writer.WriteLineAsync("EmployeeID,Name,Department");

                // Start writing from the second row

                //var stopwatch = Stopwatch.StartNew();
                var allData = _dataModelContext.Data.AsNoTracking().ToList();
                for (int i = 0; i < allData.Count; i++)
                {
                    var line = $"{allData[i].EmployeeID},{allData[i].Name},{allData[i].Department}";
                    await writer.WriteLineAsync(line);
                }
            }
        }

        public async Task GetData_Using_ListAsync()
        {
            var fileInfo = new FileInfo("DataAsListAsync.csv");

            // Create the Excel file if it doesn't exist
            if (fileInfo.Exists)
            {
                fileInfo.Delete();  // Ensure file is removed before writing
            }

            using (var writer = new StreamWriter(fileInfo.FullName))
            {
                // Write the header
                await writer.WriteLineAsync("EmployeeID,Name,Department");

                var allData = await _dataModelContext.Data.AsNoTracking().ToListAsync().ConfigureAwait(false);
                for (int i = 0; i < allData.Count; i++)
                {
                    var line = $"{allData[i].EmployeeID},{allData[i].Name},{allData[i].Department}";
                    await writer.WriteLineAsync(line);
                }
            }
        }

        public async Task GetData_Using_Dapper()
        {
            var fileInfo = new FileInfo("DataNormalDapper.csv");

            // Create the Excel file if it doesn't exist
            if (fileInfo.Exists)
            {
                fileInfo.Delete();  // Ensure file is removed before writing
            }


            using (var writer = new StreamWriter(fileInfo.FullName))
            {
                // Write the header
                await writer.WriteLineAsync("EmployeeID,Name,Department");


                using (var connection = new SqlConnection(_configuration["Data:DataConnection:ConnectionString"]))
                {
                    connection.Open();

                    string query = "SELECT * FROM Data";

                    // Query the data and map to the Inspection class
                    IEnumerable<MyData> allData = connection.Query<MyData>(query);

                    foreach (var data in allData)
                    {
                        var line = $"{data.EmployeeID},{data.Name},{data.Department}";
                        await writer.WriteLineAsync(line);
                    }
                }
            }
        }

        public async Task GetData_Using_DapperAsync()
        {
            var fileInfo = new FileInfo("DataNormalDapperAsync.csv");

            // Create the Excel file if it doesn't exist
            if (fileInfo.Exists)
            {
                fileInfo.Delete();  // Ensure file is removed before writing
            }

            using (var writer = new StreamWriter(fileInfo.FullName))
            {
                // Write the header
                await writer.WriteLineAsync("EmployeeID,Name,Department");

                // Start writing from the second row

                using (var connection = new SqlConnection(_configuration["Data:DataConnection:ConnectionString"]))
                {
                    connection.Open();

                    string query = "SELECT * FROM Data";

                    // Query the data and map to the Inspection class
                    IEnumerable<MyData> allData = await connection.QueryAsync<MyData>(query).ConfigureAwait(false);

                    foreach (var data in allData)
                    {
                        var line = $"{data.EmployeeID},{data.Name},{data.Department}";
                        await writer.WriteLineAsync(line);
                    }
                }

            }
        }

        public async Task GetData_Using_DapperList()
        {
            var fileInfo = new FileInfo("DataNormalDapperList.csv");

            // Create the Excel file if it doesn't exist
            if (fileInfo.Exists)
            {
                fileInfo.Delete();  // Ensure file is removed before writing
            }

            using (var writer = new StreamWriter(fileInfo.FullName))
            {
                // Write the header
                await writer.WriteLineAsync("EmployeeID,Name,Department");

                // Start writing from the second row

                using (var connection = new SqlConnection(_configuration["Data:DataConnection:ConnectionString"]))
                {
                    connection.Open();

                    string query = "SELECT * FROM Data";

                    // Query the data and map to the Inspection class
                    List<MyData> allData = connection.Query<MyData>(query).ToList();

                    for (int i = 0; i < allData.Count; i++)
                    {
                        var line = $"{allData[i].EmployeeID},{allData[i].Name},{allData[i].Department}";
                        await writer.WriteLineAsync(line);
                    }
                }
            }
        }


        public async Task GetData_Using_AsyncEnumerable_With_Out_ConfigureAwait()
        {
            var fileInfo = new FileInfo("DataAsyncEnumerable.csv");

            if (fileInfo.Exists)
            {
                fileInfo.Delete();  // Ensure file is removed before writing
            }

            using (var writer = new StreamWriter(fileInfo.FullName))
            {
                // Write the header
                await writer.WriteLineAsync("EmployeeID,Name,Department");


                await foreach (var data in _dataModelContext.Data.FromSqlRaw("Select * From data").AsNoTracking().AsAsyncEnumerable().ConfigureAwait(false))
                {
                    var line = $"{data.EmployeeID},{data.Name},{data.Department}";
                    await writer.WriteLineAsync(line);
                }
            }
        }

        public async Task GetData_Using_ListAsync_With_Out_ConfigureAwait()
        {
            var fileInfo = new FileInfo("DataAsListAsync.csv");

            // Create the Excel file if it doesn't exist
            if (fileInfo.Exists)
            {
                fileInfo.Delete();  // Ensure file is removed before writing
            }

            using (var writer = new StreamWriter(fileInfo.FullName))
            {
                // Write the header
                await writer.WriteLineAsync("EmployeeID,Name,Department");

                var allData = await _dataModelContext.Data.AsNoTracking().ToListAsync().ConfigureAwait(false);
                for (int i = 0; i < allData.Count; i++)
                {
                    var line = $"{allData[i].EmployeeID},{allData[i].Name},{allData[i].Department}";
                    await writer.WriteLineAsync(line);
                }
            }
        }

        public async Task GetData_Using_DapperAsync_With_Out_ConfigureAwait()
        {
            var fileInfo = new FileInfo("DataNormalDapperAsync.csv");

            // Create the Excel file if it doesn't exist
            if (fileInfo.Exists)
            {
                fileInfo.Delete();  // Ensure file is removed before writing
            }

            using (var writer = new StreamWriter(fileInfo.FullName))
            {
                // Write the header
                await writer.WriteLineAsync("EmployeeID,Name,Department");

                // Start writing from the second row

                using (var connection = new SqlConnection(_configuration["Data:DataConnection:ConnectionString"]))
                {
                    connection.Open();

                    string query = "SELECT * FROM Data";

                    // Query the data and map to the Inspection class
                    IEnumerable<MyData> allData = await connection.QueryAsync<MyData>(query);

                    foreach (var data in allData)
                    {
                        var line = $"{data.EmployeeID},{data.Name},{data.Department}";
                        await writer.WriteLineAsync(line);
                    }
                }

            }
        }

        public static async Task MeasureMemoryUsageAsync(string methodName, Func<Task> method)
        {
            // Perform garbage collection to ensure that we're working with a clean slate
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Record the initial memory usage
            long initialMemory = GC.GetTotalMemory(true);

            // Execute the method
            var stopwatch = Stopwatch.StartNew();
            await method();
            stopwatch.Stop();

            // Record the memory after execution
            long finalMemory = GC.GetTotalMemory(false);

            // Calculate the maximum memory used
            long maxMemoryUsed = finalMemory - initialMemory;

            Console.WriteLine($"{methodName}: Time = {stopwatch.Elapsed}, Max Memory Used = {maxMemoryUsed / 1024.0 / 1024.0:F2} MB");
        }

    }
}
