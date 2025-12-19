using Newtonsoft.Json;

namespace Bags_Shop_API.Services.PaymentServices.Dtos
{
    public class PaymobWebhookDto
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("obj")]
        public PaymobTransactionObj? Obj { get; set; }

        [JsonProperty("issuer_bank")]
        public string? IssuerBank { get; set; }

        [JsonProperty("transaction_processed_callback_responses")]
        public string? TransactionProcessedCallbackResponses { get; set; }

        [JsonProperty("hmac")]
        public string? Hmac { get; set; }
    }

    public class PaymobTransactionObj
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("pending")]
        public bool Pending { get; set; }

        [JsonProperty("amount_cents")]
        public long AmountCents { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("currency")]
        public string? Currency { get; set; }

        [JsonProperty("created_at")]
        public string? CreatedAt { get; set; }

        [JsonProperty("error_occured")]
        public bool ErrorOccured { get; set; }

        [JsonProperty("has_parent_transaction")]
        public bool HasParentTransaction { get; set; }

        [JsonProperty("integration_id")]
        public long? IntegrationId { get; set; }

        [JsonProperty("is_3d_secure")]
        public bool Is3DSecure { get; set; }

        [JsonProperty("is_auth")]
        public bool IsAuth { get; set; }

        [JsonProperty("is_capture")]
        public bool IsCapture { get; set; }

        [JsonProperty("is_refunded")]
        public bool IsRefunded { get; set; }

        [JsonProperty("is_standalone_payment")]
        public bool IsStandalonePayment { get; set; }

        [JsonProperty("is_voided")]
        public bool IsVoided { get; set; }

        [JsonProperty("owner")]
        public long? Owner { get; set; }

        [JsonProperty("order")]
        public PaymobOrder? Order { get; set; }

        [JsonProperty("payment_key_claims")]
        public PaymobPaymentKeyClaims? PaymentKeyClaims { get; set; }

        [JsonProperty("source_data")]
        public PaymobSourceData? SourceData { get; set; }
    }

    public class PaymobOrder
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("merchant_order_id")]
        public string? MerchantOrderId { get; set; }

        [JsonProperty("amount_cents")]
        public long AmountCents { get; set; }

        [JsonProperty("currency")]
        public string? Currency { get; set; }

        [JsonProperty("paid_amount_cents")]
        public long PaidAmountCents { get; set; }

        [JsonProperty("payment_status")]
        public string? PaymentStatus { get; set; }
    }

    public class PaymobPaymentKeyClaims
    {
        [JsonProperty("order_id")]
        public long OrderId { get; set; }

        [JsonProperty("amount_cents")]
        public long AmountCents { get; set; }

        [JsonProperty("currency")]
        public string? Currency { get; set; }

        [JsonProperty("integration_id")]
        public long IntegrationId { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }
    }

    public class PaymobSourceData
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("sub_type")]
        public string? SubType { get; set; }

        [JsonProperty("pan")]
        public string? Pan { get; set; }

        [JsonIgnore]
        public string? PanLast4 => Pan?.Length > 4 ? Pan.Substring(Pan.Length - 4) : Pan;
    }
}
