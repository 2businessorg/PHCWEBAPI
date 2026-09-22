namespace Shared.Abstractions.Privacy.Pseudonymization;

/// <summary>
/// Entity type catalog (architecture section 5). Skills are NOT tokenized by default.
/// </summary>
public static class EntityTypes
{
    // Classic PII
    public const string Person = "PERSON";
    public const string EmailAddress = "EMAIL_ADDRESS";
    public const string PhoneNumber = "PHONE_NUMBER";
    public const string IbanCode = "IBAN_CODE";
    public const string CreditCard = "CREDIT_CARD";
    public const string IpAddress = "IP_ADDRESS";
    public const string Location = "LOCATION";
    public const string DateTime = "DATE_TIME";
    public const string Nif = "NIF";
    public const string Niss = "NISS";
    public const string Url = "URL";
    public const string Username = "USERNAME";

    // Professional / organizational
    public const string Company = "COMPANY";
    public const string Employer = "EMPLOYER";
    public const string Client = "CLIENT";
    public const string Customer = "CUSTOMER";
    public const string Partner = "PARTNER";
    public const string Organization = "ORGANIZATION";
    public const string Project = "PROJECT";
    public const string Product = "PRODUCT";
    public const string Department = "DEPARTMENT";
    public const string Team = "TEAM";
    public const string University = "UNIVERSITY";
    public const string School = "SCHOOL";
    public const string CertificationId = "CERTIFICATION_ID";
    public const string InternalSystem = "INTERNAL_SYSTEM";
    public const string Contract = "CONTRACT";
    public const string InternalSolution = "INTERNAL_SOLUTION";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Person, EmailAddress, PhoneNumber, IbanCode, CreditCard, IpAddress, Location, DateTime,
        Nif, Niss, Url, Username,
        Company, Employer, Client, Customer, Partner, Organization, Project, Product,
        Department, Team, University, School, CertificationId, InternalSystem, Contract, InternalSolution
    };

    /// <summary>Education types kept plain when PseudonymizeEducation=false (default).</summary>
    public static readonly IReadOnlySet<string> EducationTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        University, School
    };
}
