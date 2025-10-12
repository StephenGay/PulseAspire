using Pulse.Models.CustomComponents;
using System.Text;

namespace Pulse.Web.Tools
{
    public static class SchemaModifier
    {
        private static readonly Dictionary<string, string> TypeMap = new()
        {
            { "System.Int32", "int" },
            { "System.String", "nvarchar" }, // Adjust length in SQL generation
            { "System.Decimal", "decimal(18,2)" },
            { "System.Boolean", "bit" },
            { "System.DateTime", "datetime2" }
            // Add more mappings as needed (e.g., System.Int64 → bigint)
        };
        private static string GenerateCreateTableScript(List<SchemaDto> schemas)
        {
            var sb = new StringBuilder();
            var foreignKeys = new List<string>();

            foreach (var schema in schemas)
            {
                if (string.IsNullOrEmpty(schema.TableName) || schema.Columns == null)
                    continue;

                // Start CREATE TABLE
                sb.AppendLine($"CREATE TABLE [{schema.TableName}] (");

                // Columns
                foreach (var col in schema.Columns)
                {
                    // Map .NET type to SQL Server type
                    string sqlType = TypeMap.TryGetValue(col.DataType, out var mappedType) ? mappedType : col.DataType;

                    // Adjust nvarchar length based on schema (e.g., nvarchar(10), nvarchar(255), or nvarchar(max))
                    if (sqlType == "nvarchar" && col.DataType == "System.String")
                    {
                        // Extract length from schema if provided (e.g., "nvarchar(10)" in original JSON)
                        // For simplicity, use a default length or max based on column name or context
                        sqlType = col.Name.Contains("max", StringComparison.OrdinalIgnoreCase) ? "nvarchar(max)" :
                                  col.Name.Contains("ID", StringComparison.OrdinalIgnoreCase) ? "nvarchar(50)" : "nvarchar(255)";
                    }

                    var columnDef = $"    [{col.Name}] {sqlType}{(col.IsNullable ? "" : " NOT NULL")}";
                    sb.AppendLine($"{columnDef},");
                }

                // Primary Key
                if (schema.PrimaryKeys != null && schema.PrimaryKeys.Count > 0)
                {
                    var pkColumns = string.Join(", ", schema.PrimaryKeys.Select(pk => $"[{pk}]"));
                    sb.AppendLine($"    CONSTRAINT [PK_{schema.TableName}] PRIMARY KEY ({pkColumns}),");
                }

                // Remove trailing comma
                if (sb[sb.Length - 1] == ',')
                    sb.Length -= 3; // Trim ", \n"

                sb.AppendLine();
                sb.AppendLine(");");

                // Collect Foreign Keys
                if (schema.Relationships != null)
                {
                    foreach (var rel in schema.Relationships)
                    {
                        foreach (var fkCol in rel.ForeignKeyColumns)
                        {
                            var fkConstraint = $"ALTER TABLE [{schema.TableName}] ADD CONSTRAINT [FK_{schema.TableName}_{rel.RelatedTableName}_{fkCol}] " +
                                               $"FOREIGN KEY ([{fkCol}]) REFERENCES [{rel.RelatedTableName}] ([{fkCol}]);";
                            foreignKeys.Add(fkConstraint);
                        }
                    }
                }

                sb.AppendLine();
            }

            // Append Foreign Key Constraints
            if (foreignKeys.Count > 0)
            {
                sb.AppendLine("-- Foreign Key Constraints");
                foreach (var fk in foreignKeys)
                {
                    sb.AppendLine(fk);
                }
            }

            return sb.ToString();
        }
    }
}

