const path = require('path');

module.exports = {
    entry: {
        bridge: './src/bridge.ts'
    },
    target: 'web',
    module: {
        rules: [
            {
                test: /\.ts$/,
                use: 'ts-loader',
                exclude: /node_modules/,
            },
        ],
    },
    resolve: {
        extensions: ['.ts', '.js'],
    },
    output: {
        filename: '[name].js',
        path: path.resolve(__dirname, 'dist'),
        library: 'WalletBridge',
        libraryTarget: 'window',
        //globalObject: 'this'     

    },
    optimization: {
        minimize: true,
        usedExports: true
    },
    experiments: {
        outputModule: true,
    },
    mode: "production"
};
