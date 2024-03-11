using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Momo.Common.Models.Tables
{
    public class PortalUserTb
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string Username { get; set; }
        [StringLength(100)]
        public string Password { get; set; }
        [StringLength(50)]
        public string Role { get; set; }
        [DefaultValue(false)]
        public bool IsActive { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        [StringLength(50)]
        public string? UserKey { get; set; } //Use to renew password


    }
}
