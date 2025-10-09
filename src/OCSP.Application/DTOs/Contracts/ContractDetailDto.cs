using System;
using System.Collections.Generic;
using System.Linq;
using OCSP.Application.DTOs.Contracts;

namespace OCSP.Application.DTOs.Contracts
{
    public class ContractDetailDto
    {
        public Guid Id { get; set; }
        public Guid ProposalId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid ContractorUserId { get; set; }
        public Guid HomeownerUserId { get; set; }

        public string Terms { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // List chi tiết item
        public List<ContractItemDto> Items { get; set; } = new();

        // Homeowner information
        public HomeownerInfoDto? Homeowner { get; set; }

        // Contractor information
        public ContractorInfoDto? Contractor { get; set; }
    }

    public class HomeownerInfoDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class ContractorInfoDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public int YearsOfExperience { get; set; }
        public int TeamSize { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int CompletedProjects { get; set; }
        public bool IsVerified { get; set; }
        public bool IsPremium { get; set; }
    }
}
