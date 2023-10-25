module beamable_sui_example::game_item {

    use sui::url::{Self, Url};
    use std::string::{Self, String};
    use sui::object::{Self, UID};
    use sui::transfer;
    use sui::tx_context::{Self, TxContext};

    struct GameAdminCap has key { id: UID }

    struct GameItem has key, store {
      id: UID,
      name: String,
      description: String,
      url: Url
    }

    fun init(ctx: &mut TxContext) {
      transfer::transfer(GameAdminCap {id: object::new(ctx)}, tx_context::sender(ctx));
    }

    public entry fun mint(
      _: &GameAdminCap,
      player: address,
      name: vector<u8>,
      description: vector<u8>,
      url: vector<u8>,
      ctx: &mut TxContext
    ) {
      let nft = GameItem {
          id: object::new(ctx),
          name: string::utf8(name),
          description: string::utf8(description),
          url: url::new_unsafe_from_bytes(url)
      };

      transfer::transfer(nft, player);
    }
}