using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProjectLogistiXX.Application.Abstraction.Contracts.Idempotency;
using ProjectLogistiXX.Application.Abstraction.Data;
using ProjectLogistiXX.Application.Abstraction.Decorators;
using ProjectLogistiXX.Application.Abstraction.Exceptions.Idempotency;
using ProjectLogistiXX.Application.Abstraction.Messaging;
using ProjectLogistiXX.Domain.Abstraction;

namespace ProjectLogistiXX.Test.TestApplication.IdempotencyDecorator;

public sealed class IdempotentDecoratorTests
{
    public sealed record TestCommand(string Data) : ICommand<string>;

    private readonly Mock<ICommandHandler<TestCommand, string>> _handler = new();
    private readonly Mock<IIdempotencyService<string>> _service = new();
    private readonly Mock<IRequestContext> _requestContext = new();
    private readonly Mock<IApplicationDbContext> _dbContext = new();
    private readonly Mock<IDbContextTransaction> _transaction = new();
    private readonly IdempotencyKeyId _key = IdempotencyKeyId.NewId();
    private readonly UserId _userId = UserId.NewId();

    public IdempotentDecoratorTests()
    {
        _requestContext.Setup(r => r.IdempotencyKey).Returns(_key);
        _requestContext.Setup(r => r.UserId).Returns(_userId);
        _dbContext.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transaction.Object);
        _handler.Setup(h => h.Handle(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("ok"));
    }

    private IdempotentDecorator<TestCommand, string> CreateSut() => new(
        _handler.Object,
        _service.Object,
        _requestContext.Object,
        _dbContext.Object,
        NullLogger<IdempotentDecorator<TestCommand, string>>.Instance);

    [Fact]
    public async Task Handle_WhenNewReservationAndHandlerSucceeds_CompletesAndCommits()
    {
        var lockToken = Guid.NewGuid();
        _service.Setup(s => s.TryReserveAsync(_key, _userId, nameof(TestCommand), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdempotencyReservationResult<string>.NewReservation(lockToken));

        var result = await CreateSut().Handle(new TestCommand("x"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);
        _handler.Verify(h => h.Handle(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _service.Verify(s => s.CompleteAsync(_key, _userId, lockToken, "ok", It.IsAny<CancellationToken>()), Times.Once);
        _transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _service.Verify(s => s.ReleaseAsync(It.IsAny<IdempotencyKeyId>(), It.IsAny<UserId>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenReservationStatusIsUnknown_ThrowsFatalAndDoesNotRunHandler()
    {
        var reservation = IdempotencyReservationResult<string>.NewReservation(Guid.NewGuid());
        // Status has a private init, so reflection is the only way to simulate a value the decorator doesn't know.
        typeof(IdempotencyReservationResult<string>)
            .GetProperty(nameof(reservation.Status))!
            .SetValue(reservation, (IdempotencyReservationStatus)99);

        _service.Setup(s => s.TryReserveAsync(_key, _userId, nameof(TestCommand), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        await Assert.ThrowsAsync<IdempotencyFatalException>(
            () => CreateSut().Handle(new TestCommand("x"), CancellationToken.None));

        _handler.Verify(h => h.Handle(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _dbContext.Verify(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenKeyPresentButUserMissing_ThrowsAndDoesNotRunHandler()
    {
        _requestContext.Setup(r => r.UserId).Returns((UserId?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateSut().Handle(new TestCommand("x"), CancellationToken.None));

        _handler.Verify(h => h.Handle(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _service.Verify(s => s.TryReserveAsync(It.IsAny<IdempotencyKeyId>(), It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
