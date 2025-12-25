#!/bin/bash
# Isolated Mosaic CMS Deployment Script

set -e

echo "=========================================="
echo "Isolated Mosaic CMS Deployment"
echo "=========================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo -e "${RED}Error: Docker is not installed${NC}"
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    echo -e "${RED}Error: Docker Compose is not installed${NC}"
    exit 1
fi

# Function to build the images
build() {
    echo -e "${YELLOW}Building Docker images...${NC}"
    docker-compose build --no-cache
    echo -e "${GREEN}Build completed successfully!${NC}"
}

# Function to start the services
start() {
    echo -e "${YELLOW}Starting Mosaic CMS services...${NC}"
    docker-compose up -d
    echo ""
    echo -e "${GREEN}Services started successfully!${NC}"
    echo ""
    echo "Mosaic CMS is now running:"
    echo "  - HTTP:  http://localhost:8080"
    echo "  - HTTPS: https://localhost:8081"
    echo ""
    echo "To view logs, run: docker-compose logs -f"
}

# Function to stop the services
stop() {
    echo -e "${YELLOW}Stopping Mosaic CMS services...${NC}"
    docker-compose down
    echo -e "${GREEN}Services stopped successfully!${NC}"
}

# Function to show logs
logs() {
    docker-compose logs -f mosaic-cms
}

# Function to check status
status() {
    echo -e "${YELLOW}Checking service status...${NC}"
    docker-compose ps
}

# Function to clean up
clean() {
    echo -e "${YELLOW}Cleaning up containers, images, and volumes...${NC}"
    docker-compose down -v --rmi all
    echo -e "${GREEN}Cleanup completed!${NC}"
}

# Function to restart services
restart() {
    stop
    sleep 2
    start
}

# Main script logic
case "${1:-}" in
    build)
        build
        ;;
    start)
        start
        ;;
    stop)
        stop
        ;;
    restart)
        restart
        ;;
    logs)
        logs
        ;;
    status)
        status
        ;;
    clean)
        clean
        ;;
    deploy)
        build
        start
        ;;
    *)
        echo "Isolated Mosaic CMS Deployment Script"
        echo ""
        echo "Usage: $0 {build|start|stop|restart|logs|status|clean|deploy}"
        echo ""
        echo "Commands:"
        echo "  build   - Build Docker images"
        echo "  start   - Start services"
        echo "  stop    - Stop services"
        echo "  restart - Restart services"
        echo "  logs    - Show logs"
        echo "  status  - Show service status"
        echo "  clean   - Remove all containers, images, and volumes"
        echo "  deploy  - Build and start (full deployment)"
        echo ""
        exit 1
        ;;
esac
