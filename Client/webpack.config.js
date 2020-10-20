const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

module.exports = {
    devtool: 'cheap-source-map',
    watchOptions: {
        ignored: ["node_modules/**"]
    },
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
        splitChunks: {
            chunks: "all",
            cacheGroups: {
                vendors: {
                    test: /[\\/]node_modules[\\/]/,
                    name: "vendors"
                }
            }
        }
    },
    plugins: [new CleanWebpackPlugin()],
    entry: {
        main: {import: './ts/main.ts'},
    },
    output: {
        path: path.resolve(__dirname, 'wwwroot/js'),
        publicPath: "js/"
    }
};