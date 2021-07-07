-- produce a distribution of the number of txs at each block inclusion delay
select eth_tx.block_number - eth_tx.arrival_block as block_delay,count(*) as count
from tx_time.eth_tx 
where is_warm = 1 and eth_tx.arrival_block > 0
group by  eth_tx.block_number - eth_tx.arrival_block
order by eth_tx.block_number - eth_tx.arrival_block
