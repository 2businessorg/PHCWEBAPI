# 🔍 Test Login Endpoint Audit Logging
# Tests if the login endpoint is generating audit logs

$baseUrl = "https://localhost:7001"
$loginEndpoint = "$baseUrl/api/authenticate/login"

Write-Host "🧪 TESTING LOGIN ENDPOINT AUDIT LOGGING" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host ""

# Test payload
$payload = @{
    username = "admin"
    password = "YOUR_PASSWORD"
} | ConvertTo-Json

Write-Host "📤 Sending login request..." -ForegroundColor Yellow
Write-Host "   Endpoint: $loginEndpoint" -ForegroundColor Gray
Write-Host "   Payload: $payload" -ForegroundColor Gray
Write-Host ""

try {
    $response = Invoke-WebRequest `
        -Uri $loginEndpoint `
        -Method POST `
        -Body $payload `
        -ContentType "application/json" `
        -SkipCertificateCheck `
        -ErrorAction Stop
    
    Write-Host "✅ Response received:" -ForegroundColor Green
    Write-Host "   Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "   Content-Type: $($response.Headers['Content-Type'])" -ForegroundColor Gray
    Write-Host "   Body:" -ForegroundColor Gray
    Write-Host "   $($response.Content)" -ForegroundColor Gray
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "⚠️  Response with status $statusCode" -ForegroundColor Yellow
    
    if ($statusCode -eq 401) {
        Write-Host "   Expected: Invalid credentials" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "🔍 CHECK THE FOLLOWING:" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Application Logs:" -ForegroundColor White
Write-Host "   Look for these log entries:" -ForegroundColor Gray
Write-Host "   [MIDDLEWARE START] POST /api/authenticate/login" -ForegroundColor DarkGray
Write-Host "   [AUDIT API] Path: /api/authenticate/login" -ForegroundColor DarkGray
Write-Host "   [AUDIT START] Logging audit for POST /api/authenticate/login" -ForegroundColor DarkGray
Write-Host "   [AUDIT CALL] Calling LogResponseAsync" -ForegroundColor DarkGray
Write-Host "   [AUDIT SUCCESS] Audit log saved" -ForegroundColor DarkGray
Write-Host "   [AUDIT SERVICE] Starting audit log for operation" -ForegroundColor DarkGray
Write-Host "   [AUDIT SERVICE] Hangfire job enqueued successfully" -ForegroundColor DarkGray
Write-Host "   [MIDDLEWARE END] POST /api/authenticate/login" -ForegroundColor DarkGray
Write-Host ""

Write-Host "2. Hangfire Dashboard:" -ForegroundColor White
Write-Host "   URL: $baseUrl/hangfire" -ForegroundColor Gray
Write-Host "   Check for SaveAuditLogJob in the queue/succeeded jobs" -ForegroundColor Gray
Write-Host ""

Write-Host "3. Database (AuditLogs table):" -ForegroundColor White
Write-Host "   Run this SQL query:" -ForegroundColor Gray
Write-Host @"
   SELECT TOP 10 
       Id,
       Code,
       RequestId,
       ResponseDesc,
       Operation,
       Ip,
       CreatedAt
   FROM AuditLogs
   WHERE Operation LIKE '%authenticate/login%'
   ORDER BY CreatedAt DESC
"@ -ForegroundColor DarkGray
Write-Host ""

Write-Host "4. If no logs appear, check:" -ForegroundColor White
Write-Host "   ✓ Is IAuditLogService registered in DI?" -ForegroundColor Gray
Write-Host "   ✓ Is Hangfire running?" -ForegroundColor Gray
Write-Host "   ✓ Is IBackgroundJobClient available?" -ForegroundColor Gray
Write-Host "   ✓ Are there any errors in Hangfire dashboard?" -ForegroundColor Gray
Write-Host ""

Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host "🏁 Test completed at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
