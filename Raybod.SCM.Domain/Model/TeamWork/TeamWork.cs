using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class TeamWork
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(800)]
        public string Title { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }

        [ForeignKey(nameof(ContractCode))]
        public virtual Contract Contract { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime DateCreat { get; set; }

        public ICollection<TeamWorkUser> TeamWorkUsers { get; set; }

        public  ICollection<UserNotify> UserNotifies { get; set; }

    }
}
