sui move build

sui client publish .\sources\game_item.move --gas-budget 20000000

sui client call --function mint --module game_item --package 0x6fd3cc49345e4b55c7b28fcf5ca828f75e4f0f2a5268f8bf339ed50dcb7b397f --args 0xdf3b23e9c6efd213e1362bb204f6ee3b0c4d5298d020bc3f273969e075c894cd 0x5d5baf2a10f14f14e31b33ad25f53b08b717c4206ae5fbaa154d1ad9f6dd754f "name5" "desc1" "https://i.imgur.com/OEyexMf.jpeg" --gas-budget 10000000