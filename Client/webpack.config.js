const path = require('path');
module.exports = {
    devtool: 'inline-source-map',
    module: {
        rules: [{
            test: /\.tsx?$/,
            use: 'ts-loader',
            exclude: /node_modules/
        }, {
            test: /\.css$/i,
            use: ['style-loader', 'css-loader'],
        }, {
            test: /\.ttf$/i,
            use: [{
                loader: 'file-loader',
                options: {
                    name: '[name].[ext]',
                    outputPath: './ttf'
                }
            }]
        }]
    },
    resolve: {
        extensions: ['.ts', '.js', '.tsx']
    },
    optimization: {
        minimize: false,
    },
    entry: './ts/main.ts',
    output: {
        filename: 'main.js',
        path: path.resolve(__dirname, 'wwwroot/js'),
        publicPath: "/js/"
    }
};