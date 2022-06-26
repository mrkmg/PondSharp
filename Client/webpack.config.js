const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

module.exports = (env) => {
    return {
        performance: { hints: false },
        cache: {type: 'filesystem'},
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
                test: /\.s?css$/i,
                use: ['style-loader', 'css-loader', 'sass-loader'],
            }, {
                test: /\.(ttf|woff|eot|otf|svg)$/i,
                use: [{
                    loader: 'file-loader',
                    options: {
                        name: '[name].[ext]',
                        outputPath: './fonts'
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
            main: {import: './wwwroot-src/ts/main.ts'},
            'editor.worker': 'monaco-editor/esm/vs/editor/editor.worker.js',
            'json.worker': 'monaco-editor/esm/vs/language/json/json.worker',
            'css.worker': 'monaco-editor/esm/vs/language/css/css.worker',
            'html.worker': 'monaco-editor/esm/vs/language/html/html.worker',
            'ts.worker': 'monaco-editor/esm/vs/language/typescript/ts.worker'
        },
        output: {
            globalObject: 'self',
            path: path.resolve(__dirname, 'wwwroot/js'),
            publicPath: "js/"
        }
    }
};