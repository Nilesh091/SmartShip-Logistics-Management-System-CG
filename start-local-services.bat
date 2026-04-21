@echo off
REM SmartShip Local Environment - Quick Start Script (Windows)
REM This script starts all services locally without Docker

setlocal enabledelayedexpansion

cls
echo ========================================
echo SmartShip Local Environment - Quick Start
echo ========================================
echo.

REM Check prerequisites
echo Checking prerequisites...

where dotnet >nul 2>nul
if errorlevel 1 (
    echo [ERROR] .NET SDK not found. Please install .NET 10.0 SDK
    pause
    exit /b 1
)
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo [OK] .NET SDK found: %DOTNET_VERSION%

echo.
echo Available services:
echo   1. AuthService (http://localhost:5001)
echo   2. ShipmentService (http://localhost:5003)
echo   3. TrackingService (http://localhost:5004)
echo   4. AdminService (http://localhost:5002)
echo   5. Gateway (http://localhost:5166)
echo   all - Start all services
echo.

set /p SERVICE_CHOICE="Enter choice [all]: "
if "%SERVICE_CHOICE%"=="" set SERVICE_CHOICE=all

if "%SERVICE_CHOICE%"=="all" (
    set SERVICES=AuthService ShipmentService TrackingService AdminService Gateway
    set PATHS=Services\AuthService\AuthService.API Services\ShipmentService\ShipmentService.API Services\TrackingService\TrackingService.API Services\AdminService\AdminService.API Gateway\Gateway.API
    set PORTS=5001 5003 5004 5002 5166
)

echo.
echo Starting services...
echo.

REM Create a temp file to store process info
set PROCESS_FILE=%temp%\smartship_processes.txt
if exist %PROCESS_FILE% del %PROCESS_FILE%

if "%SERVICE_CHOICE%"=="all" (
    REM Start each service in a new window
    echo Starting AuthService...
    start "SmartShip - Auth Service" cmd /k "cd Services\AuthService\AuthService.API && dotnet run --configuration Development"
    timeout /t 2 /nobreak
    
    echo Starting ShipmentService...
    start "SmartShip - Shipment Service" cmd /k "cd Services\ShipmentService\ShipmentService.API && dotnet run --configuration Development"
    timeout /t 2 /nobreak
    
    echo Starting TrackingService...
    start "SmartShip - Tracking Service" cmd /k "cd Services\TrackingService\TrackingService.API && dotnet run --configuration Development"
    timeout /t 2 /nobreak
    
    echo Starting AdminService...
    start "SmartShip - Admin Service" cmd /k "cd Services\AdminService\AdminService.API && dotnet run --configuration Development"
    timeout /t 2 /nobreak
    
    echo Starting Gateway...
    start "SmartShip - Gateway" cmd /k "cd Gateway\Gateway.API && dotnet run --configuration Development"
) else (
    REM Handle single service selection
    if "%SERVICE_CHOICE%"=="1" (
        start "SmartShip - Auth Service" cmd /k "cd Services\AuthService\AuthService.API && dotnet run --configuration Development"
    ) else if "%SERVICE_CHOICE%"=="2" (
        start "SmartShip - Shipment Service" cmd /k "cd Services\ShipmentService\ShipmentService.API && dotnet run --configuration Development"
    ) else if "%SERVICE_CHOICE%"=="3" (
        start "SmartShip - Tracking Service" cmd /k "cd Services\TrackingService\TrackingService.API && dotnet run --configuration Development"
    ) else if "%SERVICE_CHOICE%"=="4" (
        start "SmartShip - Admin Service" cmd /k "cd Services\AdminService\AdminService.API && dotnet run --configuration Development"
    ) else if "%SERVICE_CHOICE%"=="5" (
        start "SmartShip - Gateway" cmd /k "cd Gateway\Gateway.API && dotnet run --configuration Development"
    ) else (
        echo Invalid choice
        pause
        exit /b 1
    )
)

cls
echo ========================================
echo Services are starting in separate windows
echo ========================================
echo.
echo Service URLs:
echo   - Gateway: http://localhost:5166
echo   - Auth Service: http://localhost:5001
echo   - Shipment Service: http://localhost:5003
echo   - Tracking Service: http://localhost:5004
echo   - Admin Service: http://localhost:5002
echo.
echo RabbitMQ Management UI: http://localhost:15672
echo.
echo Close the individual service windows to stop them.
echo.
pause
