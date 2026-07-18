using Ecom.Domain.Enums;

namespace Ecom.Domain.Entities;

public sealed class SecurityEvent : BaseEntity
{
    private SecurityEvent() { }
    public SecurityEvent(Guid? userId, Guid? sessionId, string eventType, SecurityRiskLevel riskLevel,
        bool success, string ipFingerprint, string userAgentSummary, string? metadata, DateTime occurredAt)
    { UserId=userId; SessionId=sessionId; EventType=eventType; RiskLevel=riskLevel; Success=success;
      IpFingerprint=ipFingerprint; UserAgentSummary=userAgentSummary; Metadata=metadata; OccurredAt=occurredAt; }
    public Guid? UserId { get; private set; }
    public Guid? SessionId { get; private set; }
    public string EventType { get; private set; } = null!;
    public SecurityRiskLevel RiskLevel { get; private set; }
    public bool Success { get; private set; }
    public string IpFingerprint { get; private set; } = null!;
    public string UserAgentSummary { get; private set; } = null!;
    public string? Metadata { get; private set; }
    public DateTime OccurredAt { get; private set; }
}
