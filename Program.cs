/// MIT License
/// Copyright © 2021 pmcgoohan
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.Web3;
using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Drawing;

namespace EthInclude
{
    class Program
    {
        static Nethereum.Web3.Web3 _web3;
        static Dictionary<string, TxTime> _txh = new Dictionary<string, TxTime>();
        static ulong _arrivalIndex = 0;
        static ulong _inclusionIndex = 0;
        static DateTime _lastTimestamp = DateTime.MinValue;
        static Nethereum.Hex.HexTypes.HexBigInteger _lastFlashbotsBlock;
        static Nethereum.Hex.HexTypes.HexBigInteger _lastBlock;

        static void Main(string[] args)
        {
            // collect outstanding pending txs to estimate tx isWarm flag
            Console.WriteLine("how many pending txs currently at https://etherscan.io/txsPending?");
            DB.PendingTxsToImport = ulong.Parse(Console.ReadLine());
            DB.PendingTxsToImport += 10000; // add a safety amount

            // connect to eth node
            var client = new StreamingWebSocketClient(DB.GetConfig("WssNodePath"));
            _web3 = new Nethereum.Web3.Web3(DB.GetConfig("HttpsNodePath"));

            // subscribe blocks
            var blockHeaderSubscription = new EthNewBlockHeadersObservableSubscription(client);
            blockHeaderSubscription.GetSubscriptionDataResponsesAsObservable().Subscribe(NewBlock);

            // subscribe pending txs
            var pendingTransactionsSubscription = new EthNewPendingTransactionObservableSubscription(client);
            pendingTransactionsSubscription.GetSubscriptionDataResponsesAsObservable().Subscribe(NewPendingTx);

            // wait for data
            client.StartAsync().Wait();
            blockHeaderSubscription.SubscribeAsync().Wait();
            pendingTransactionsSubscription.SubscribeAsync().Wait();

            // wait for exit
            Console.ReadLine();
        }

        public static void NewPendingTx(string txh)
        {
            // record new pending txs as they arrive
            DateTime timestamp = DateTime.Now;
            var tt = new TxTime();
            tt.Hash = txh;
            tt.ArrivalTime = timestamp;
            tt.ArrivalIndex = _arrivalIndex;
            tt.ArrivalBlockNumber = _lastBlock;
            tt.IsWarm = _arrivalIndex > DB.PendingTxsToImport;
            if (_txh.TryAdd(txh, tt))
                _arrivalIndex++;
        }

        public static void NewBlock(Block block)
        {
            _lastBlock = block.Number;
            DateTime timestamp = DateTime.Now;

            // get block txs
            var blockTxsReq = _web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(block.Number);
            blockTxsReq.Wait();

            // iterate txs
            var btxs = blockTxsReq.Result;
            if (btxs == null || btxs.Transactions == null) return;
            List<TxTime> tts = new List<TxTime>(btxs.Transactions.Length);

            foreach (var t in btxs.Transactions)
            {
                TxTime tt;
                if (!_txh.TryGetValue(t.TransactionHash, out tt))
                {
                    // if we haven't seen this tx before, it has likely been sent direct to the miner (Flashbots, MistX, Mining pool, etc)
                    // until all pending txs have been downloaded, the first of these will be set incorrectly (isWarm set false)
                    tt = new TxTime();
                    tt.IsDark = true;
                    tt.Hash = t.TransactionHash;
                    tt.ArrivalTime = timestamp;
                    tt.ArrivalIndex = _arrivalIndex++;
                    tt.ArrivalBlockNumber = block.Number;
                    tt.IsWarm = _arrivalIndex > DB.PendingTxsToImport;
                }
                else
                {
                    // if we have already assigned this tx to a block, don't overwrite
                    if (tt.BlockNumber != null)
                        continue;

                    tt.IsDark = false;
                }

                // updating block inclusion details
                tt.BlockNumber = block.Number;
                tt.TxIndex = t.TransactionIndex;
                tt.Gas = t.Gas;
                tt.GasPrice = t.GasPrice;
                tt.InclusionTime = timestamp;
                tt.InclusionIndex = _inclusionIndex++;
                if (tt.IsDark)
                    tt.Delay = new TimeSpan(0);
                else
                    tt.Delay = tt.InclusionTime - tt.ArrivalTime;

                tts.Add(tt);
            }

            // non blocking write to the db
            _ = DB.WriteTxTimeAsync(tts);

            // non blocking collection and write of flashbots data (if this is a new block)
            if (block.Number != _lastFlashbotsBlock)
            {
                _ = Flashbots.Collect(4000); // delay by a few seconds to give them a chance to update the api with the new block
                _lastFlashbotsBlock = block.Number;
            }

            Console.WriteLine("block " + block.Number);
        }

        static void VisualizeBitmap()
        {
            // get pixel at arrival index
            // write to inclusion index

            // load timed tx data and sort by arrival index
            List<TxTime> tts = TxTime.Load(DB.GetConfig("ReadFile")); // note: first version using csv files rather than MySql - not yet updated for db
            tts.Sort();

            Bitmap bmpIn = new Bitmap(DB.GetConfig("ReadBmp"), true);
            Bitmap bmpOut = new Bitmap(DB.GetConfig("ReadBmp"), true);

            int x, y;
            for (x = 0; x < bmpOut.Width; x++)
                for (y = 0; y < bmpOut.Height; y++)
                    bmpOut.SetPixel(x, y, Color.Transparent);

            for (int i = 0; i < tts.Count; i++)
            {
                // allow wrap around if we run out of image data
                int k = 0;
                do
                {
                    int ai = (int)tts[i].ArrivalIndex + k;
                    x = ai % bmpIn.Width;
                    y = ai / bmpIn.Width;
                    k -= bmpIn.Width * bmpIn.Height;
                } while (y >= bmpIn.Height);

                if (y < bmpIn.Height)
                {
                    // get the pixel at the arrival index (having wrapped around if required)
                    Color pixelColor = bmpIn.GetPixel(x, y);

                    int ii = (int)tts[i].InclusionIndex;
                    x = ii % bmpOut.Width;
                    y = ii / bmpOut.Width;

                    // write it where it was included
                    if (y < bmpOut.Height)
                        bmpOut.SetPixel((int)x, (int)y, pixelColor);
                }
            }

            bmpOut.Save(DB.GetConfig("WriteBmp"));
        }
    }
}
