#[allow(unused_use)]
module {{toLowerCase Name}}_package::{{toLowerCase Name}} {

    use sui::url::{Self, Url};
    use std::string::{Self, String, utf8};
    use sui::tx_context::{sender};
    use sui::package;
    use sui::display;
    use std::vector as vec;

    /// Ensure NFT metadata (Attributes) vector properties length
    const EVecLengthMismatch: u64 = 1;

    /// Type that marks Capability to create new item
    public struct AdminCap has key { id: UID }

    /// NFT one-time witness (guaranteed to have at most one instance), name matches the module name
    public struct {{toUpperCase Name}} has drop {}

    /// NFT metadata
    public struct Attribute has store, copy, drop {
        name: String,
        value: String
    }

    /// NFT Struct
    public struct {{toStructName Name}} has key, store {
        id: UID,
        name: String,
        description: String,
        url: Url,
        contentId: String,
        attributes: vector<Attribute>
    }

    /// Called on contract publish, defines NFT display properties
    fun init(otw: {{toUpperCase Name}}, ctx: &mut TxContext) {
        let keys = vector[
            utf8(b"name"),
            utf8(b"description"),
            utf8(b"url")
        ];
        let values = vector[
            utf8(b"{name}"),
            utf8(b"{description}"),
            utf8(b"{url}")
        ];

        let publisher = package::claim(otw, ctx);
        let mut display = display::new_with_fields<{{toStructName Name}}>(&publisher, keys, values, ctx);
        display::update_version(&mut display);
        transfer::public_transfer(publisher, sender(ctx));
        transfer::public_transfer(display, sender(ctx));
        transfer::transfer(AdminCap {id: object::new(ctx)}, tx_context::sender(ctx));
    }

    /// Constructs NFT object
    fun new(
        name: vector<u8>,
        description: vector<u8>,
        url: vector<u8>,
        contentId: vector<u8>,
        names: vector<String>,
        values: vector<String>,
        ctx: &mut TxContext): {{toStructName Name}} {
        let len = vec::length(&names);
        assert!(len == vec::length(&values), EVecLengthMismatch);

        let mut item = {{toStructName Name}} {
            id: object::new(ctx),
            name: string::utf8(name),
            description: string::utf8(description),
            url: url::new_unsafe_from_bytes(url),
            contentId: string::utf8(contentId),
            attributes: vec::empty()
        };

        let mut i = 0;
        while (i < len) {
            vec::push_back(&mut item.attributes, Attribute {
                name: *vec::borrow(&names, i),
                value: *vec::borrow(&values, i)
            });
            i = i + 1;
        };
        item
    }

    /// NFT mint function
    public entry fun mint(
        _: &AdminCap,
        wallet: address,
        name: vector<u8>,
        description: vector<u8>,
        url: vector<u8>,
        contentId: vector<u8>,
        names: vector<String>,
        values: vector<String>,
        ctx: &mut TxContext) {
        let item = new(name,description,url,contentId,names,values,ctx);
        transfer::transfer(item, wallet);
    }
}