using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadLargeData.Models
{
    public class DataModelContext : DbContext
    {
        public DataModelContext(DbContextOptions<DataModelContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        public DbSet<MyData> Data { get; set; }

    }

    public class MyData
    {
        [Key]
        public Guid EmployeeID { get; set; }
        public string? Name { get; set; }
        public string? Department { get; set; }
    }
}
