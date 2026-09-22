namespace Shared.Kernel.Responses;

/// <summary>
/// Códigos de resposta padrão da API
/// </summary>
public static class ResponseCodes
{
    // Success
    public static readonly ResponseCodeDTO Success = new("0000", "Success");

    // HTTP Errors
    public static readonly ResponseCodeDTO IncorrectHttpMethod = new("0001", "Incorrect HTTP method");
    public static readonly ResponseCodeDTO InvalidJson = new("0002", "Invalid JSON");

    // Authentication/Authorization
    public static readonly ResponseCodeDTO IncorrectApiKey = new("0003", "Incorrect API Key");
    public static readonly ResponseCodeDTO ApiKeyNotProvided = new("0004", "API Key not provided");
    public static readonly ResponseCodeDTO Unauthorized = new("0013", "Unauthorized");

    // Validation/Business Logic
    public static readonly ResponseCodeDTO InvalidReference = new("0005", "Invalid Reference");
    public static readonly ResponseCodeDTO DuplicatedPayment = new("0006", "Duplicated payment");
    public static readonly ResponseCodeDTO InvalidAmount = new("0008", "Invalid Amount");
    public static readonly ResponseCodeDTO RequestIdNotProvided = new("0009", "Request ID not provided");
    public static readonly ResponseCodeDTO NotFound = new("0014", "Resource not found");
    public static readonly ResponseCodeDTO ValidationError = new("0015", "Validation error");
    public static readonly ResponseCodeDTO AlreadyExists = new("0016", "Resource already exists");

    // Internal Errors
    public static readonly ResponseCodeDTO InternalError = new("0007", "Internal error");
    public static readonly ResponseCodeDTO DatabaseError = new("0017", "Database error");

    // User Management
    public static readonly ResponseCodeDTO UserAlreadyExists = new("0010", "User already exists");
    public static readonly ResponseCodeDTO UserCreationFailed = new("0011", "User creation failed");
    public static readonly ResponseCodeDTO UserNotFound = new("0012", "User not found");

    // Parameters Module (custom)
    public static class Parameter
    {
        public static readonly ResponseCodeDTO NotFound = new("1001", "Parameter not found");
        public static readonly ResponseCodeDTO CodeAlreadyExists = new("1002", "Parameter code already exists");
        public static readonly ResponseCodeDTO InvalidCode = new("1003", "Invalid parameter code");
        public static readonly ResponseCodeDTO InactiveCannotUpdate = new("1004", "Cannot update inactive parameter");
        public static readonly ResponseCodeDTO CreatedSuccessfully = new("1005", "Parameter created successfully");
        public static readonly ResponseCodeDTO UpdatedSuccessfully = new("1006", "Parameter updated successfully");
        public static readonly ResponseCodeDTO DeletedSuccessfully = new("1007", "Parameter deleted successfully");
    }

    // Providers Module
    public static class Provider
    {
        public static readonly ResponseCodeDTO NotFound = new("1101", "Provider not found");
        public static readonly ResponseCodeDTO ConfigNotFound = new("1102", "Provider configuration not found");
        public static readonly ResponseCodeDTO AlreadyExists = new("1103", "Provider already exists for this environment");
        public static readonly ResponseCodeDTO CreatedSuccessfully = new("1104", "Provider created successfully");
        public static readonly ResponseCodeDTO UpdatedSuccessfully = new("1105", "Provider updated successfully");
        public static readonly ResponseCodeDTO DeletedSuccessfully = new("1106", "Provider deleted successfully");
    }

    // Customers Module (futuro)
    public static class Customer
    {
        public static readonly ResponseCodeDTO NotFound = new("2001", "Customer not found");
        public static readonly ResponseCodeDTO EmailAlreadyExists = new("2002", "Customer email already exists");
        // ... mais códigos
    }

    // Orders Module (futuro)
    public static class Order
    {
        public static readonly ResponseCodeDTO NotFound = new("3001", "Order not found");
        public static readonly ResponseCodeDTO InvalidStatus = new("3002", "Invalid order status");
        // ... mais códigos
    }
}

