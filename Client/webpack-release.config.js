const devConfig = require("./webpack.config");

devConfig.devtool = "hidden-source-map";
devConfig.optimization.minimize = true;

module.exports = devConfig;