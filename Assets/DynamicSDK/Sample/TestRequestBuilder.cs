using UnityEngine;
using DynamicSDK.Unity.Utils;

namespace DynamicSDK.Sample
{
    /// <summary>
    /// Test script to verify RequestBuilder output compatibility
    /// This can be used to test that the new managed code stripping-safe classes
    /// produce the same JSON output as the previous anonymous type implementation
    /// </summary>
    public class TestRequestBuilder : MonoBehaviour
    {
        [ContextMenu("Test All Request Builders")]
        public void TestAllRequestBuilders()
        {
            Debug.Log("=== Testing RequestBuilder JSON Output ===");

            // Test connect wallet
            string connectRequest = RequestBuilder.BuildConnectWalletRequest();
            Debug.Log($"Connect Wallet Request:\n{connectRequest}");

            // Test disconnect
            string disconnectRequest = RequestBuilder.BuildDisconnectRequest();
            Debug.Log($"Disconnect Request:\n{disconnectRequest}");

            // Test sign message
            string signRequest = RequestBuilder.BuildSignMessageRequest("0x123...", "Hello World");
            Debug.Log($"Sign Message Request:\n{signRequest}");

            // Test transaction
            string transactionRequest = RequestBuilder.BuildTransactionRequest(
                "0x123...", "0x456...", "1.5", "sui", "mainnet");

            Debug.Log($"Transaction Request:\n{transactionRequest}");

            // Test get balance
            string balanceRequest = RequestBuilder.BuildGetBalanceRequest("0x123...", "sui");
            Debug.Log($"Get Balance Request:\n{balanceRequest}");

            // Test open profile
            string profileRequest = RequestBuilder.BuildOpenProfileRequest("0x123...");
            Debug.Log($"Open Profile Request:\n{profileRequest}");

            // Test JWT token
            string jwtRequest = RequestBuilder.BuildGetJwtTokenRequest();
            Debug.Log($"JWT Token Request:\n{jwtRequest}");

            Debug.Log("=== All Tests Completed ===");
        }

        [ContextMenu("Test Validators")]
        public void TestValidators()
        {
            Debug.Log("=== Testing RequestBuilder Validators ===");

            // Test address validation
            bool validAddress   = RequestBuilder.Validator.IsValidAddress("0x1234567890abcdef1234567890abcdef12345678");
            bool invalidAddress = RequestBuilder.Validator.IsValidAddress("0x123");
            Debug.Log($"Valid Address Test: {validAddress} (should be true)");
            Debug.Log($"Invalid Address Test: {invalidAddress} (should be false)");

            // Test amount validation
            bool validAmount    = RequestBuilder.Validator.IsValidAmount("1.5");
            bool invalidAmount  = RequestBuilder.Validator.IsValidAmount("-1");
            bool invalidAmount2 = RequestBuilder.Validator.IsValidAmount("abc");
            Debug.Log($"Valid Amount Test: {validAmount} (should be true)");
            Debug.Log($"Invalid Amount Test 1: {invalidAmount} (should be false)");
            Debug.Log($"Invalid Amount Test 2: {invalidAmount2} (should be false)");

            // Test message validation
            bool validMessage   = RequestBuilder.Validator.IsValidMessage("Hello World");
            bool invalidMessage = RequestBuilder.Validator.IsValidMessage("");
            Debug.Log($"Valid Message Test: {validMessage} (should be true)");
            Debug.Log($"Invalid Message Test: {invalidMessage} (should be false)");

            // Test chain validation
            bool validChain   = RequestBuilder.Validator.IsValidChain("sui");
            bool invalidChain = RequestBuilder.Validator.IsValidChain("");
            Debug.Log($"Valid Chain Test: {validChain} (should be true)");
            Debug.Log($"Invalid Chain Test: {invalidChain} (should be false)");

            Debug.Log("=== Validator Tests Completed ===");
        }
    }
}