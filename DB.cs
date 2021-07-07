using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.Web3;
using MySql.Data;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace EthInclude
{
    public class DB
    {
        static IConfiguration _config;
        public static ulong PendingTxsToImport;

        public static string GetConfig(string key)
        {
            if (_config == null)
                _config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

            return _config[key];
        }

        public static void WriteFlashbots(FB.Root fb)
        {
            if (fb == null)
                return;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            using (MySqlConnection conn = new MySqlConnection(_config["MySqlConnection"]))
            {
                conn.Open();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;

                    foreach (FB.Block b in fb.blocks)
                    {
                        cmd.CommandText = @$"INSERT IGNORE INTO `tx_time`.`fb_block`
                                            (`block_number`,
                                            `miner_reward`,
                                            `miner`,
                                            `coinbase_transfer`,
                                            `gas_used`,
                                            `gas_price`)
                                            VALUES
                                            ({b.block_number},
                                            {b.miner_reward},
                                            '{b.miner}',
                                            {b.coinbase_transfers},
                                            {b.gas_used},
                                            {b.gas_price})";
                        cmd.ExecuteNonQuery();

                        foreach (FB.Transaction t in b.transactions)
                        {
                            cmd.CommandText = @$"INSERT IGNORE INTO `tx_time`.`fb_tx`
                                            (`hash`,
                                            `index`,
                                            `bundle`,
                                            `block_number`,
                                            `eoa_address`,
                                            `to_address`,
                                            `gas_used`,
                                            `gas_price`,
                                            `coinbase_transfer`,
                                            `total_miner_reward`)
                                            VALUES
                                            ('{t.transaction_hash}',
                                            {t.tx_index},
                                            {t.bundle_index},
                                            {t.block_number},
                                            '{t.eoa_address}',
                                            '{t.to_address}',
                                            {t.gas_used},
                                            {t.gas_price},
                                            '{t.coinbase_transfer}',
                                            {t.total_miner_reward})";
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("error WriteFlashbots " + e.ToString());
                            }
                        }
                    }
                }
                conn.Close();

                sw.Stop();
                Console.WriteLine("fb block {2} update {0} blocks in {1} ms", fb.blocks.Count, sw.ElapsedMilliseconds, fb.latest_block_number);
            }
        }

        public static async Task WriteTxTimeAsync(List<TxTime> tts)
        {
            await Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                using (MySqlConnection conn = new MySqlConnection(DB.GetConfig("MySqlConnection")))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;

                        foreach (TxTime tt in tts)
                        {
                            // this happens when we haven't yet recieved a block
                            if (tt.ArrivalBlockNumber == null)
                                continue;

                            string sql = @$"INSERT INTO `tx_time`.`eth_tx`
                                                        (`hash`,
                                                        `block_number`,
                                                        `index`,
                                                        `gas`,
                                                        `gas_price`,
                                                        `arrival_time`,
                                                        `inclusion_time`,
                                                        `is_warm`,
                                                        `is_dark`,
                                                        `delay_ms`,
                                                        `arrival_index`,
                                                        `inclusion_index`,
                                                        `arrival_block`)
                                                        VALUES
                                                        ('{tt.Hash}',
                                                        {tt.BlockNumber},
                                                        {tt.TxIndex},
                                                        {tt.Gas},
                                                        {tt.GasPrice},
                                                        '{tt.ArrivalTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}',
                                                        '{tt.InclusionTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}',
                                                        {(tt.IsWarm ? "1" : "0")},
                                                        {(tt.IsDark ? "1" : "0")},
                                                        {tt.Delay.TotalMilliseconds},
                                                        {tt.ArrivalIndex},
                                                        {tt.InclusionIndex},
                                                        {tt.ArrivalBlockNumber});";
                            cmd.CommandText = sql;
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("error WriteTxTimeAsync " + e.ToString());
                            }
                        }
                    }
                    conn.Close();
                }

                sw.Stop();
                Console.WriteLine("tx_time update {0} rows in {1} ms", tts.Count, sw.ElapsedMilliseconds);
            });
        }
    }
}