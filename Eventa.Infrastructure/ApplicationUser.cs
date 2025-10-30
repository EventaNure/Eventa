﻿using System.ComponentModel.DataAnnotations;
using Eventa.Domain;
using Microsoft.AspNetCore.Identity;

namespace Eventa.Infrastructure
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(32)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(6)]
        public string? VerificationCode { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Organization { get; set; }

        public DateTime TicketsExpireAt { get; set; }

        public ICollection<Cart> Carts { get; set; } = [];

        public ICollection<Order> Orders { get; set; } = [];
    }
}