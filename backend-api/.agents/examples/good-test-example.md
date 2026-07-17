# Good Verification Example

This repository currently has no dedicated automated test project. For code changes, use the narrowest relevant build and report the test gap.

```powershell
dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore
dotnet build Presentation\Ecom.API\Ecom.API.csproj --no-restore
```

If a test project is added later, prefer behavior-focused tests:

```csharp
[Fact]
public async Task Handle_WhenScaleDoesNotExist_ReturnsNotFound()
{
    var command = new UpdateScaleCommand { Id = missingId, Name = "Scale A" };

    var result = await handler.Handle(command, CancellationToken.None);

    result.IsSuccess.Should().BeFalse();
    result.ErrorCode.Should().Be(ErrorCodes.NOT_FOUND);
}
```

## Why This Is Good
- It uses the actual project result convention.
- It verifies behavior instead of private implementation.
- It does not pretend current feature commands named `Test*Connection` are automated tests.

