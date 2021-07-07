using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Serialization;
using Nethereum.Hex.HexTypes;

namespace EthInclude.FB
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Transaction : IComparable<Transaction>
    {
        public string transaction_hash { get; set; }
        public int tx_index { get; set; }
        public int bundle_index { get; set; }
        public ulong block_number { get; set; }
        public string eoa_address { get; set; }
        public string to_address { get; set; }
        public ulong gas_used { get; set; }
        public ulong gas_price { get; set; }
        public ulong coinbase_transfer { get; set; }
        public ulong total_miner_reward { get; set; }

        public int CompareTo(Transaction other)
        {
            if (this.bundle_index != other.bundle_index)
                return this.bundle_index.CompareTo(other.bundle_index);
            return this.tx_index.CompareTo(other.tx_index);
        }
    }

    public class Block
    {
        public ulong block_number { get; set; }
        public ulong miner_reward { get; set; }
        public string miner { get; set; }
        public ulong coinbase_transfers { get; set; }
        public ulong gas_used { get; set; }
        public ulong gas_price { get; set; }
        public List<Transaction> transactions { get; set; }
    }

    public class Root
    {
        public List<Block> blocks { get; set; }
        public ulong latest_block_number { get; set; }
    }
}