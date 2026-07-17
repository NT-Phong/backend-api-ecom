namespace Ecom.Domain.Enums;

public enum AdministrativeAreaLevel { Province, District, Ward }
public enum PublicStatus { Draft, Verified, Published, Hidden }
public enum ContactType { Phone, Email, Zalo, Website }
public enum CatalogStatus { Draft, Published, Hidden }
public enum ProductStatus { Draft, Review, Published, Paused, Discontinued }
public enum VariantStatus { Active, Paused, Discontinued }
public enum InventoryMode { NotTracked, Tracked, Preorder }
public enum MediaType { Image, Video, Document }
public enum MediaVisibility { Public, Internal, Restricted }
public enum MediaScanStatus { Pending, Clean, Rejected, Failed }
public enum PriceListStatus { Draft, Active, Inactive, Expired }
public enum PriceType { Public, Sale, B2B }
public enum PromotionType { Percentage, FixedAmount, FreeShipping }
public enum PromotionStatus { Draft, Active, Paused, Expired }
public enum CouponStatus { Draft, Active, Paused, Expired }
public enum InventoryReservationStatus { Active, Consumed, Released, Expired }
public enum InventoryMovementType { Receive, Allocate, Release, Adjust, Ship, Return }
public enum CartStatus { Active, Converted, Expired }
public enum OrderStatus { Pending, Confirmed, Processing, Shipped, Delivered, Cancelled, Refunded }
public enum OrderNoteType { Internal, Customer, System }
public enum PaymentMethod { COD, BankTransfer, Gateway }
public enum PaymentStatus { Pending, Authorized, Paid, Failed, Refunded }
public enum PaymentTransactionType { Initiate, Capture, Verify, Refund }
public enum ShipmentStatus { Pending, Ready, Shipped, Delivered, Failed }
public enum CertificationVerificationStatus { Pending, Verified, Rejected, Expired }
public enum CertificationEvidenceType { Certificate, SupportingDocument, Other }
public enum TraceLotStatus { Draft, Active, Expired, Recalled }
public enum ReviewModerationStatus { Pending, Published, Hidden, Rejected }
public enum QuestionStatus { Pending, Answered, Hidden }
public enum AnswerStatus { Draft, Published, Hidden }
public enum NewsletterStatus { Pending, Subscribed, Unsubscribed }
public enum ContentStatus { Draft, Published, Hidden, Archived }
public enum TradeInquiryType { BulkPurchase, Agency, Partnership }
public enum TradeInquiryStatus { New, InProgress, Qualified, Closed, Rejected }
public enum PartnerApplicationType { Agency, Distributor, Partnership }
public enum PartnerApplicationStatus { New, Reviewing, Approved, Rejected, Closed }
public enum NotificationDeliveryStatus { Pending, Delivered, Failed, Read }
public enum ConsentStatus { Unknown, Granted, Denied, Withdrawn }
public enum AnalyticsEventType { PageView, Search, ProductView, AddToCart, Checkout }
