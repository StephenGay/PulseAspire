using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.CustomComponents
{
    // DTOs for safe serialization
    public class SchemaDto
    {
        public string EntityType { get; set; } // C# class name, e.g., "ClientMaster"
        public string TableName { get; set; }
        public List<string> PrimaryKeys { get; set; } = new List<string>();
        public List<ColumnDto> Columns { get; set; } = new List<ColumnDto>();
        public List<RelationshipDto> Relationships { get; set; } = new List<RelationshipDto>();
    }

    public class ColumnDto
    {
        public string Name { get; set; }
        public string DataType { get; set; } // SQL type, e.g., "decimal(18,2)"
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
    }

    public class RelationshipDto
    {
        public string NavigationName { get; set; } // e.g., "Rollers" (collection navigation)
        public string RelatedEntityType { get; set; } // e.g., "Roller"
        public string RelatedTableName { get; set; } // e.g., "Rollers"
        public List<string> ForeignKeyColumns { get; set; } = new List<string>(); // e.g., ["ClientID"]
        public string Cardinality { get; set; } // e.g., "OneToMany", "ManyToOne", "OneToOne"
    }
}
