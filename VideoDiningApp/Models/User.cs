using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace VideoDiningApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        [JsonIgnore]
        public string? PasswordResetToken { get; set; }

        [JsonIgnore]
        public DateTime? TokenExpiry { get; set; }

        public bool IsBlocked { get; set; } = false;
        public string? Avatar { get; set; }

        [JsonIgnore]
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<Friendship> FriendshipsAsUser1 { get; set; } = new List<Friendship>();
        public ICollection<Friendship> FriendshipsAsUser2 { get; set; } = new List<Friendship>();

        [NotMapped]
        public List<User> Friends => FriendshipsAsUser1.Select(f => f.User2)
            .Union(FriendshipsAsUser2.Select(f => f.User1))
            .ToList();
    }
}
