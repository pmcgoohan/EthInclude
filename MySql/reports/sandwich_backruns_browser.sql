-- sandwich
SELECT eth_tx.block_number,eth_tx.arrival_block,eth_tx.delay_ms,fb_bundle.*,fb_tx.* FROM fb_bundle,fb_tx,eth_tx
where not_dark_count > 0 and is_first_dark = 1 and is_last_dark = 1 and min_delay_ms > 0
and fb_bundle.bundle = fb_tx.bundle and fb_bundle.block_number = fb_tx.block_number 
and eth_tx.hash = fb_tx.hash
order by fb_bundle.block_number desc, fb_bundle.bundle asc;

-- backrun
SELECT eth_tx.block_number,eth_tx.arrival_block,eth_tx.delay_ms,fb_bundle.*,fb_tx.* FROM fb_bundle,fb_tx,eth_tx
where not_dark_count > 0 and is_first_dark = 0 and is_last_dark = 1 and min_delay_ms > 0
and fb_bundle.bundle = fb_tx.bundle and fb_bundle.block_number = fb_tx.block_number 
and eth_tx.hash = fb_tx.hash
order by fb_bundle.block_number desc, fb_bundle.bundle asc;
