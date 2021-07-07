-- run this on offline data before perfoming analysis

drop table fb_bundle_groupby;
truncate table fb_bundle;

-- create bundle rows from fb_tx
insert into fb_bundle
select block_number,bundle,count(*) as count,0,0,0,0,0,0,0
from fb_tx
group by block_number,bundle
order by block_number asc,bundle asc;

-- update first
update eth_tx,fb_tx,fb_bundle set fb_bundle.is_first_dark = eth_tx.is_dark
where fb_tx.index = 0 and
eth_tx.is_warm = 1 and
eth_tx.hash = fb_tx.hash and
fb_bundle.block_number = fb_tx.block_number and
fb_bundle.bundle = fb_tx.bundle;

-- update last
update eth_tx,fb_tx,fb_bundle set fb_bundle.is_last_dark = eth_tx.is_dark
where fb_tx.index = fb_bundle.count - 1 and
eth_tx.is_warm = 1 and
eth_tx.hash = fb_tx.hash and
fb_bundle.block_number = fb_tx.block_number and
fb_bundle.bundle = fb_tx.bundle;

-- aggregate bundle data
create table fb_bundle_groupby
select fb_tx.block_number,
fb_tx.bundle,
count(*) as not_dark_count,
min(delay_ms) as min_delay_ms,
max(delay_ms) as max_delay_ms,
sum(delay_ms) as sum_delay_ms
from fb_tx, eth_tx
where is_warm = 1 and is_dark = 0 and delay_ms > 0 and eth_tx.hash = fb_tx.hash
group by fb_tx.block_number,fb_tx.bundle;

-- update bundle from aggregate
update fb_bundle, fb_bundle_groupby
set fb_bundle.not_dark_count = fb_bundle_groupby.not_dark_count,
fb_bundle.min_delay_ms = fb_bundle_groupby.min_delay_ms,
fb_bundle.max_delay_ms = fb_bundle_groupby.max_delay_ms,
fb_bundle.sum_delay_ms = fb_bundle_groupby.sum_delay_ms
where fb_bundle.block_number = fb_bundle_groupby.block_number and
fb_bundle.bundle = fb_bundle_groupby.bundle;

-- if any txs in the bundle arrived >n blocks before they are included, set is_within_n false
update fb_bundle set is_within_n = 1;
update fb_bundle, eth_tx, fb_tx set is_within_n = 0
where eth_tx.hash = fb_tx.hash and
fb_bundle.block_number = fb_tx.block_number and
fb_bundle.bundle = fb_tx.bundle and (eth_tx.block_number - eth_tx.arrival_block) > 1;