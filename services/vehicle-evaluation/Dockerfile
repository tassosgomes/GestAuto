# Multi-stage Dockerfile for GestAuto Vehicle Evaluation Service
FROM maven:3.9.4-eclipse-temurin-21-alpine AS builder

WORKDIR /app

# Copy Maven wrapper and pom files
COPY mvnw .
COPY .mvn .mvn
COPY pom.xml .
COPY domain/pom.xml domain/
COPY application/pom.xml application/
Copy infra/pom.xml infra/
COPY api/pom.xml api/

# Download dependencies
RUN ./mvnw dependency:go-offline -B

# Copy source code
COPY domain/src ./domain/src
COPY application/src ./application/src
COPY infra/src ./infra/src
COPY api/src ./api/src

# Build the application
RUN ./mvnw clean package -DskipTests -Dspring-boot.build-image.skip=true

# Runtime stage
FROM eclipse-temurin:21-jre-alpine

# Install necessary packages
RUN apk add --no-cache tzdata curl

# Set timezone
ENV TZ=America/Sao_Paulo
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# Create application user
RUN addgroup -g 1001 appgroup && adduser -u 1001 -G appgroup -s /bin/sh -D appuser

WORKDIR /app

# Copy the built JAR from builder stage
COPY --from=builder /app/api/target/vehicle-evaluation-api-*.jar app.jar

# Create logs directory
RUN mkdir -p /var/log/vehicle-evaluation && chown -R appuser:appgroup /var/log/vehicle-evaluation

# Change ownership of app directory
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8081

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8081/api/actuator/health || exit 1

# JVM options
ENV JAVA_OPTS="-Xms512m -Xmx1g -XX:+UseG1GC -XX:+UseContainerSupport -XX:MaxRAMPercentage=75.0"

# Run the application
ENTRYPOINT ["sh", "-c", "java $JAVA_OPTS -jar app.jar"]