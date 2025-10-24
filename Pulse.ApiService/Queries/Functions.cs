using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

public class Functions
{
    public async Task<List<Dictionary<string, object>>> ExecuteAiQry(string connectionString, string query)
    {

        await Task.Delay(100);
        var results = new List<Dictionary<string, object>>();

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        results.Add(row);
                    }
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            //var errorRow = new Dictionary<string, object>
            //{
            //    { "Error", ex.Message }
            //};
            //results.Add(errorRow);
            //return results;
            //throw new Exception(ex.Message);
            return null;
        }
    }
    public async Task<int> ExecuteAiUpdateInsertQry(string connectionString, string query)
    {

        await Task.Delay(100);
        var results = 0;

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    results = command.ExecuteNonQuery();
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            return results;
        }
    }
}
