using System;
using UnityEngine;

namespace DynamicSDK.Unity.Messages.Wallet
{
    // ============================================================================
    // BALANCE RESPONSE
    // ============================================================================

    [Serializable]
    public class BalanceResponseData
    {
        public string walletAddress;
        public string chain;
        public string network;
        public string balance;
        public string symbol;
        public int decimals;
        public string tokenAddress;
        public float usdValue;
        public bool success;
        public string error;

        public override string ToString()
        {
            return $"BalanceResponseData {{ WalletAddress: {walletAddress}, Chain: {chain}, Balance: {balance} {symbol}, Success: {success}, Error: {error} }}";
        }
    }

    [Serializable]
    public class BalanceResponseMessage : BaseMessage
    {
        public BalanceResponseData data;

        public BalanceResponseMessage()
        {
            type = "wallet";
            action = WalletActions.BALANCE_RESPONSE;
        }

        public override string ToString()
        {
            return $"BalanceResponseMessage {{ Type: {type}, Action: {action}, Data: {data} }}";
        }
    }

    // ============================================================================
    // SIGN MESSAGE RESPONSE
    // ============================================================================

    [Serializable]
    public class SignMessageResponseData
    {
        public string walletAddress;
        public string signature;
        public string message;
        public bool success;
        public string error;

        public override string ToString()
        {
            return $"SignMessageResponseData {{ WalletAddress: {walletAddress}, Message: {message}, Success: {success}, Error: {error} }}";
        }
    }

    [Serializable]
    public class SignMessageResponseMessage : BaseMessage
    {
        public SignMessageResponseData data;

        public SignMessageResponseMessage()
        {
            type = "wallet";
            action = WalletActions.SIGN_MESSAGE_RESPONSE;
        }

        public override string ToString()
        {
            return $"SignMessageResponseMessage {{ Type: {type}, Action: {action}, Data: {data} }}";
        }
    }

    // ============================================================================
    // TRANSACTION RESPONSE
    // ============================================================================

    [Serializable]
    public class TransactionResponseData
    {
        public string walletAddress;
        public string transactionHash;
        public bool success;
        public string error;
        public string gasUsed;
        public long blockNumber;
        public int confirmations;

        public override string ToString()
        {
            return $"TransactionResponseData {{ WalletAddress: {walletAddress}, TxHash: {transactionHash}, Success: {success}, Error: {error} }}";
        }
    }

    [Serializable]
    public class TransactionResponseMessage : BaseMessage
    {
        public TransactionResponseData data;

        public TransactionResponseMessage()
        {
            type = "wallet";
            action = WalletActions.TRANSACTION_RESPONSE;
        }

        public override string ToString()
        {
            return $"TransactionResponseMessage {{ Type: {type}, Action: {action}, Data: {data} }}";
        }
    }

    // ============================================================================
    // WALLET CONNECTION MESSAGES
    // ============================================================================

    [Serializable]
    public class WalletConnectedData
    {
        public WalletCredential wallet;
        public bool success;

        public override string ToString()
        {
            return $"WalletConnectedData {{ Wallet: {wallet}, Success: {success} }}";
        }
    }

    [Serializable]
    public class WalletConnectedMessage : BaseMessage
    {
        public WalletConnectedData data;

        public WalletConnectedMessage()
        {
            type = "wallet";
            action = WalletActions.WALLET_CONNECTED;
        }

        public override string ToString()
        {
            return $"WalletConnectedMessage {{ Type: {type}, Action: {action}, Data: {data} }}";
        }
    }

    [Serializable]
    public class WalletDisconnectedData
    {
        public string walletAddress;
        public string reason;
        public bool success;

        public override string ToString()
        {
            return $"WalletDisconnectedData {{ WalletAddress: {walletAddress}, Reason: {reason}, Success: {success} }}";
        }
    }

    [Serializable]
    public class WalletDisconnectedMessage : BaseMessage
    {
        public WalletDisconnectedData data;

        public WalletDisconnectedMessage()
        {
            type = "wallet";
            action = WalletActions.WALLET_DISCONNECTED;
        }

        public override string ToString()
        {
            return $"WalletDisconnectedMessage {{ Type: {type}, Action: {action}, Data: {data} }}";
        }
    }

    // ============================================================================
    // WALLET ERROR
    // ============================================================================

    [Serializable]
    public class WalletErrorData : ErrorInfo
    {
        public string walletAddress;
        public string action;

        public override string ToString()
        {
            return $"WalletErrorData {{ WalletAddress: {walletAddress}, Action: {action}, Error: {error} }}";
        }
    }

    [Serializable]
    public class WalletErrorMessage : BaseMessage
    {
        public WalletErrorData data;

        public WalletErrorMessage()
        {
            type = "wallet";
            action = WalletActions.WALLET_ERROR;
        }

        public override string ToString()
        {
            return $"WalletErrorMessage {{ Type: {type}, Action: {action}, Data: {data} }}";
        }
    }

    // ============================================================================
    // WALLETS RESPONSE
    // ============================================================================

    [Serializable]
    public class WalletsResponseData
    {
        public WalletCredential[] wallets;
        public WalletCredential primaryWallet;
        public bool success;
        public string error;

        public override string ToString()
        {
            return $"WalletsResponseData {{ WalletsCount: {wallets?.Length ?? 0}, PrimaryWallet: {primaryWallet?.address}, Success: {success}, Error: {error} }}";
        }
    }

    [Serializable]
    public class WalletsResponseMessage : BaseMessage
    {
        public WalletsResponseData data;

        public WalletsResponseMessage()
        {
            type = "wallet";
            action = WalletActions.WALLETS_RESPONSE;
        }

        public override string ToString()
        {
            return $"WalletsResponseMessage {{ Type: {type}, Action: {action}, Data: {data} }}";
        }
    }

    // ============================================================================
    // NETWORKS RESPONSE
    // ============================================================================

    [Serializable]
    public class NetworkInfo
    {
        public string chainId;
        public string cluster;
        public string genesisHash;
        public string iconUrl;
        public bool isTestnet;
        public string key;
        public string name;
        public NativeCurrency nativeCurrency;
        public string networkId;
        public string vanityName;

        public override string ToString()
        {
            return $"NetworkInfo {{ ChainId: {chainId}, Name: {name}, VanityName: {vanityName}, IsTestnet: {isTestnet}, Symbol: {nativeCurrency?.symbol} }}";
        }
    }

    [Serializable]
    public class NativeCurrency
    {
        public int decimals;
        public string name;
        public string symbol;
        public string denom;
        public string iconUrl;

        public override string ToString()
        {
            return $"NativeCurrency {{ Name: {name}, Symbol: {symbol}, Decimals: {decimals}, Denom: {denom} }}";
        }
    }

    [Serializable]
    public class NetworksResponseData
    {
        public NetworkInfo[] networks;
        public bool success;
        public string error;

        public override string ToString()
        {
            return $"NetworksResponseData {{ NetworksCount: {networks?.Length ?? 0}, Success: {success}, Error: {error} }}";
        }
    }

    [Serializable]
    public class NetworksResponseMessage : BaseMessage
    {
        public NetworksResponseData data;

        public NetworksResponseMessage()
        {
            type = "wallet";
            action = WalletActions.NETWORKS_RESPONSE;
        }

        public override string ToString()
        {
            return $"NetworksResponseMessage {{ Type: {type}, Action: {action}, Data: {data} }}";
        }
    }
}