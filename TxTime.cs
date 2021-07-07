using Nethereum.Contracts;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.JsonRpc.Client.Streaming;
using Nethereum.RPC.Eth.Subscriptions;
using Nethereum.RPC.Reactive.Eth;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.RPC.Reactive.Extensions;
using Nethereum.JsonRpc.WebSocketClient;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Drawing;
using System.Diagnostics.CodeAnalysis;

namespace EthInclude
{
    public class TxTime : IComparable<TxTime>
    {
        public string Hash;
        public HexBigInteger BlockNumber;
        public HexBigInteger TxIndex;
        public HexBigInteger Gas;
        public HexBigInteger GasPrice;
        public DateTime ArrivalTime;
        public DateTime InclusionTime;
        public bool IsWarm; // set if the initial glut of pending txs was processed by the time this was created
        public bool IsDark; // set if the tx was not seen in the mempool, and was therefore likely added by the miner at block creation time
        public TimeSpan Delay; // between arrival and inclusion
        public ulong ArrivalIndex;
        public ulong InclusionIndex;
        public HexBigInteger ArrivalBlockNumber;

        public static List<TxTime> Load(string file)
        {
            List<TxTime> r = new List<TxTime>();

            using (var sr = new StreamReader(file))
            {
                // skip header
                sr.ReadLine();

                while (!sr.EndOfStream)
                {
                    string[] f = sr.ReadLine().Split(",");
                    if (f.Length != 8) continue;

                    // only load the fields we are interested in
                    TxTime tt = new TxTime();
                    tt.ArrivalIndex = ulong.Parse(f[6]);
                    tt.InclusionIndex = ulong.Parse(f[7]);
                    r.Add(tt);
                }
            }

            return r;
        }

        public int CompareTo(TxTime other)
        {
            return this.ArrivalIndex.CompareTo(other.ArrivalIndex);
        }
    }
}