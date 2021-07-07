-- sandwich, frontrun, backrun reports
-- run update_bundles_nblock first

SELECT 
(SELECT count(*) FROM tx_time.fb_bundle) as total,
(SELECT count(*) FROM tx_time.fb_bundle where not_dark_count > 0 and is_first_dark = 1 and is_last_dark = 1 and min_delay_ms > 0) as sandwich,
(SELECT count(*) FROM tx_time.fb_bundle where not_dark_count > 0 and is_first_dark = 1 and is_last_dark = 0 and min_delay_ms > 0) as frontrun,
(SELECT count(*) FROM tx_time.fb_bundle where not_dark_count > 0 and is_first_dark = 0 and is_last_dark = 1 and min_delay_ms > 0) as backrun,
(SELECT count(*) FROM tx_time.fb_bundle where is_within_n = 1) as total_cl,
(SELECT count(*) FROM tx_time.fb_bundle where is_within_n = 1 and not_dark_count > 0 and is_first_dark = 1 and is_last_dark = 1 and min_delay_ms > 0) as sandwich_cl,
(SELECT count(*) FROM tx_time.fb_bundle where is_within_n = 1 and not_dark_count > 0 and is_first_dark = 1 and is_last_dark = 0 and min_delay_ms > 0) as frontrun_cl,
(SELECT count(*) FROM tx_time.fb_bundle where is_within_n = 1 and not_dark_count > 0 and is_first_dark = 0 and is_last_dark = 1 and min_delay_ms > 0) as backrun_cl