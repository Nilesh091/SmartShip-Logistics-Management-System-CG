#!/bin/bash

# SmartShip Local Environment - Quick Start Script
# This script starts all services locally without Docker

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}========================================${NC}"
echo -e "${YELLOW}SmartShip Local Environment - Quick Start${NC}"
echo -e "${YELLOW}========================================${NC}\n"

# Check prerequisites
echo "Checking prerequisites..."

# Check .NET
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK not found. Please install .NET 10.0 SDK${NC}"
    exit 1
fi
echo -e "${GREEN}✅ .NET SDK found: $(dotnet --version)${NC}"

# Check RabbitMQ (optional - just warn)
if ! command -v rabbitmqctl &> /dev/null; then
    echo -e "${YELLOW}⚠️  RabbitMQ not found in PATH. Ensure RabbitMQ is running on localhost:5672${NC}"
else
    echo -e "${GREEN}✅ RabbitMQ found${NC}"
fi

# Define service directories and ports
declare -A SERVICES=(
    ["AuthService"]="Services/AuthService/AuthService.API:5001"
    ["ShipmentService"]="Services/ShipmentService/ShipmentService.API:5003"
    ["TrackingService"]="Services/TrackingService/TrackingService.API:5004"
    ["AdminService"]="Services/AdminService/AdminService.API:5002"
    ["Gateway"]="Gateway/Gateway.API:5166"
)

echo -e "\n${YELLOW}Available services:${NC}"
for service in "${!SERVICES[@]}"; do
    IFS=':' read -r path port <<< "${SERVICES[$service]}"
    echo "  • $service (http://localhost:$port)"
done

echo -e "\n${YELLOW}Choose services to start (comma-separated) or press Enter for all:${NC}"
echo "  1. AuthService"
echo "  2. ShipmentService"
echo "  3. TrackingService"
echo "  4. AdminService"
echo "  5. Gateway"
echo "  all - Start all services"
echo ""
read -p "Enter choice [all]: " service_choice

if [ -z "$service_choice" ] || [ "$service_choice" = "all" ]; then
    SERVICES_TO_RUN=("${!SERVICES[@]}")
else
    SERVICES_TO_RUN=()
    case $service_choice in
        1) SERVICES_TO_RUN=("AuthService") ;;
        2) SERVICES_TO_RUN=("ShipmentService") ;;
        3) SERVICES_TO_RUN=("TrackingService") ;;
        4) SERVICES_TO_RUN=("AdminService") ;;
        5) SERVICES_TO_RUN=("Gateway") ;;
        *) 
            echo -e "${RED}Invalid choice${NC}"
            exit 1
            ;;
    esac
fi

echo -e "\n${YELLOW}Starting services...${NC}\n"

# Function to start a service
start_service() {
    local service_name=$1
    local service_path=$2
    local port=$3
    
    echo -e "${YELLOW}Starting $service_name on port $port...${NC}"
    
    # Start in background
    (
        cd "$service_path"
        echo "Building $service_name..."
        dotnet build --configuration Development --no-restore > /dev/null 2>&1
        echo "Running $service_name..."
        dotnet run --configuration Development --no-build
    ) &
    
    local PID=$!
    echo -e "${GREEN}✅ $service_name started (PID: $PID)${NC}"
    echo $PID >> /tmp/smartship_services.pids
}

# Clear previous PID file
> /tmp/smartship_services.pids

# Start selected services
for service in "${SERVICES_TO_RUN[@]}"; do
    IFS=':' read -r path port <<< "${SERVICES[$service]}"
    start_service "$service" "$path" "$port"
    echo ""
done

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}All selected services are starting!${NC}"
echo -e "${GREEN}========================================${NC}\n"

echo -e "Service URLs:"
for service in "${SERVICES_TO_RUN[@]}"; do
    IFS=':' read -r path port <<< "${SERVICES[$service]}"
    echo -e "  • ${GREEN}$service: http://localhost:$port${NC}"
done

echo -e "\nRabbitMQ Management UI: ${GREEN}http://localhost:15672${NC}"
echo -e "\nPress Ctrl+C to stop all services\n"

# Wait for all background processes
wait

echo -e "${YELLOW}All services stopped${NC}"
