using BBWM.Core.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BBWM.AggregatedLogs
{
    [Index(nameof(Level))]
    [Index(nameof(TimeStamp))]
    public class Log : IEntity
    {
        public int Id { get; set; }

        [MaxLength(2000)]
        public string Message { get; set; }


        [MaxLength(20)]
        public string Level { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string Exception { get; set; }

        [MaxLength(2000)]
        public string LogEvent { get; set; }

        [MaxLength(100)]
        public string AppName { get; set; }

        [MaxLength(500)]
        public string Server { get; set; }

        [MaxLength(20)]
        public string IP { get; set; }

        [MaxLength(100)]
        public string Source { get; set; }

        [MaxLength(50)]
        public string UserName { get; set; }

        public bool? IsImpersonating { get; set; }

        [MaxLength(50)]
        public string OriginalUserName { get; set; }

        [MaxLength(50)]
        public string ErrorId { get; set; }

        public int? HttpStatus { get; set; }
    }
}
