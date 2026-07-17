# Good Controller Example

Controllers should stay as HTTP adapters and delegate behavior to MediatR.

Project-style example based on the Scale controller pattern:

```csharp
[HttpPut("{id:guid}")]
[Authorize(Policy = Permissions.Scale.Update)]
public async Task<IActionResult> UpdateScale(
    [FromRoute] Guid id,
    [FromBody] UpdateScaleCommand command,
    CancellationToken cancellationToken)
{
    command.Id = id;
    var result = await Mediator.Send(command, cancellationToken);

    return result.IsSuccess
        ? Ok(ApiResponse<object>.Ok(result.Data!))
        : HandleResult(result);
}
```

## Why This Is Good
- Authorization uses `Permissions.*`.
- Route id is assigned into the command before `Mediator.Send`.
- Business logic stays in the handler.
- Response wrapping matches nearby controller style.
- Cancellation token is passed through.
