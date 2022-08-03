using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class FileDriveFile:BaseEntity
    {
        [Key]
        public Guid FileId { get; set; }
        [Required]
        [MaxLength(250)]
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public Guid DirectoryId { get; set; }
        public int? UserId { get; set; }
        public bool PermanentDelete { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(DirectoryId))]
        public FileDriveDirectory Directory { get; set; }
        public ICollection<FileDriveShare> Shares { get; set; }
       
    }
}
