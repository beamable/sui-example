sui move build

sui client publish .\sources\game_item.move --gas-budget 20000000

sui client call --function create --module game_item --package 0x9ace13ead53e084ff933edd6e6f20e8339b036d0c9e06a89ee68a0ec72cafc16 --args 0xd16cf9dba45c68e91005b7910890ee0173e3e39966d755afdf4536a28735b63c 0x5d5baf2a10f14f14e31b33ad25f53b08b717c4206ae5fbaa154d1ad9f6dd754f "name69" "desc1" "https://i.imgur.com/OEyexMf.jpeg" --gas-budget 10000000
