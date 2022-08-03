using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class FileDriveShare:BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public Guid? DirectoryId { get; set; }
        public Guid? FileId { get; set; }
        public EntityType EntityType { get; set; }
        public Accessablity Accessablity { get; set; }
        public ShareEntityStatus Status { get; set; }
        public int UserId { get; set; }
        [ForeignKey(nameof(DirectoryId))]
        public FileDriveDirectory Directory { get; set; }
        [ForeignKey(nameof(FileId))]
        public FileDriveFile File { get; set; }
    }
}
