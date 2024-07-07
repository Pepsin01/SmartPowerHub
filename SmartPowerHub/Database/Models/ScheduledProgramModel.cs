using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPowerHub.Database.Models
{
    [Table("ScheduledPrograms")]
    public record ScheduledProgramModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int DeviceId { get; set; }
        [Required]
        public string ProgramName { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
    }
}
