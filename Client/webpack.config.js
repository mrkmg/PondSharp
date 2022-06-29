const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const CopyPlugin = require('copy-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

module.exports = (env) => {
    const basePath = path.resolve(__dirname, 'bin/', env.release ? "Release" : "Debug", 'net6.0', 'wwwroot');
    const assetsPath = path.resolve(basePath, "assets");
    
    return {
        stats: 'minimal',
        performance: { hints: false },
        cache: {type: 'filesystem'},
        mode: env.release ? "production" : "development",
        devtool: env.release ? 'hidden-source-map' : 'cheap-source-map',
        watchOptions: {
            ignored: ["node_modules/**"]
        },
        plugins: [
            new CleanWebpackPlugin(),
            new HtmlWebpackPlugin({
                template: './index.html',
                filename: '../index.html',
                chunks: ['main', 'vendor', 'monaco', "pixi", 'styles'],
            }),
            new MiniCssExtractPlugin(),
            new CopyPlugin({
                patterns: [
                    { from: "./favicon.ico", to: basePath },
                ]
            }),
        ],
        module: {
            rules: [{
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/
            }, {
                test: /\.css$/,
                use: [
                    MiniCssExtractPlugin.loader, 
                    {loader: "css-loader", options: {sourceMap: true}}
                ]
            }, {
                test: /\.scss$/i,
                use: [
                    MiniCssExtractPlugin.loader,
                    {loader: "css-loader", options: {sourceMap: true}},
                    'resolve-url-loader',
                    {
                        loader: 'sass-loader',
                        options: {
                            sourceMap: true,
                        }
                    },
                ],
            }, {
                test: /\.(ttf|woff|woff2|eot|otf|svg)$/i,
                use: [{
                    loader: 'file-loader',
                    options: {
                        name: '[name].[ext]',
                        outputPath: './fonts',
                        publicPath: './fonts',
                    }
                }]
            }]
        },
        resolve: {
            extensions: ['.ts', '.js', '.tsx', '.scss']
        },
        optimization: {
            minimize: !!env.release,
            splitChunks: {
                chunks: "initial",
                cacheGroups: {
                    vendors: {
                        test: /[\\/]node_modules[\\/]/,
                        name: "vendors",
                        reuseExistingChunk: true,
                        priority: 1,
                    },
                    monaco: {
                        test: /[\\/]node_modules[\\/]monaco-editor[\\/]/,
                        name: "monaco",
                        reuseExistingChunk: true,
                        priority: 2,
                    },
                    pixi: {
                        test: /[\\/]node_modules[\\/]pixi.js[\\/]/,
                        name: "pixi",
                        reuseExistingChunk: true,
                        priority: 2,
                    }
                }
            }
        },
        entry: {
            main: {import: './main.ts'},
            styles: {import: './app.scss'},
            'editor.worker': 'monaco-editor/esm/vs/editor/editor.worker.js',
            'json.worker': 'monaco-editor/esm/vs/language/json/json.worker.js',
            'css.worker': 'monaco-editor/esm/vs/language/css/css.worker.js',
            'html.worker': 'monaco-editor/esm/vs/language/html/html.worker.js',
            'ts.worker': 'monaco-editor/esm/vs/language/typescript/ts.worker.js'
        },
        output: {
            globalObject: 'self',
            path: assetsPath,
            publicPath: "./assets/"
        }
    }
};