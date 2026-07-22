using Domain.Entities;
using Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
namespace Application.DTO
{
    public class CreateBorrowerDTO
    {
        public string? FirstName {get;set;}
        public string? LastName {get;set;}
        public int BorrowerTypeId {get;set;}
          public BorrowerType BorrowerType {get;set;}
        public Sex? sex{get;set;}
        public DateTime? DateOfBirth {get;set;}
        [Required]
        [StringLength(16, ErrorMessage = "IdentificationNumber must not be more than 16 characters.")]
        public string IdentificationNumber{get;set;}
        public string Email {get;set;}
        public string PhoneNumber {get;set;}
        public Maritalstatus? Maritalstatus{get;set;}
        public string? SpouceIdNumber {get;set;}
        public string? SpouceName {get;set;}
        public string? NextOfKin {get;set;}
        public string? KinPhoneNumber{get;set;}
        public string? NextOfKinRelationship {get;set;}
        public string Province {get;set;}
        public string District {get;set;}
        public string Sector {get;set;}
        public string Cell {get;set;}
        public string Village {get;set;}

        // Business specific fields
        public string? CompanyName {get;set;}
        public string? TIN {get;set;}
        public string? ContactPersonName {get;set;}
        public string? ContactPersonPhone {get;set;}
    }
    public class UpdateBorrowerDTO
    {
       public string? FirstName {get;set;}
        public string? LastName {get;set;}
        public int BorrowerTypeId {get;set;}
        public Sex? sex{get;set;}
        public DateTime? DateOfBirth {get;set;}
        public Maritalstatus? Maritalstatus{get;set;}
        public string? SpouceIdNumber {get;set;}
        public string? SpouceName {get;set;}  

        // Business specific fields
        public string? CompanyName {get;set;}
        public string? TIN {get;set;}
        public string? ContactPersonName {get;set;}
        public string? ContactPersonPhone {get;set;}
    }
}