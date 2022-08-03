using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class FileDriveDirectory:BaseEntity
    {
        [Key]
        public Guid DirectoryId { get; set; }
        public Guid? ParentId { get; set; }
        [Required]
        [MaxLength(60)]
        public string DirectoryName { get; set; }
        [Required]
        [MaxLength(2048)]
        public string DirectoryPath { get; set; }

        public bool PermanentDelete { get; set; }
        public int? UserId { get; set; }

        [ForeignKey("Contract")]
        public string ContractCode { get; set; }

        [ForeignKey(nameof(ParentId))]
        public FileDriveDirectory ParentDirectory { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        public virtual Contract Contract { get; set; }
        public ICollection<FileDriveDirectory> Directories { get; set; }
        public ICollection<FileDriveFile> Files { get; set; }
        public ICollection<FileDriveShare> Shares { get; set; }
    }
}
