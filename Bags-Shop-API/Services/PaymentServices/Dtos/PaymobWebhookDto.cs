using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Bags_Shop_API.Services.PaymentServices.Dtos
{
    public class PaymobWebhookDto
    {
        [JsonProperty("type")]
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonProperty("obj")]
        [JsonPropertyName("obj")]
        public PaymobTransactionObj? Obj { get; set; }

        [JsonProperty("issuer_bank")]
        [JsonPropertyName("issuer_bank")]
        public string? IssuerBank { get; set; }

        [JsonProperty("transaction_processed_callback_responses")]
        [JsonPropertyName("transaction_processed_callback_responses")]
        public string? TransactionProcessedCallbackResponses { get; set; }

        [JsonProperty("hmac")]
        [JsonPropertyName("hmac")]
        public string? Hmac { get; set; }
    }

    public class PaymobTransactionObj
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonProperty("pending")]
        [JsonPropertyName("pending")]
        public bool Pending { get; set; }

        [JsonProperty("amount_cents")]
        [JsonPropertyName("amount_cents")]
        public long AmountCents { get; set; }

        [JsonProperty("success")]
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonProperty("currency")]
        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonProperty("created_at")]
        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonProperty("error_occured")]
        [JsonPropertyName("error_occured")]
        public bool ErrorOccured { get; set; }

        [JsonProperty("has_parent_transaction")]
        [JsonPropertyName("has_parent_transaction")]
        public bool HasParentTransaction { get; set; }

        [JsonProperty("integration_id")]
        [JsonPropertyName("integration_id")]
        public long? IntegrationId { get; set; }

        [JsonProperty("is_3d_secure")]
        [JsonPropertyName("is_3d_secure")]
        public bool Is3DSecure { get; set; }

        [JsonProperty("is_auth")]
        [JsonPropertyName("is_auth")]
        public bool IsAuth { get; set; }

        [JsonProperty("is_capture")]
        [JsonPropertyName("is_capture")]
        public bool IsCapture { get; set; }

        [JsonProperty("is_refunded")]
        [JsonPropertyName("is_refunded")]
        public bool IsRefunded { get; set; }

        [JsonProperty("is_standalone_payment")]
        [JsonPropertyName("is_standalone_payment")]
        public bool IsStandalonePayment { get; set; }

        [JsonProperty("is_voided")]
        [JsonPropertyName("is_voided")]
        public bool IsVoided { get; set; }

        [JsonProperty("owner")]
        [JsonPropertyName("owner")]
        public long? Owner { get; set; }

        [JsonProperty("order")]
        [JsonPropertyName("order")]
        public PaymobOrder? Order { get; set; }

        [JsonProperty("payment_key_claims")]
        [JsonPropertyName("payment_key_claims")]
        public PaymobPaymentKeyClaims? PaymentKeyClaims { get; set; }

        [JsonProperty("source_data")]
        [JsonPropertyName("source_data")]
        public PaymobSourceData? SourceData { get; set; }
    }

    public class PaymobOrder
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonProperty("merchant_order_id")]
        [JsonPropertyName("merchant_order_id")]
        public string? MerchantOrderId { get; set; }

        [JsonProperty("amount_cents")]
        [JsonPropertyName("amount_cents")]
        public long AmountCents { get; set; }

        [JsonProperty("currency")]
        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonProperty("paid_amount_cents")]
        [JsonPropertyName("paid_amount_cents")]
        public long PaidAmountCents { get; set; }

        [JsonProperty("payment_status")]
        [JsonPropertyName("payment_status")]
        public string? PaymentStatus { get; set; }
    }

    public class PaymobPaymentKeyClaims
    {
        [JsonProperty("order_id")]
        [JsonPropertyName("order_id")]
        public long OrderId { get; set; }

        [JsonProperty("amount_cents")]
        [JsonPropertyName("amount_cents")]
        public long AmountCents { get; set; }

        [JsonProperty("currency")]
        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonProperty("integration_id")]
        [JsonPropertyName("integration_id")]
        public long IntegrationId { get; set; }

        [JsonProperty("user_id")]
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonProperty("extra")]
        [JsonPropertyName("extra")]
        public PaymobExtra? Extra { get; set; }
    }

    public class PaymobExtra
    {
        [JsonProperty("special_reference")]
        [JsonPropertyName("special_reference")]
        public string? SpecialReference { get; set; }
    }

    public class PaymobSourceData
    {
        [JsonProperty("type")]
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonProperty("sub_type")]
        [JsonPropertyName("sub_type")]
        public string? SubType { get; set; }

        [JsonProperty("pan")]
        [JsonPropertyName("pan")]
        public string? Pan { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string? PanLast4 => Pan?.Length > 4 ? Pan.Substring(Pan.Length - 4) : Pan;
    }
}
