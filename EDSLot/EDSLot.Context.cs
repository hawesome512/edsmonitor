﻿//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace EDSLot
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class EDSEntities : DbContext
    {
        public EDSEntities()
            : base("name=EDSEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Energy> Energy { get; set; }
        public DbSet<Record_ACB> Record_ACB { get; set; }
        public DbSet<Record_MCCB> Record_MCCB { get; set; }
        public DbSet<Record_Measure> Record_Measure { get; set; }
        public DbSet<Trip> Trip { get; set; }
    }
}
