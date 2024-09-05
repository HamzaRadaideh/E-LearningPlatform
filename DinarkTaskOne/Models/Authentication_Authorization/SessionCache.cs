using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.Authentication_Authorization
{
    [Table("SessionCache")]
    public class SessionCache
    {
        [Key]
        [StringLength(449)]
        public string Id { get; set; } = string.Empty;

        [Required]
        public byte[] Value { get; set; } = [];

        [Required]
        public DateTimeOffset ExpiresAtTime { get; set; }

        public long? SlidingExpirationInSeconds { get; set; }

        public DateTimeOffset? AbsoluteExpiration { get; set; }
    }
}
