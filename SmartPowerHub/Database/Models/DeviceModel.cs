﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPowerHub.Database.Models
{
    [Table("Devices")]
    public record DeviceModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ControllerName { get; set; }
        public string Configuration { get; set; }
    }
}
