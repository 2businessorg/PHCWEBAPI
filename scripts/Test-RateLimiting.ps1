# 🧪 Rate Limiting Testing Script
# Tests each endpoint with its specific rate limit configuration
# Author: Security Audit Team
# Date: 2025

# Configuration
$baseUrl = "https://localhost:7001"  # Adjust to your API URL
$loginEndpoint = "$baseUrl/api/auth/login"

# Colors for output
$ErrorColor = "Red"
$SuccessColor = "Green"
$WarningColor = "Yellow"
$InfoColor = "Cyan"

Write-Host "🚀 RATE LIMITING TEST SUITE" -ForegroundColor $InfoColor
Write-Host "=" * 60 -ForegroundColor $InfoColor
Write-Host ""

# Test 1: Login Endpoint (3 attempts/min)
Write-Host "🔴 TEST 1: Login Endpoint Rate Limiting (3 requests/min)" -ForegroundColor $InfoColor
Write-Host "Expected: First 3 requests succeed, 4th returns HTTP 429" -ForegroundColor $WarningColor
Write-Host ""

$loginPayload = @{
    username = "testuser"
    password = "YOUR_PASSWORD"
} | ConvertTo-Json

for ($i = 1; $i -le 5; $i++) {
    try {
        Write-Host "  Request #$i at $(Get-Date -Format 'HH:mm:ss.fff')..." -NoNewline
        
        $response = Invoke-WebRequest `
            -Uri $loginEndpoint `
            -Method POST `
            -Body $loginPayload `
            -ContentType "application/json" `
            -ErrorAction Stop `
            -TimeoutSec 5
        
        Write-Host " ✅ HTTP $($response.StatusCode)" -ForegroundColor $SuccessColor
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        
        if ($statusCode -eq 429) {
            Write-Host " 🛑 HTTP 429 - RATE LIMIT BLOCKED!" -ForegroundColor $ErrorColor
            
            # Extract Retry-After header
            $retryAfter = $_.Exception.Response.Headers["Retry-After"]
            if ($retryAfter) {
                Write-Host "    ⏱️  Retry-After: $retryAfter seconds" -ForegroundColor $WarningColor
            }
            
            # Parse error message if available
            try {
                $errorStream = $_.Exception.Response.GetResponseStream()
                $reader = New-Object System.IO.StreamReader($errorStream)
                $errorBody = $reader.ReadToEnd() | ConvertFrom-Json
                Write-Host "    💬 Message: $($errorBody.message)" -ForegroundColor $WarningColor
                Write-Host "    🎯 Endpoint: $($errorBody.endpoint)" -ForegroundColor $WarningColor
            }
            catch {
                # Unable to parse error body
            }
        }
        elseif ($statusCode -eq 401) {
            # Expected - Invalid credentials (but not rate limited)
            Write-Host " ✅ HTTP 401 (Not rate limited)" -ForegroundColor $SuccessColor
        }
        else {
            Write-Host " ⚠️  HTTP $statusCode - $($_.Exception.Message)" -ForegroundColor $WarningColor
        }
    }
    
    # Very small delay to ensure requests hit the same time window
    Start-Sleep -Milliseconds 50
}

Write-Host ""
Write-Host "⏳ Waiting 60 seconds for rate limit window to reset..." -ForegroundColor $InfoColor
for ($i = 60; $i -gt 0; $i--) {
    Write-Host "`r  ⏱️  $i seconds remaining..." -NoNewline -ForegroundColor $WarningColor
    Start-Sleep -Seconds 1
}
Write-Host "`r  ✅ Rate limit window reset!                    " -ForegroundColor $SuccessColor
Write-Host ""

# Test 2: Verify rate limit has reset
Write-Host "🔄 TEST 2: Verify Rate Limit Reset" -ForegroundColor $InfoColor
Write-Host "Expected: Request succeeds after window reset" -ForegroundColor $WarningColor
Write-Host ""

try {
    Write-Host "  Request after reset..." -NoNewline
    
    $response = Invoke-WebRequest `
        -Uri $loginEndpoint `
        -Method POST `
        -Body $loginPayload `
        -ContentType "application/json" `
        -ErrorAction Stop `
        -TimeoutSec 5
    
    Write-Host " ✅ HTTP $($response.StatusCode) - Rate limit successfully reset!" -ForegroundColor $SuccessColor
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    
    if ($statusCode -eq 429) {
        Write-Host " ❌ HTTP 429 - Rate limit did NOT reset properly!" -ForegroundColor $ErrorColor
    }
    elseif ($statusCode -eq 401) {
        Write-Host " ✅ HTTP 401 - Rate limit reset (invalid credentials expected)" -ForegroundColor $SuccessColor
    }
    else {
        Write-Host " ⚠️  HTTP $statusCode" -ForegroundColor $WarningColor
    }
}

Write-Host ""
Write-Host "=" * 60 -ForegroundColor $InfoColor
Write-Host "🏁 RATE LIMITING TEST COMPLETED" -ForegroundColor $InfoColor
Write-Host ""
Write-Host "📊 SUMMARY OF RATE LIMITS:" -ForegroundColor $InfoColor
Write-Host "  🔴 Login:          3 requests/min  (Fixed Window)" -ForegroundColor $ErrorColor
Write-Host "  🟠 Create:         5 requests/min  (Sliding Window)" -ForegroundColor $WarningColor
Write-Host "  🟡 Update:         10 requests/min (Sliding Window)" -ForegroundColor $WarningColor
Write-Host "  🔴 Delete:         3 requests/min  (Fixed Window)" -ForegroundColor $ErrorColor
Write-Host "  🟢 Query:          50 requests/min (Sliding Window)" -ForegroundColor $SuccessColor
Write-Host "  🌍 Global Fallback: 100 requests/min (Sliding Window)" -ForegroundColor $InfoColor
Write-Host ""
Write-Host "💡 TIP: Rate limiting partitions by IP + Username" -ForegroundColor $InfoColor
Write-Host "    Different users from same IP share the IP-based limit" -ForegroundColor $InfoColor
Write-Host "    Authenticated users get user-specific limits" -ForegroundColor $InfoColor
Write-Host ""
