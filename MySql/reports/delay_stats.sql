-- statistical analysis of inclusion delay times by block count and time (avg and stdev)
select  min(eth_tx.arrival_time) as from_time,
        max(eth_tx.arrival_time) as to_time,
        count(*) as tx_count,
        avg(eth_tx.block_number - eth_tx.arrival_block) as avg_inclusion_delay_blocks,
        stddev(eth_tx.block_number - eth_tx.arrival_block) as stdev_inclusion_delay_blocks,
        avg(eth_tx.delay_ms) as avg_inclusion_delay_ms,
        stddev(eth_tx.delay_ms) as stdev_inclusion_delay_ms  
from tx_time.eth_tx 
where is_warm = 1 and eth_tx.arrival_block > 0 and eth_tx.block_number > 0;

-- statistical analysis of inclusion delay times for mempool only txs by block count and time (avg and stdev)
select  min(eth_tx.arrival_time) as from_time,
        max(eth_tx.arrival_time) as to_time,
        count(*) as tx_count,
        avg(eth_tx.block_number - eth_tx.arrival_block) as avg_inclusion_delay_blocks,
        stddev(eth_tx.block_number - eth_tx.arrival_block) as stdev_inclusion_delay_blocks,
        avg(eth_tx.delay_ms) as avg_inclusion_delay_ms,
        stddev(eth_tx.delay_ms) as stdev_inclusion_delay_ms  
from tx_time.eth_tx 
where is_warm = 1 and is_dark = 0 and delay_ms > 0 and eth_tx.arrival_block > 0 and eth_tx.block_number > 0;
