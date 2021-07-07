# Eth Include

Ethereum transaction arrival and inclusion time data extraction and reporting allowing for limited MEV type classification.

## Description

This project was created to provide supporting data for my [Plain Alex](https://github.com/pmcgoohan/targeting-zero-mev/blob/main/README.md) content layer solution and [my presentation](https://www.youtube.com/watch?v=zf2l3veT9EI&t=114s) at the EthGlobal / HackMoney / MEV.wtf summit.

It allows for the collection of tx metadata such as arrival time in the mempool and subsequent inclusion time in the Ethereum blockchain.

In doing so, it allows for the broad categorization of [Flashbots MEVA bundles](https://flashbots-explorer.marto.lol/) into the MEV types: frontrunning, backrunning, sandwiching and other.

## MEV Categorization Methodology

MEV type categorization can be performed by examining the relative position of dark txs (those added at block creation by the miner) and lit txs (those than arrive through the mempool) in individual MEVA bundles.

|MEV type|Identifying features|
|---|---|
|sandwich attack|dark first and last txs, lit middle txs|
|frontrun attack|dark first tx, lit final txs|
|backrun attack|lit first txs, dark final tx|
|other|none of the above, including single tx bundles|

### Limitations

Because categorization relies on arrival/inclusion time metadata and not the deep parsing of tx contents, some bundles may be misclassified.

Informal spot checks suggest it does a pretty good job, but the technique would benefit from greater auditing.

## Getting Started

* Build the MySql tx_time database using the MySql/create_db_tx_time scripts.
* Set appsettings.json MySqlConnection to point to this db
* Set appsettings.json WssNodePath to point to an Ethereum Node web socket (eg: Infura)
* Compile and run the EthInclude.csproj Console app
* Enter the number of pending txs in the system (typically around 150000. For accurate arrival times, ignore these early txs when reporting)
* Collect data for as long as you want (note: disconnections are not gracefully handled at the moment)
* Run MySql/reports/update_bundles_nblock.sql against the db to update reporting fields
* Run sql reports against it (see MySql/reports for examples)

### Dependencies

* .Net Core and IDE (Visual Studio 2019 / Visual Studio Code)
* MySql
* Ethereum node (eg: [Infura](https://infura.io/))
* [Flashbots API](https://blocks.flashbots.net/)

## Sample Data

This is the [sample data set](https://drive.google.com/file/d/1WPknOb-Y3jIGaNc-2wA3VuWkUjXxuS8O) used in MEV type categorization in [my talk](https://www.youtube.com/watch?v=zf2l3veT9EI&t=114s).

## Authors

P mcgoohan

Find me on 
[Twitter](https://twitter.com/pmcgoohanCrypto), 
[Medium](https://pmcgoohan.medium.com), 
[Github](https://github.com/pmcgoohan), 
[Eth.Research](https://ethresear.ch/u/pmcgoohan), 
[Reddit (predicting MEV in 2014)](https://www.reddit.com/r/ethereum/comments/2d84yv/miners_frontrunning)

## License

This project is licensed under the MIT License - see the LICENSE.md file for details

## Acknowledgments

* [Infura](https://infura.io/)
* [Flashbots API](https://blocks.flashbots.net/)
