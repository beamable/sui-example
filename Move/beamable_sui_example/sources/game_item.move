module beamable_sui_example::game_item {

    use sui::url::{Self, Url};
    use std::string::{Self, String, utf8};
    use sui::object::{Self, UID};
    use sui::tx_context::{Self, TxContext, sender};
    use sui::transfer;
    use sui::package;
    use sui::display;
    use std::vector as vec;

    const EVecLengthMismatch: u64 = 1;

    struct GameAdminCap has key { id: UID }

    struct GAME_ITEM has drop {}

    struct Attribute has store, copy, drop {
      name: String,
      value: String
    }

    struct GameItem has key, store {
      id: UID,
      name: String,
      description: String,
      url: Url,
      attributes: vector<Attribute>
    }    

    fun init(otw: GAME_ITEM, ctx: &mut TxContext) {
      let keys = vector[
            utf8(b"name"),
            utf8(b"description"),
            utf8(b"url"),
            ];
      let values = vector[
            utf8(b"{name}"),
            utf8(b"{description}"),
            utf8(b"{url}"),            
            ];           

      let publisher = package::claim(otw, ctx);
      let display = display::new_with_fields<GameItem>(&publisher, keys, values, ctx);
      display::update_version(&mut display);
      transfer::public_transfer(publisher, sender(ctx));
      transfer::public_transfer(display, sender(ctx));
      transfer::transfer(GameAdminCap {id: object::new(ctx)}, tx_context::sender(ctx));
    }

    fun new(name: vector<u8>, description: vector<u8>, url: vector<u8>, names: vector<String>, values: vector<String>, ctx: &mut TxContext): GameItem {
      let len = vec::length(&names);
      assert!(len == vec::length(&values), EVecLengthMismatch);

      let item = GameItem {
        id: object::new(ctx),
        name: string::utf8(name),
        description: string::utf8(description),
        url: url::new_unsafe_from_bytes(url),
        attributes: vec::empty()
      };

      let i = 0;
      while (i < len) {
            vec::push_back(&mut item.attributes, Attribute {
                name: *vec::borrow(&names, i),
                value: *vec::borrow(&values, i)
            });
            i = i + 1;
        };

      item
    }

    public entry fun create(_: &GameAdminCap, player: address, name: vector<u8>, description: vector<u8>, url: vector<u8>, names: vector<String>, values: vector<String>, ctx: &mut TxContext) {
      let game_item = new(name, description, url, names, values, ctx);
      transfer::transfer(game_item, player);
    }
}

module beamable_sui_example::coin_item {
    use std::option;
    use sui::coin;
    use sui::transfer;
    use sui::tx_context::{Self, TxContext};

    struct COIN_ITEM has drop {}

    fun init(otw: COIN_ITEM, ctx: &mut TxContext) {
        let decimal = 0;
        let symbol = b"CI";
        let name = b"Coin item";
        let desc = b"Custom coin item";
        let (treasury, metadata) = coin::create_currency(otw, decimal, symbol, name, desc, option::none(), ctx);
        transfer::public_freeze_object(metadata);
        transfer::public_transfer(treasury, tx_context::sender(ctx))
    }

    public entry fun mint(
        treasury: &mut coin::TreasuryCap<COIN_ITEM>, 
        amount: u64, 
        recipient: address, 
        ctx: &mut TxContext
    ) {
        coin::mint_and_transfer(treasury, amount, recipient, ctx)
    }

    
    public entry fun burn(treasury: &mut coin::TreasuryCap<COIN_ITEM>, coin: coin::Coin<COIN_ITEM>) {
        coin::burn(treasury, coin);
    }
}