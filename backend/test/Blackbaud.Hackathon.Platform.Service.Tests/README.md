# Test Project

This project contains unit and integration tests for the Blackbaud Hackathon Platform.

## Test Structure

### Unit Tests
- **EmailServiceTests**: Tests for email notification functionality
- **FileServiceTests**: Tests for file upload/download functionality
- **NotificationServiceTests**: Tests for SignalR real-time notifications

### Integration Tests
- **FilesControllerIntegrationTests**: End-to-end tests for file operations
- **IdeasControllerEmailIntegrationTests**: Tests for email integration in controllers

## Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReporter=html

# Run specific test class
dotnet test --filter "FullyQualifiedName~FileServiceTests"

# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"
```

## Test Coverage

Current test coverage focuses on Phase 5 and Phase 6 features:
- ✅ Email Service (7 email types)
- ✅ File Service (validation, upload, delete)
- ✅ Notification Service (SignalR hub integration)
- ✅ Controller Integration (authentication, authorization)

## Mocking Strategy

- **Moq**: Used for mocking dependencies (IConfiguration, ILogger, IHubContext)
- **In-Memory Database**: Used for integration tests (Entity Framework)
- **WebApplicationFactory**: Used for controller integration tests

## Test Data

Test data is seeded in integration tests:
- Test User (ID: 1, Email: test@blackbaud.com)
- Test Hackathon (ID: 1, Name: "Test Hackathon")
- Test Idea (ID: 1, Title: "Test Idea")

## Known Limitations

- **Email Tests**: SMTP tests expect exceptions due to no real SMTP server in test environment
- **File Tests**: Use temporary directories that are cleaned up after tests
- **Authentication**: Integration tests require authentication mocking (work in progress)

## Future Enhancements

- Add E2E tests with Playwright or Selenium
- Implement authentication mocking for full controller tests
- Add performance tests for file uploads
- Add load tests for SignalR hubs
