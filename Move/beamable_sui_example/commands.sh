sui move build

sui client publish .\sources\game_item.move --gas-budget 20000000

sui client call --function mint --module game_item --package 0x5e5b7eedd11151019b3ca1339bb21ef276dbf5b5ea8605850227b4f7d773b46d --args 0x00f2a1c8b41c436e45702c2992a3be0e8cc4a0a9b8a813ca870757285a71bbd6 0x55b76278efe36d8fd45b54b3020e32e892691242d3ba4c0d9446611a36f52358 "name1" "desc1" "https://i.imgur.com/OEyexMf.jpeg" --gas-budget 10000000