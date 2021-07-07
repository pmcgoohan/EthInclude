-- statistical analysis of inclusion delay times (avg and stdev)
select  min(eth_tx.arrival_time) as from_time,
        min(eth_tx.arrival_time) as to_time,
        count(*) as tx_count,avg(eth_tx.delay_ms) as avg_inclusion_delay_ms,
        stddev(eth_tx.delay_ms) as stdev_inclusion_delay_ms 
from tx_time.eth_tx where is_warm = 1
