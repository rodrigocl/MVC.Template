﻿using MvcTemplate.Objects;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace MvcTemplate.Data.Logging
{
    public class LoggableEntity
    {
        public String Id
        {
            get;
            private set;
        }
        public String Name
        {
            get;
            private set;
        }
        public String Action
        {
            get;
            private set;
        }
        public Boolean HasChanges
        {
            get;
            private set;
        }

        public IEnumerable<LoggableProperty> Properties
        {
            get;
            private set;
        }

        public LoggableEntity(DbEntityEntry<BaseModel> entry)
        {
            DbPropertyValues originalValues =
                (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                    ? entry.GetDatabaseValues()
                    : entry.CurrentValues;

            Type entityType = entry.Entity.GetType();
            if (entityType.Namespace == "System.Data.Entity.DynamicProxies") entityType = entityType.BaseType;
            Properties = originalValues.PropertyNames.Select(name => new LoggableProperty(entry.Property(name), originalValues[name]));
            HasChanges = Properties.Any(property => property.IsModified);
            Action = entry.State.ToString().ToLower();
            Name = entityType.Name;
            Id = entry.Entity.Id;
        }

        public override String ToString()
        {
            StringBuilder entry = new StringBuilder();
            entry.AppendFormat("{0} {1}:{2}", Name, Action, Environment.NewLine);

            foreach (LoggableProperty property in Properties)
                entry.AppendFormat("    {0}{1}", property, Environment.NewLine);

            return entry.ToString();
        }
    }
}
