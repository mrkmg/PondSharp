const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

module.exports = (env) => {
    return {
        performance: { hints: false },
        mode: env.release ? "production" : "development",
        devtool: env.release ? 'hidden-source-map' : 'cheap-source-map',
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
            minimize: !!env.release,
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
        plugins: [
            new CleanWebpackPlugin()
        ],
        entry: {
            main: {import: './ts/main.ts'},
        },
        output: {
            path: path.resolve(__dirname, 'wwwroot/js'),
            publicPath: "js/"
        }
    }
};