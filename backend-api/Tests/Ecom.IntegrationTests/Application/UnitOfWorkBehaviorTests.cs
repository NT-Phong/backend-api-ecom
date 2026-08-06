using Ecom.Application.Common.Behaviours;
using Ecom.Application.Common.Interfaces;
using Ecom.Application.Common.Models;
using Ecom.Application.Features.AuthV2.Login;
using Ecom.Domain.Common;
using Ecom.Domain.Exceptions;
using Ecom.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecom.IntegrationTests.Application;

public sealed class UnitOfWorkBehaviorTests
{
    [Fact]
    public async Task NonTransactionalRequest_does_not_open_a_transaction()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var behavior = CreateBehavior<ReadRequest>(unitOfWork);

        var result = await behavior.Handle(new ReadRequest(), _ => Task.FromResult(TResult.Success()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, unitOfWork.BeginCalls);
        Assert.Equal(0, unitOfWork.CommitCalls);
        Assert.Equal(0, unitOfWork.RollbackCalls);
    }

    [Fact]
    public async Task Successful_transactional_request_commits_once()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var behavior = CreateBehavior<WriteRequest>(unitOfWork);

        var result = await behavior.Handle(new WriteRequest(), _ => Task.FromResult(TResult.Success()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, unitOfWork.BeginCalls);
        Assert.Equal(1, unitOfWork.CommitCalls);
        Assert.Equal(0, unitOfWork.RollbackCalls);
    }

    [Fact]
    public async Task Failed_result_rolls_back_and_clears_the_change_tracker()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var behavior = CreateBehavior<WriteRequest>(unitOfWork);

        var result = await behavior.Handle(new WriteRequest(), _ => Task.FromResult(TResult.Failure("expected failure")), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(1, unitOfWork.RollbackCalls);
        Assert.Equal(1, unitOfWork.ClearTrackerCalls);
        Assert.Equal(0, unitOfWork.CommitCalls);
    }

    [Fact]
    public async Task Exception_rolls_back_and_is_rethrown()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var behavior = CreateBehavior<WriteRequest>(unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.Handle(
            new WriteRequest(),
            _ => throw new InvalidOperationException("boom"),
            CancellationToken.None));

        Assert.Equal(1, unitOfWork.RollbackCalls);
        Assert.Equal(1, unitOfWork.ClearTrackerCalls);
        Assert.Equal(0, unitOfWork.CommitCalls);
    }

    [Fact]
    public async Task Concurrency_exception_rolls_back_clears_tracking_and_returns_stable_exception()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var behavior = CreateBehavior<WriteRequest>(unitOfWork);

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => behavior.Handle(
            new WriteRequest(),
            _ => throw new DbUpdateConcurrencyException("stale write"),
            CancellationToken.None));

        Assert.Equal(1, unitOfWork.RollbackCalls);
        Assert.Equal(1, unitOfWork.ClearTrackerCalls);
        Assert.Equal(0, unitOfWork.CommitCalls);
    }

    [Fact]
    public async Task Cancellation_rolls_back_clears_tracking_and_is_rethrown()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var behavior = CreateBehavior<WriteRequest>(unitOfWork);

        await Assert.ThrowsAsync<OperationCanceledException>(() => behavior.Handle(
            new WriteRequest(),
            _ => throw new OperationCanceledException(),
            new CancellationToken(canceled: true)));

        Assert.Equal(1, unitOfWork.RollbackCalls);
        Assert.Equal(1, unitOfWork.ClearTrackerCalls);
        Assert.Equal(0, unitOfWork.CommitCalls);
    }

    [Fact]
    public async Task Nested_transactional_request_does_not_commit_the_outer_transaction()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var outer = CreateBehavior<WriteRequest>(unitOfWork);
        var inner = CreateBehavior<NestedWriteRequest>(unitOfWork);

        var result = await outer.Handle(new WriteRequest(),
            _ => inner.Handle(new NestedWriteRequest(), _ => Task.FromResult(TResult.Success()), CancellationToken.None),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, unitOfWork.BeginCalls);
        Assert.Equal(1, unitOfWork.CommitCalls);
        Assert.Equal(0, unitOfWork.RollbackCalls);
    }

    [Fact]
    public async Task Failed_password_session_completion_rolls_back_instead_of_committing_credential_state()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var behavior = CreateBehavior<CompletePasswordLoginCommand, TResult<PasswordLoginResult>>(unitOfWork);
        var request = new CompletePasswordLoginCommand(
            Guid.NewGuid(), Guid.NewGuid(), "not-logged", null, false, "ip", "agent", DateTime.UtcNow);

        var result = await behavior.Handle(request,
            _ => Task.FromResult(TResult<PasswordLoginResult>.Failure("session creation failed")),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(1, unitOfWork.BeginCalls);
        Assert.Equal(1, unitOfWork.RollbackCalls);
        Assert.Equal(1, unitOfWork.ClearTrackerCalls);
        Assert.Equal(0, unitOfWork.CommitCalls);
    }

    [Fact]
    public async Task Failed_password_audit_is_committed_as_an_independent_successful_mutation()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var behavior = CreateBehavior<RecordFailedPasswordLoginCommand>(unitOfWork);
        var request = new RecordFailedPasswordLoginCommand(
            Guid.NewGuid(), Guid.NewGuid(), "ip", "agent", DateTime.UtcNow);

        var result = await behavior.Handle(request, _ => Task.FromResult(TResult.Success()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, unitOfWork.BeginCalls);
        Assert.Equal(1, unitOfWork.CommitCalls);
        Assert.Equal(0, unitOfWork.RollbackCalls);
    }

    private static UnitOfWorkBehavior<TRequest, TResult> CreateBehavior<TRequest>(RecordingUnitOfWork unitOfWork)
        where TRequest : IRequest<TResult> => new(unitOfWork, NullLogger<UnitOfWorkBehavior<TRequest, TResult>>.Instance);

    private static UnitOfWorkBehavior<TRequest, TResponse> CreateBehavior<TRequest, TResponse>(RecordingUnitOfWork unitOfWork)
        where TRequest : IRequest<TResponse> => new(unitOfWork, NullLogger<UnitOfWorkBehavior<TRequest, TResponse>>.Instance);

    private sealed record ReadRequest : IRequest<TResult>;
    private sealed record WriteRequest : IRequest<TResult>, ITransactionalRequest;
    private sealed record NestedWriteRequest : IRequest<TResult>, ITransactionalRequest;

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        private bool _active;
        public int BeginCalls { get; private set; }
        public int CommitCalls { get; private set; }
        public int RollbackCalls { get; private set; }
        public int ClearTrackerCalls { get; private set; }

        public IBaseRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity => throw new NotSupportedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<bool> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            BeginCalls++;
            if (_active) return Task.FromResult(false);
            _active = true;
            return Task.FromResult(true);
        }
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            CommitCalls++;
            _active = false;
            return Task.CompletedTask;
        }
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            RollbackCalls++;
            _active = false;
            return Task.CompletedTask;
        }
        public Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default) => action();
        public Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> func, CancellationToken cancellationToken = default) => func();
        public void ClearChangeTracker() => ClearTrackerCalls++;
        public void Dispose() { }
    }
}
