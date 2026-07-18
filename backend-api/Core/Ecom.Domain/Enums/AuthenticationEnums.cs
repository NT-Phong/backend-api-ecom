namespace Ecom.Domain.Enums;

public enum VerificationChallengePurpose { Register, Login, PasswordReset, ContactChange, EmailVerification }
public enum VerificationChallengeStatus { Pending, Consumed, Expired, Locked, Superseded }
public enum SessionClientType { Web, Mobile }
public enum AuthenticationMethod { Otp, Password, Google, Passkey, Qr, LegacyRefresh }
public enum AuthenticationStrength { SingleFactor, MultiFactor, PhishingResistant }
public enum SecurityRiskLevel { Low, Medium, High, Critical }
