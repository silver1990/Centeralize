using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Raybod.SCM.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raybod.SCM.DataAccess.Audit_log
{

    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public string TableName { get; set; }
        public int? UserId { get; set; }
        public string Username { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string AuditType { get; set; }
        public string KeyValues { get; set; }
        public string ForeignKey { get; set; }
        public Dictionary<string, object> OldValues { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; set; } = new Dictionary<string, object>();
        public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        //public Audit ToAudit()
        //{
        //    var audit = new Audit();
        //    audit.UserId = UserId;
        //    audit.Controller = Controller;
        //    audit.Action = Action;
        //    audit.AuditType = AuditType;
        //    audit.TableName = TableName;
        //    audit.Username = Username;
        //    audit.DateTime = DateTime.UtcNow;
        //    audit.KeyValues = KeyValues;
        //    audit.ForeignKey = ForeignKey;
        //    audit.OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues);
        //    audit.NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues);
        //    return audit;
        //}
    }

}
